using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.Host;
using L3.Cargo.Communications.Interfaces;
using L3.Cargo.Common;
using System.Xml.Serialization;
using System.IO;
using l3.cargo.corba;
using System.ServiceModel;
using System.Threading;
using L3.Cargo.Common.Xml.Profile_1_0;

namespace L3.Cargo.ArchiveCaseServer
{
    public class ACSServer : L3.Cargo.Communications.Host.CaseRequestManager
    {
        private HostComm m_HostComm;
        private string m_ProfilesFolder;

        private Dictionary<string, ICaseRequestManagerCallback> m_WSCallbacks;
        private Dictionary<string, DateTime> m_WSLastPing;

        private object m_CaseListLock = new object();
        private object m_ClientListLock = new object();

        private NetworkHost acsCommHost;

        private bool m_Shutdown = false;

        private Thread ClientConnThread;

        private bool EnableArchiveDecision = false;

        #region Public Members

        public delegate void CaseListUpdateHandler(String caseId, String awsId, Boolean additem);

        public event CaseListUpdateHandler CaseListUpdateEvent;

        #endregion Public Members

        public ACSServer(HostComm hostComm, L3.Cargo.Communications.Common.Logger log, string localCaseListPath, out CaseList caseList)
        {
            string uri = (String)ConfigurationManager.AppSettings["ConnectionUri"];
            string AllowedIPList = (String)ConfigurationManager.AppSettings["AllowedIPList"];

            string path = localCaseListPath;
            string caseListSource = (path == null
                                         ? (String)ConfigurationManager.AppSettings["CaseListSource"]
                                         : "File System"
                                    );
            string Alias = (String)ConfigurationManager.AppSettings["ServerName"];
            m_ProfilesFolder = ConfigurationManager.AppSettings["UserProfiles"];                                  

            bool isReference;

            if (ConfigurationManager.AppSettings["ACSMode"].Equals("Archive"))
            {
                isReference = false;
                if (path == null)
                    path = ConfigurationManager.AppSettings["CaseListFileSystemPath"];
            }
            else
            {
                isReference = true;
                if (path == null)
                    path = ConfigurationManager.AppSettings["ReferenceFileSystemPath"];
            }

            if (caseListSource == "File System")
                caseList = new FSCaseList(log, path, isReference);
            else
                caseList = new DBCaseList(log, path, isReference);

            caseList.configFullSync();
            
            base.Subscribe(caseList);
            base.caseList = caseList;

            bool loginRequired = Boolean.Parse(ConfigurationManager.AppSettings["LoginRequired"]);
            acsCommHost = new NetworkHost(this, new Uri(uri), Alias, AllowedIPList, loginRequired);
            acsCommHost.SendTimeoutMin = int.Parse(ConfigurationManager.AppSettings["WcfTcpBindingSendTimeoutMin"]);
            acsCommHost.ReceiveTimeoutMin = int.Parse(ConfigurationManager.AppSettings["WcfTcpBindingReceiveTimeoutMin"]);

            EnableArchiveDecision = Boolean.Parse(ConfigurationManager.AppSettings["EnableArchiveDecision"]);

            caseList.List.CaseListTable.CaseListTableRowChanged += new CaseListDataSet.CaseListTableRowChangeEventHandler(CaseListTable_RowChanged);
            caseList.List.CaseListTable.CaseListTableRowDeleting += new CaseListDataSet.CaseListTableRowChangeEventHandler(CaseListTable_RowChanged);

            m_HostComm = hostComm;
            m_HostComm.ConnectedToHostEvent += new HostComm.ConnectedToHostHandler(hostComm_ConnectedToHostEvent);            

            m_WSCallbacks = new Dictionary<String, ICaseRequestManagerCallback>();
            m_WSLastPing = new Dictionary<String, DateTime>();

            ClientConnThread = new Thread(new ThreadStart(ClientConnState));
        }

        private void CaseListTable_RowChanged(object sender, CaseListDataSet.CaseListTableRowChangeEvent e)
        {
            try
            {
                if (e.Action == System.Data.DataRowAction.Add)
                {
                    CaseListUpdateEvent(e.Row.CaseId, String.Empty, true);
                }
                else if (e.Action == System.Data.DataRowAction.Delete)
                {
                    CaseListUpdateEvent(e.Row.CaseId, String.Empty, false);
                }
            }
            catch
            {
                throw;
            }
        }
        /*
        private void CaseListTable_RowDeleting(object sender, CaseListDataSet.CaseListTableRowChangeEvent e)
        {
            try
            {
                if (e.Action == System.Data.DataRowAction.Delete)
                {
                    CaseListUpdateEvent(e.Row.CaseId, String.Empty, false);
                }
            }
            catch
            {
                throw;
            }
        }
        */
        public void hostComm_ConnectedToHostEvent(bool Connected)
        {            
        }

        public void StartUp()
        {
            if (CaseListUpdateEvent != null)
            {
                foreach (CaseListDataSet.CaseListTableRow row in base.caseList.List.CaseListTable.Rows)
                {
                    CaseListUpdateEvent(row.CaseId, string.Empty, true);
                }
            }

            if (!ClientConnThread.IsAlive)
            {
                ClientConnThread.Start();
            }

            try
            {
                string uri = ConfigurationManager.AppSettings["WcfAnnouncementConnectionUri"];
                int timeout = int.Parse(ConfigurationManager.AppSettings["WcfAnnouncementFrequencyPeriodSec"]);
                bool enable = bool.Parse(ConfigurationManager.AppSettings["EnableWcfAnnouncement"]);

                acsCommHost.Open(uri, timeout, enable);
            }
            catch (Exception)
            {
            }
        }

        public void ShutDown()
        {
            m_Shutdown = true;

            if (ClientConnThread.IsAlive)
            {
                ClientConnThread.Abort();
                ClientConnThread.Join();
            }

            try
            {
                acsCommHost.Close();
                caseList.Dispose();
                base.IsShuttingDown = true;
                base.Dispose();
            }
            catch (Exception)
            {
            }
        }

        private void ClientConnState()
        {
            while (!m_Shutdown)
            {
                List<String> callbacks = new List<String>();

                lock (m_ClientListLock)
                {
                    foreach (String awsId in m_WSLastPing.Keys)
                    {
                        TimeSpan lastPing = DateTime.Now - m_WSLastPing[awsId];
                        if (lastPing > new TimeSpan(0, 0, 5))
                        {
                            callbacks.Add(awsId);
                        }
                    }
                }

                RemoveCallBacks(callbacks);

                Thread.Sleep(2000);
            }
        }

        public override void UpdateCaseList(CaseListUpdate listupdate)
        {
            List<String> callbacks = new List<String>();

            lock (m_ClientListLock)
            {
                if (m_WSCallbacks != null)
                {
                    foreach (String key in m_WSCallbacks.Keys)
                    {
                        try
                        {
                            m_WSCallbacks[key].UpdatedCaseList(listupdate);
                        }
                        catch
                        {
                            callbacks.Add(key);
                        }
                    }
                }
            }

            RemoveCallBacks(callbacks);
        }

        private void RemoveCallBacks(List<String> callbacks)
        {
            foreach (String wsId in callbacks)
            {
                try
                {
                    lock (m_ClientListLock)
                    {
                        m_WSCallbacks.Remove(wsId);
                        m_WSLastPing.Remove(wsId);

                        if (EnableArchiveDecision == true)
                        {
                            base.caseList.ClearAssignments(wsId);
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        public override CaseRequestMessageResponse RequestCase(CaseMessage message)
        {
            try
            {
                Boolean caseAvailable = false;
                String caseArea = String.Empty;

                if (!String.IsNullOrWhiteSpace(message.CaseId))
                {
                    lock (m_CaseListLock)
                    {
                        try
                        {
                            if (EnableArchiveDecision && message.IsCaseEditable)
                            {
                                caseAvailable = base.IsCaseAvailable(message.CaseId, message.WorkstationId);

                                if (caseAvailable)
                                {
                                    base.AssignId(message.CaseId, message.WorkstationId);
                                }
                            }
                            else
                            {
                                caseAvailable = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }

                    if (caseAvailable)
                    {
                        if(EnableArchiveDecision && message.IsCaseEditable)
                            CaseListUpdateEvent(message.CaseId, message.WorkstationId.ToString(), true);

                        try
                        {
                            return base.RequestCase(message);
                        }
                        catch (Exception ex)
                        {
                            throw new FaultException(new FaultReason(ex.Message));
                        }
                    }
                    else
                    {
                        throw new FaultException(new FaultReason(ErrorMessages.CASE_CURRENTLY_IN_USE));
                    }
                }

                throw new FaultException(new FaultReason(ErrorMessages.CASE_ID_INVALID));
            }
            catch (Exception ex)
            {
                //throw ex;
                throw new FaultException(new FaultReason(ex.Message));
            }
        }

        public override void UpdateCase(UpdateCaseMessage message)
        {

            if ((message.Type == CaseUpdateEnum.CloseCase) ||
                (message.Type == CaseUpdateEnum.ReleaseCase))
            {
                try
                {
                    if (EnableArchiveDecision)
                    {
                        //disassociate any cases assgined to this workstation requesting logout
                        base.ClearAssignment(message.CaseId);
                        CaseListUpdateEvent(message.CaseId, String.Empty, true);
                    }
                }
                catch (CargoException cex)
                {
                    throw new FaultException(new FaultReason(cex.error_msg));
                }
                catch (Exception ex)
                {
                    throw new FaultException(new FaultReason(ex.Message));
                }
            }            
            else
            {
                base.UpdateCase(message);
            }
            
        }

        public override void Ping(String awsId)
        {
            lock (m_ClientListLock)
            {
                if (!m_WSCallbacks.ContainsKey(awsId))
                {
                    m_WSCallbacks.Add(awsId, OperationContext.Current.GetCallbackChannel<ICaseRequestManagerCallback>());
                    m_WSLastPing.Add(awsId, DateTime.Now);
                }
                else
                {
                    m_WSCallbacks[awsId] = OperationContext.Current.GetCallbackChannel<ICaseRequestManagerCallback>();
                    m_WSLastPing[awsId] = DateTime.Now;
                }
            }
        }

        public override LoginResponse Login(WorkstationInfo awsInfo)
        {
            try
            {
                Profile profile;
                XmlSerializer mySerializer = new XmlSerializer(typeof(Profile));

                string profileFile = Path.Combine(m_ProfilesFolder, awsInfo.userInfo.UserName + ".xml");
                if (!File.Exists(profileFile))
                {
                    File.Copy(Path.Combine(m_ProfilesFolder, "default.xml"), profileFile);
                }

                using (FileStream fileStream = new FileStream(profileFile, FileMode.Open))
                {
                    profile = (Profile)mySerializer.Deserialize(fileStream);
                }


                if (Boolean.Parse(ConfigurationManager.AppSettings["TrainingModeEnabled"]))
                {
                    SystemConfiguration sysConfig = new SystemConfiguration(String.Empty, 2);
                    Database db = new Database();
                    sysConfig.ContainerDBConnectString = db.GetConnectionStringByDBName(ConfigurationManager.AppSettings["ContainerDBName"]);
                    sysConfig.ContainerRefreshPeriodSeconds = int.Parse(ConfigurationManager.AppSettings["ContainerRefreshPeriodSeconds"]);

                    return new LoginResponse(L3.Cargo.Communications.Interfaces.AuthenticationLevel.Operator, sysConfig, profile);
                }
                else if (Boolean.Parse(ConfigurationManager.AppSettings["LoginRequired"]))
                {
                    L3.Cargo.Communications.Interfaces.AuthenticationLevel authenticationLevel =
                        L3.Cargo.Communications.Interfaces.AuthenticationLevel.None;

                    authenticationLevel =
                        (L3.Cargo.Communications.Interfaces.AuthenticationLevel)m_HostComm.Login(awsInfo.userInfo.UserName,
                                                                                                      awsInfo.userInfo.Password);

                    SystemConfiguration sysConfig = new SystemConfiguration(String.Empty, m_HostComm.GetMaxManifestPerCase());

                    return new LoginResponse(authenticationLevel, sysConfig, profile);
                }
                
                else
                {
                     SystemConfiguration sysConfig = new SystemConfiguration(String.Empty, 0);

                     return new LoginResponse(L3.Cargo.Communications.Interfaces.AuthenticationLevel.None, sysConfig, null);
                }
                
            }
            catch (CargoException cex)
            {
                throw new FaultException(new FaultReason(cex.error_msg));
            }
            catch (Exception ex)
            {
                throw new FaultException(new FaultReason(ex.Message));
            }
        }

        public override void UpdateProfile(string usersName, Profile profile)
        {
            XmlSerializer mySerializer = new XmlSerializer(typeof(Profile));

            string profileFile = m_ProfilesFolder + "\\" + usersName + ".xml";

            using (FileStream fileStream = new FileStream(profileFile, FileMode.Create))
            {
                mySerializer.Serialize(fileStream, profile);
            }
        }
    }
}
