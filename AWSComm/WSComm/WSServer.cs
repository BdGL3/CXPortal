using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.ServiceModel;
using System.Threading;
using System.Xml.Serialization;
using L3.Cargo.Common;
using L3.Cargo.Common.Xml.Profile_1_0;
using L3.Cargo.Communications.Client;
using L3.Cargo.Communications.Host;
using L3.Cargo.Communications.Interfaces;
using L3.Cargo.Communications.Common;
using System.Windows;
using System.Diagnostics;

namespace L3.Cargo.WSCommunications
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class WSServer : WSComm
    {
        #region Private Members

        private bool m_Shutdown;

        private bool m_EnableArchiveDecision;

        private string m_Alias;

        private string m_ProfilesFolder;

        private object m_ClientListLock;

        private object m_CaseListLock;

        private Thread m_HostConnThread;

        private Thread m_ClientConnThread;

        private NetworkHost m_NetworkHost;

        private CargoHostEndPoint m_CargoHostEndPoint;

        private TIPClientManager m_TIPManagerComm;

        private WSServerCallbacks m_Callbacks;

        private ManualCodingCallback_Impl m_ManualCodingCallback;

        private AnalystCallback_Impl m_AnalystCallback;

        private CaseChangeCallback_Impl m_CaseChangeCallback;

        private class LoadBalanceInfo
        {
            public string workstationId;
            public int numCasesRequested;
            public bool caseRequestedCurrently;
            public bool requestedCaseWithinTimerWindow;

            public LoadBalanceInfo(string wkst, int numcases, bool caseReqested)
            {
                workstationId = wkst;
                numCasesRequested = numcases;
                caseRequestedCurrently = caseReqested;
                requestedCaseWithinTimerWindow = false;
            }

            public LoadBalanceInfo()
            {
                workstationId = null;
                numCasesRequested = 0;
                caseRequestedCurrently = false;
                requestedCaseWithinTimerWindow = false;
            }
        }

        private List<LoadBalanceInfo> _workstationLoadBalanceList;

        private object _workstationLoadListLock;

        private Timer _loadBalanceCheckTimer;

        private bool _loadBalanceTimerSet;

        private object _updateCaseAssignmentLock;

        private string _loadBalanceTimerSelectedWorkstation;

        #endregion Private Members


        #region Public Members

        public TIPClientManager TIPClientManager
        {
            set
            {
                m_TIPManagerComm = value;
            }
        }

        public bool AutoVerifyCaseEnabled;

        public bool AutoVerifyManualCodingCaseEnabled
        {
            get { return AutoVerifyCaseEnabled; }
            set
            {
                AutoVerifyCaseEnabled = value;

                if (value)
                {
                    try
                    {
                        Thread AutoReleaseMCSThread = new Thread(new ThreadStart(AutoReleaseMCSCases));
                        AutoReleaseMCSThread.Start();
                    }
                    catch
                    {
                        //Manual Coding area does not exist, ignore this error.
                    }
                }
            }
        }

        public delegate void HostStatusHandler(bool isConnected);

        public event HostStatusHandler HostStatusEvent;

        public delegate void CaseListUpdateHandler(string caseId, string awsId, string Area, bool additem);

        public event CaseListUpdateHandler CaseListUpdateEvent;

        #endregion Public Members


        #region Constructors

        public WSServer(CargoHostEndPoint cargoHost, string alias, string uri, string allowedIPs, string profileDir, bool enableArchiveDecision) : 
            base()
        {
            m_Shutdown = false;
            m_CargoHostEndPoint = cargoHost;
            m_ProfilesFolder = profileDir;
            m_Alias = alias;
            m_EnableArchiveDecision = enableArchiveDecision;

            m_ClientListLock = new object();
            m_CaseListLock = new object();
            _workstationLoadListLock = new object();
            _updateCaseAssignmentLock = new object();

            m_HostConnThread = new Thread(new ThreadStart(HostConnState));
            m_ClientConnThread = new Thread(new ThreadStart(ClientConnState));

            m_NetworkHost = new NetworkHost(this, new Uri(uri), alias, allowedIPs);
            m_NetworkHost.SendTimeoutMin = int.Parse(ConfigurationManager.AppSettings["WcfTcpBindingSendTimeoutMin"]);
            m_NetworkHost.ReceiveTimeoutMin = int.Parse(ConfigurationManager.AppSettings["WcfTcpBindingReceiveTimeoutMin"]);
            m_Callbacks = new WSServerCallbacks();

            m_ManualCodingCallback = new ManualCodingCallback_Impl(this);
            m_AnalystCallback = new AnalystCallback_Impl(this);
            m_CaseChangeCallback = new CaseChangeCallback_Impl(this);

            _workstationLoadBalanceList = new List<LoadBalanceInfo>();

            _loadBalanceCheckTimer = new Timer(new TimerCallback(LoadBalanceCheckTimerCallback), null, Timeout.Infinite, Timeout.Infinite);
            _loadBalanceTimerSet = false;
            _loadBalanceTimerSelectedWorkstation = string.Empty;

            base.Subscribe(base.caseList);
            base.caseList.StartUpdate = true;
        }

        #endregion Constructors


        #region Private Methods

        private void HostConnState()
        {
            bool IsConnected = false;

            while (!m_Shutdown)
            {
                if (m_CargoHostEndPoint.IsHostAvailable)
                {
                    if (!IsConnected)
                    {
                        base.caseList.List.Clear();

                        Thread.Sleep(500);

                        m_CargoHostEndPoint.AddCaseChangeListener(m_CaseChangeCallback);

                        try
                        {
                            m_CargoHostEndPoint.AddManualCodingComm(m_ManualCodingCallback);
                        }
                        catch { }

                        m_CargoHostEndPoint.AddAnalystWSComm(m_AnalystCallback);

                        foreach (l3.cargo.corba.XCase xc in m_CargoHostEndPoint.GetAllWorkstationAreaCases())
                        {
                            AddToCaseList(xc);
                        }
                    }
                    IsConnected = true;
                }
                else
                {
                    IsConnected = false;
                    base.caseList.List.Clear();
                }

                HostStatusEvent(IsConnected);

                Thread.Sleep(500);
            }
        }

        private void ClientConnState()
        {
            int timeout = int.Parse(ConfigurationManager.AppSettings["PingTimeoutMsec"]);
            TimeSpan disconnectTimeSpan = new TimeSpan(0, 0, 0, 0, timeout);

            while (!m_Shutdown)
            {
                List<String> callbacks = new List<String>();

                lock (m_ClientListLock)
                {
                    foreach (String wsId in m_Callbacks.Keys)
                    {
                        TimeSpan lastPing = DateTime.Now - m_Callbacks[wsId].LastPingTime;
                        if (lastPing > disconnectTimeSpan)
                        {
                            callbacks.Add(wsId);
                        }
                    }
                }

                RemoveCallBacks(callbacks);

                Thread.Sleep(1000);
            }
        }

        private void RemoveCallBacks(List<String> callbacks)
        {
            foreach (String wsId in callbacks)
            {
                lock (_updateCaseAssignmentLock)
                {
                    m_Callbacks.Remove(wsId);
                    base.ClearAssignments(wsId);
                    CaseListUpdateEvent(String.Empty, wsId, String.Empty, false);
                    RemoveLoadBalanceWorkstation(wsId);

                    if (m_TIPManagerComm != null)
                    {
                        m_TIPManagerComm.ClearAssignments(wsId);
                    }
                }
            }
        }

        private void AutoReleaseMCSCases()
        {
            bool createdNew = true;

            Mutex mutex = new Mutex(true, "DisableMCSArea", out createdNew);

            if (createdNew)
            {
                while (!m_Shutdown && AutoVerifyCaseEnabled)
                {
                    try
                    {
                        List<l3.cargo.corba.XCase> xcases =
                            m_CargoHostEndPoint.GetWorkstationAreaCases(l3.cargo.corba.WorkstationArea.MCS);

                        foreach (l3.cargo.corba.XCase xcase in xcases)
                        {
                            string caseId = xcase.getId();
                            if (!base.caseList.IsAssigned(caseId))
                            {
                                m_CargoHostEndPoint.ReleaseCase(caseId);
                            }
                            Thread.Sleep(500);
                        }

                        Thread.Sleep(1000);
                    }
                    catch
                    {
                        break;
                    }
                }

                mutex.Dispose();
            }
        }

        private void LoadBalanceCheckTimerCallback(object param)
        {
            bool firstFoundInfo = false;
            LoadBalanceInfo selectedInfo = null;
            
            lock (_workstationLoadListLock)
            {
                _loadBalanceTimerSet = false;

                //timer expired select the workstation that should get the next case
                foreach (LoadBalanceInfo info in _workstationLoadBalanceList)
                {
                    if (!info.caseRequestedCurrently && info.requestedCaseWithinTimerWindow)
                    {
                        // select workstation that has the least number of cases processed
                        if (!firstFoundInfo)
                        {
                            firstFoundInfo = true;
                            selectedInfo = info;
                            continue;
                        }

                        if (selectedInfo.numCasesRequested > info.numCasesRequested)
                        {
                            selectedInfo = info;
                        }

                        info.requestedCaseWithinTimerWindow = false;
                    }
                }

                if (selectedInfo != null)
                {
                    _loadBalanceTimerSelectedWorkstation = selectedInfo.workstationId;
                }               

            }
        }

        private string GetNextLoadBalanceWorkstationId(string workstation)
        {
            string workstationId = string.Empty;

            //if there are more cases then Auto Select enabled workatations then
            // go ahead and select the case requesting workstations.
            if (_workstationLoadBalanceList.Count > 0)
            {
                if (caseList.List.CaseListTable.Count >= _workstationLoadBalanceList.Count)
                {
                    workstationId = workstation;
                }
                else  //else if there are more workstations than cases then start a timer and log
                //workstations hat request a case within the timer time window.  When the timer
                //expires select a workstation that should get the next case.
                {
                    lock (_workstationLoadListLock)
                    {
                        LoadBalanceInfo foundInfo = FindLoadBalanceInfo(workstation);

                        if (!string.IsNullOrEmpty(foundInfo.workstationId))
                        {
                            if (!string.IsNullOrEmpty(_loadBalanceTimerSelectedWorkstation))
                            {
                                //foundInfo.timerSelected = false;
                                //workstationId = foundInfo.workstationId;
                                workstationId = _loadBalanceTimerSelectedWorkstation;
                            }
                            else
                            {
                                foundInfo.requestedCaseWithinTimerWindow = true;

                                if (!_loadBalanceTimerSet)
                                {
                                    _loadBalanceCheckTimer.Change(2000, Timeout.Infinite);
                                    _loadBalanceTimerSet = true;
                                }                                
                            }
                        }                                                
                    }
                }
            }
            return workstationId;
        }

        private void SetLoadBalancecaseRequestedCurrently(string workstationId, bool currentlyRequested)
        {
            LoadBalanceInfo foundInfo = FindLoadBalanceInfo(workstationId);

            if (!String.IsNullOrEmpty(foundInfo.workstationId))
            {
                lock (_workstationLoadListLock)
                {
                    foundInfo.caseRequestedCurrently = currentlyRequested;
                }
            }
        }

        private void IncrementLoadBalanceNumRequestedCases(string workstationId, bool currentlyRequested)
        {
            LoadBalanceInfo foundInfo = FindLoadBalanceInfo(workstationId);

            if (!String.IsNullOrEmpty(foundInfo.workstationId))
            {
                lock (_workstationLoadListLock)
                {
                    foundInfo.numCasesRequested++;
                    foundInfo.caseRequestedCurrently = currentlyRequested;

                    if (foundInfo.workstationId == _loadBalanceTimerSelectedWorkstation)
                    {
                        _loadBalanceTimerSelectedWorkstation = string.Empty;
                    }
                }
            }
        }

        private void RemoveLoadBalanceWorkstation(string workstationId)
        {
            lock (_workstationLoadListLock)
            {
                LoadBalanceInfo foundInfo = FindLoadBalanceInfo(workstationId);

                if (!String.IsNullOrEmpty(foundInfo.workstationId))
                {
                    _workstationLoadBalanceList.Remove(foundInfo);

                    if (foundInfo.workstationId == _loadBalanceTimerSelectedWorkstation)
                    {
                        _loadBalanceTimerSelectedWorkstation = string.Empty;
                    }
                }
            }
        }

        private void AddLoadBalanceWorkstation(string workstationId)
        {
            lock (_workstationLoadListLock)
            {
                LoadBalanceInfo info = FindLoadBalanceInfo(workstationId);

                //if the workstation entry exist already then
                //reset its number of requested cases paramter
                if (String.IsNullOrEmpty(info.workstationId))
                {
                    info.workstationId = workstationId;
                    _workstationLoadBalanceList.Add(info);
                }

                foreach (LoadBalanceInfo lInfo in _workstationLoadBalanceList)
                {
                    lInfo.numCasesRequested = 0;
                }
            }
        }

        private LoadBalanceInfo FindLoadBalanceInfo(string workstationId)
        {
            LoadBalanceInfo foundInfo = new LoadBalanceInfo();

            foreach (LoadBalanceInfo info in _workstationLoadBalanceList)
            {
                if (info.workstationId == workstationId)
                {
                    foundInfo = info;
                    break;
                }
            }

            return foundInfo;
        }

        #endregion Private Methods


        #region Public Methods

        public void StartUp()
        {
            try
            {
                string uri = ConfigurationManager.AppSettings["WcfAnnouncementConnectionUri"];
                int timeout = int.Parse(ConfigurationManager.AppSettings["WcfAnnouncementFrequencyPeriodSec"]);
                bool enable = bool.Parse(ConfigurationManager.AppSettings["EnableWcfAnnouncement"]);

                m_NetworkHost.Open(uri, timeout, enable);
                m_CargoHostEndPoint.Open();
            }
            catch (Exception)
            {
            }
            
            if (!m_ClientConnThread.IsAlive)
            {
                m_ClientConnThread.Start();
            }

            if (!m_HostConnThread.IsAlive)
            {
                m_HostConnThread.Start();
            }
        }

        public void ShutDown()
        {
            m_Shutdown = true;

            if (m_ClientConnThread.IsAlive)
            {
                m_ClientConnThread.Abort();
                m_ClientConnThread.Join();
            }

            if (m_HostConnThread.IsAlive)
            {
                m_HostConnThread.Abort();
                m_HostConnThread.Join();
            }

            try
            {
                m_CargoHostEndPoint.RemoveCaseChangeListener(m_CaseChangeCallback);
                m_CargoHostEndPoint.RemoveAnalystWSComm(m_AnalystCallback);
                try
                {
                    m_CargoHostEndPoint.RemoveManualCodingComm(m_ManualCodingCallback);
                }
                catch { }
                m_NetworkHost.Close();
                base.IsShuttingDown = true;
                base.Dispose();
            }
            catch (Exception)
            {
            }
        }

        public void AddToCaseList(l3.cargo.corba.XCase xCase)
        {
            base.caseList.Add(xCase.getId(), xCase.getContainerId(), xCase.getConveyanceId(), string.Empty, string.Empty, 0, xCase.getCaseDir(), false,
                WorkstationDecision.Unknown.ToString(), DateTime.Now, false, null, DateTime.Parse(xCase.getCreateTime()), xCase.getCurrentArea(), false, string.Empty, false);

            CaseListUpdateEvent(xCase.getId(), String.Empty, xCase.getCurrentArea(), true);
        }

        public void AddToCaseList(string caseId, string caseDir, string createTime, string area, bool isCTI)
        {            
            DateTime createtime = DateTime.Parse(createTime);

            base.caseList.Add(caseId, caseDir, createtime, area, isCTI);
            CaseListUpdateEvent(caseId, String.Empty, area, true);
        }

        public void ModifyCaseList(l3.cargo.corba.XCase xCase)
        {
            base.caseList.Modify(xCase.getId(), xCase.getContainerId(), xCase.getConveyanceId(), string.Empty, string.Empty, 0, xCase.getCaseDir(), false,
                (Int32) WorkstationDecision.Unknown, DateTime.Now, false, null, DateTime.Parse(xCase.getCreateTime()), xCase.getCurrentArea(), false, string.Empty, false);

            CaseListUpdateEvent(xCase.getId(), String.Empty, xCase.getCurrentArea(), true);
        }

        public void DeleteFromCaseList(l3.cargo.corba.XCase xCase)
        {
            lock (_updateCaseAssignmentLock)
            {
                base.caseList.Delete(xCase.getId());
                CaseListUpdateEvent(xCase.getId(), String.Empty, xCase.getCurrentArea(), false);
            }
        }

        public override void UpdateCaseList(CaseListUpdate listupdate)
        {
            List<String> callbacks = new List<String>();

            lock (m_ClientListLock)
            {
                foreach (String key in m_Callbacks.Keys)
                {
                    try
                    {
                        m_Callbacks[key].Callback.UpdatedCaseList(listupdate);
                    }
                    catch
                    {
                        callbacks.Add(key);
                    }
                }
            }

            RemoveCallBacks(callbacks);
        }

        public void UpdateManifestList(List<String> manifestDelta, Boolean isAddingToList)
        {
            ManifestListUpdateState state = ManifestListUpdateState.Delete;

            if (isAddingToList)
            {
                state = ManifestListUpdateState.Add;
            }

            ManifestListUpdate listUpdate = new ManifestListUpdate(manifestDelta, state);

            List<String> callbacks = new List<String>();

            lock (m_ClientListLock)
            {
                foreach (String awsId in m_Callbacks.Keys)
                {
                    try
                    {
                        m_Callbacks[awsId].Callback.UpdatedManifestList(listUpdate);
                    }
                    catch (Exception)
                    {
                        callbacks.Add(awsId);
                    }
                }
            }

            RemoveCallBacks(callbacks);
        }

        public override LoginResponse Login(WorkstationInfo wsInfo)
        {
            try
            {
                AuthenticationLevel authLvl = AuthenticationLevel.None;

                authLvl = (AuthenticationLevel)m_CargoHostEndPoint.Login(wsInfo.userInfo.UserName, wsInfo.userInfo.Password);

                Profile profile;

                try
                {
                    XmlSerializer mySerializer = new XmlSerializer(typeof(Profile));

                    string profileFile = m_ProfilesFolder + "\\" + wsInfo.userInfo.UserName + ".xml";
                    if (!File.Exists(profileFile))
                    {
                        File.Copy(m_ProfilesFolder + "\\default.xml", profileFile);
                    }

                    using (FileStream fileStream = new FileStream(profileFile, FileMode.Open))
                    {
                        profile = (Profile)mySerializer.Deserialize(fileStream);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ErrorMessages.NO_USER_PROFILE, ex.InnerException);
                }

                SystemConfiguration sysConfig =
                    new SystemConfiguration(m_Alias, m_CargoHostEndPoint.GetMaxManifestPerCase());

                Database db = new Database();
                sysConfig.ContainerDBConnectString = db.GetConnectionStringByDBName(ConfigurationManager.AppSettings["ContainerDBName"]);                
                sysConfig.ContainerRefreshPeriodSeconds = int.Parse(ConfigurationManager.AppSettings["ContainerRefreshPeriodSeconds"]);

                return new LoginResponse(authLvl, sysConfig, profile);
            }
            catch (Exception ex)
            {
                throw new FaultException(new FaultReason(ex.Message));
            }
        }

        public override void Logout(LogOutInfo logOutInfo)
        {
            try
            {
                m_CargoHostEndPoint.LogOut(logOutInfo.WorkstationId);

                lock (_updateCaseAssignmentLock)
                {
                    base.ClearAssignments(logOutInfo.WorkstationId);
                    CaseListUpdateEvent(String.Empty, logOutInfo.WorkstationId, String.Empty, false);
                    RemoveLoadBalanceWorkstation(logOutInfo.WorkstationId);
                }

                if (m_TIPManagerComm != null)
                {
                    m_TIPManagerComm.ClearAssignments(logOutInfo.WorkstationId);
                }
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

        public override void AutoSelectEnabled(bool enabled, string workstationId)
        {
            if (enabled)
            {
                AddLoadBalanceWorkstation(workstationId);
            }
            else
            {
                RemoveLoadBalanceWorkstation(workstationId);
            }
        }

        public override CaseRequestMessageResponse RequestCase(CaseMessage message)
        {
            try
            {
                lock (m_CaseListLock)
                {
                    //if caseId is empty then auto select a case for this workstation
                    if (String.IsNullOrWhiteSpace(message.CaseId))
                    {
                        //for load balancing figure out whether this workstation should get the requested case
                        string workstation = GetNextLoadBalanceWorkstationId(message.WorkstationId);

                        //string[] caseIdList = null;// string.Empty;//GetAssignedCaseID(workstation);

                        //if (caseIdList == null || caseIdList.Length == 0)
                        //{

                            //select next available case from the case list that matches the workstation mode(Analyst or ManualCodig)
                            message.CaseId = GetUnassignedCaseId(message.WorkstationMode);
                        //}
                        //else
                        //{
                        //    message.CaseId = caseIdList[0];
                        //}

                        //case id is empty there are currently no live cases
                        if (String.IsNullOrWhiteSpace(message.CaseId))
                        {
                            throw new FaultException(new FaultReason(ErrorMessages.NO_LIVE_CASE));
                        }
                        else if (workstation != message.WorkstationId)
                        {
                            throw new FaultException(new FaultReason(ErrorMessages.LOAD_BALANCE_DELAY_CASE_REQUEST));
                        }                        
                    }

                    if (IsCaseAvailable(message.CaseId, message.WorkstationId))
                    {
                        CaseRequestMessageResponse response = base.RequestCase(message);

                        string ftiFile = string.Empty;

                        if (m_TIPManagerComm != null)
                        {
                            ftiFile = m_TIPManagerComm.RequestFTIFile(message.WorkstationId);
                        }

                        if (!String.IsNullOrWhiteSpace(ftiFile))
                        {
                            response.AdditionalFiles.Add(FileType.FTIFile, ftiFile);
                            response.caseType = L3.Cargo.Communications.Interfaces.CaseType.FTICase;
                            ClearAssignment(message.CaseId);
                        }
                        else
                        {
                            string caseArea = string.Empty;

                            if (caseList.IsCTI(message.CaseId))
                            {
                                m_TIPManagerComm.AssignCTICase(message.CaseId, message.WorkstationId);
                                response.caseType = L3.Cargo.Communications.Interfaces.CaseType.CTICase;
                                caseArea = l3.cargo.corba.WorkstationArea.AWS.ToString();
                            }
                            else
                            {
                                caseArea = m_CargoHostEndPoint.GetCase(message.CaseId).getCurrentArea();
                                response.caseType = L3.Cargo.Communications.Interfaces.CaseType.LiveCase;
                            }

                            lock (_updateCaseAssignmentLock)
                            {
                                base.AssignId(message.CaseId, message.WorkstationId);
                                CaseListUpdateEvent(message.CaseId, message.WorkstationId, caseArea, true);
                                //Set this workstation has currently requested case and increment number of cases requested                         
                                IncrementLoadBalanceNumRequestedCases(message.WorkstationId, true);
                            }
                        }

                        return response;
                    }
                    else
                    {
                        throw new FaultException(new FaultReason(ErrorMessages.CASE_CURRENTLY_IN_USE));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new FaultException(new FaultReason(ex.Message));
            }            
        }

        public override Stream RequestCaseData(CaseDataInfo caseDataInfo)
        {
            try
            {
                if (caseDataInfo.fileType.Equals(FileType.ManifestFile))
                {
                    string fileDirectory = m_CargoHostEndPoint.GetManifestDir();
                    FileStream stream = new FileStream(Path.Combine(fileDirectory, caseDataInfo.FileName), FileMode.Open);
                    return stream;
                }
                else if (caseDataInfo.fileType.Equals(FileType.FTIFile))
                {
                    FileStream stream = new FileStream(Path.Combine(m_TIPManagerComm.FTIImageDirectory, caseDataInfo.FileName), FileMode.Open);
                    return stream;
                }
                else
                {
                    return base.RequestCaseData(caseDataInfo);
                }
            }
            catch (Exception exp)
            {
                throw new FaultException(new FaultReason(exp.Message));
            }
        }

        public override void UpdateCase(UpdateCaseMessage message)
        {
            if (caseList.AssignedId(message.CaseId) == message.WorkstationId)
            {
                if (message.Type == CaseUpdateEnum.ReleaseCase)
                {
                    try
                    {
                        m_CargoHostEndPoint.ReleaseCase(message.CaseId);
                        SetLoadBalancecaseRequestedCurrently(message.WorkstationId, false);

                        if (m_TIPManagerComm != null)
                        {
                            m_TIPManagerComm.ProcessedCase(message.CaseId);
                        }
                    }
                    catch (Exception exp)
                    {
                        throw new FaultException(new FaultReason(exp.Message));
                    }
                }
                else if (message.Type == CaseUpdateEnum.CloseCase && message.CaseType != L3.Cargo.Communications.Interfaces.CaseType.FTICase)
                {
                    lock (_updateCaseAssignmentLock)
                    {
                        base.ClearAssignment(message.CaseId);
                        CaseListUpdateEvent(message.CaseId, string.Empty, string.Empty, true);

                        if (message.CaseType == L3.Cargo.Communications.Interfaces.CaseType.CTICase)
                        {
                            m_TIPManagerComm.RemoveCTICaseAssignment(message.CaseId);
                            CaseListUpdateEvent(message.CaseId, String.Empty, String.Empty, false);
                            base.caseList.Delete(message.CaseId);
                        }
                    }
                }
                else if (message.Type == CaseUpdateEnum.CloseCase && message.CaseType == L3.Cargo.Communications.Interfaces.CaseType.FTICase)
                {
                    WorkstationResult result = new WorkstationResult();

                    TimeSpan analysisTime = DateTime.Now.Subtract(DateTime.Parse(message.CreateTime));

                    result.AnalysisTime = (uint)analysisTime.TotalSeconds;
                    result.CaseId = message.CaseId;
                    result.CaseType = message.CaseType;
                    result.Comment = string.Empty;
                    result.CreateTime = message.CreateTime;
                    result.Decision = WorkstationDecision.Unknown;
                    result.Reason = WorkstationReason.NotApplicable;
                    result.UserName = message.UserName;
                    result.WorkstationId = message.WorkstationId;
                    m_TIPManagerComm.TipResult(result);
                }
                else if (message.Type == CaseUpdateEnum.Result)
                {
                    try
                    {
                        if (message.CaseType == L3.Cargo.Communications.Interfaces.CaseType.CTICase ||
                            message.CaseType == L3.Cargo.Communications.Interfaces.CaseType.FTICase)
                        {
                            if (message.workstationResult.CaseType == L3.Cargo.Communications.Interfaces.CaseType.CTICase)
                            {
                                lock (_updateCaseAssignmentLock)
                                {
                                    CaseListUpdateEvent(message.CaseId, String.Empty, String.Empty, false);
                                    base.caseList.Delete(message.CaseId);
                                }
                            }

                            m_TIPManagerComm.TipResult(message.workstationResult);
                        }
                        else
                        {
                            l3.cargo.corba.Result result = new l3.cargo.corba.Result();
                            result.mAnalysisTime = message.workstationResult.AnalysisTime.ToString();
                            result.mComment = message.workstationResult.Comment;
                            result.mCreateTime = message.workstationResult.CreateTime;

                            if (message.workstationResult.Decision == WorkstationDecision.Unknown)
                                result.mDecision = l3.cargo.corba.ResultDecision.RESULT_UNKNOWN;
                            else if (message.workstationResult.Decision == WorkstationDecision.Reject)
                                result.mDecision = l3.cargo.corba.ResultDecision.REJECT;
                            else if (message.workstationResult.Decision == WorkstationDecision.Clear)
                                result.mDecision = l3.cargo.corba.ResultDecision.CLEAR;
                            else
                                result.mDecision = l3.cargo.corba.ResultDecision.CAUTION;

                            result.mReason = message.workstationResult.Reason.ToString();
                            result.mStationType = message.workstationResult.WorkstationType;
                            result.mUserName = message.workstationResult.UserName;

                            m_CargoHostEndPoint.SetResult(message.CaseId, result);
                        }
                    }
                    catch (Exception exp)
                    {
                        throw new FaultException(new FaultReason(exp.Message));
                    }
                }
                else if (message.CaseType == L3.Cargo.Communications.Interfaces.CaseType.FTICase || message.CaseType == L3.Cargo.Communications.Interfaces.CaseType.CTICase)
                {
                    //Do Nothing
                }
                else if (message.Type == CaseUpdateEnum.AttachFile)
                {
                    String TempDirectory = ConfigurationManager.AppSettings["hostTempDirectory"];
                    try
                    {
                        if (!Directory.Exists(TempDirectory))
                        {
                            Directory.CreateDirectory(TempDirectory);
                        }

                        using (FileStream stream = new FileStream(Path.Combine(TempDirectory, message.Filename), FileMode.OpenOrCreate))
                        {
                            message.File.CopyTo(stream);
                        }

                        m_CargoHostEndPoint.AddAttachment(message.CaseId, Path.Combine(TempDirectory, message.Filename),
                            message.AttachFileType.ToString(), message.UserName, message.CreateTime);
                    }
                    catch (Exception exp)
                    {
                        throw new FaultException(new FaultReason(exp.Message));
                    }
                }
                else if (message.Type == CaseUpdateEnum.ObjectID)
                {
                    try
                    {
                        m_CargoHostEndPoint.SetContainerID(message.CaseId, message.ObjectId);
                    }
                    catch (Exception exp)
                    {
                        throw new FaultException(new FaultReason(exp.Message));
                    }
                }
            }
        }

        public override void Ping(String wsId)
        {
            m_Callbacks.Add(wsId, OperationContext.Current.GetCallbackChannel<IWSCommCallback>());
        }

        #endregion Methods
    }
}
