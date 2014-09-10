using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.ServiceModel;
using System.Threading;
using L3.Cargo.Common;
using L3.Cargo.Common.Xml.Profile_1_0;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.Interfaces;
using L3.Cargo.Translators;

namespace L3.Cargo.Communications.Host
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class CaseRequestManager : IDisposable, IObserver<CaseList>, ICaseRequestManager
    {
        private Dictionary<String, ICaseRequestManagerCallback> connectionList = new Dictionary<String, ICaseRequestManagerCallback>();
        private Dictionary<String, DateTime> m_ConnectionLastPing = new Dictionary<String, DateTime>();
        private CaseList m_CaseList;
        private Logger logger;

        private object m_UpdateCaseListLock = new object();

        public Boolean IsShuttingDown = false;

        private Thread clientConnectionThread;

        private bool _sendCaseListThumbnail;

        public CaseList caseList
        {
            get
            {
                return m_CaseList;
            }
            set
            {
                m_CaseList = value;
            }
        }


        #region IObserver members

        private IDisposable unsubscriber;
        private string instName = string.Empty;

        public virtual string Name
        { get { return this.instName; } }

        public virtual void Subscribe(IObservable<CaseList> provider)
        {
            if (provider != null)
                unsubscriber = provider.Subscribe(this);
        }

        public virtual void OnCompleted()
        {
            if (logger != null)
                logger.PrintLine("The Caselist Tracker has completed updating data to " + this.Name + ".");

            this.Unsubscribe();
        }

        public virtual void OnError(Exception e)
        {
            if (logger != null)
                logger.PrintLine(this.Name + ": The Caselist cannot be determined.");
        }

        public virtual void OnNext(CaseList caselist)
        {
            DataRowVersion rowVersion;
            CaseListDataSet List;
            CaseListUpdateState caseListState;

            if ((List = (CaseListDataSet)caselist.List.GetChanges(DataRowState.Deleted)) != null)
            {
                caseListState = CaseListUpdateState.Delete;
                rowVersion = DataRowVersion.Original;
            }
            else if ((List = (CaseListDataSet)caselist.List.GetChanges(DataRowState.Modified)) != null)
            {
                caseListState = CaseListUpdateState.Modify;
                rowVersion = DataRowVersion.Current;
            }
            else if ((List = (CaseListDataSet)caselist.List.GetChanges(DataRowState.Added)) != null)
            {
                caseListState = CaseListUpdateState.Add;
                rowVersion = DataRowVersion.Current;
            }
            else
                return;

            if (caseListState != CaseListUpdateState.Delete && !_sendCaseListThumbnail)
            {
                foreach (CaseListDataSet.CaseListTableRow row in caselist.List.CaseListTable.Rows)
                {
                    row.SetImageNull();
                }
            }

            if (logger != null)
            {
                logger.Print("caselist " + caseListState + " :");

                foreach (DataRow row in List.CaseListTable.Rows)
                {
                    foreach (DataColumn col in List.CaseListTable.Columns)
                    {
                        logger.Print(row[col, rowVersion] + " ");
                    }
                    logger.PrintLine();
                }
            }

            CaseListUpdate listupdate = new CaseListUpdate(List, caseListState);
            UpdateCaseList(listupdate);
        }

        public virtual void Unsubscribe()
        {
            unsubscriber.Dispose();
        }
        #endregion

        public CaseRequestManager()
        {
            string generateThumbnail = ConfigurationManager.AppSettings["GenerateThumbnail"];
            string sendCaseListThumbnail = ConfigurationManager.AppSettings["SendCaseListThumbnail"];

            m_CaseList = new CaseList();

            if (generateThumbnail != null)
                m_CaseList.GenerateThumbnail = bool.Parse(generateThumbnail);
            else
                m_CaseList.GenerateThumbnail = true;

            if (sendCaseListThumbnail != null)
                _sendCaseListThumbnail = bool.Parse(sendCaseListThumbnail);
            else
                _sendCaseListThumbnail = true;
        }

        public CaseRequestManager(Logger l = null)
        {
            string path = ConfigurationManager.AppSettings["CaseListFileSystemPath"];
            string generateThumbnail = ConfigurationManager.AppSettings["GenerateThumbnail"];
            string sendCaseListThumbnail = ConfigurationManager.AppSettings["SendCaseListThumbnail"];

            m_CaseList = new CaseList(l, path, false);

            if (generateThumbnail != null)
                m_CaseList.GenerateThumbnail = bool.Parse(generateThumbnail);
            else
                m_CaseList.GenerateThumbnail = true;

            if (sendCaseListThumbnail != null)
                _sendCaseListThumbnail = bool.Parse(sendCaseListThumbnail);
            else
                _sendCaseListThumbnail = true;

            logger = l;
        }

        public virtual CaseListDataSet RequestCaseList(String AwsID)
        {
            try
            {
                if (!connectionList.ContainsKey(AwsID))
                {
                    connectionList.Add(AwsID, OperationContext.Current.GetCallbackChannel<ICaseRequestManagerCallback>());
                }
                else
                {
                    connectionList[AwsID] = OperationContext.Current.GetCallbackChannel<ICaseRequestManagerCallback>();
                }

                if (logger != null)
                {
                    logger.PrintInfoLine(AwsID + " requested case list");
                }

                Thread thread = new Thread(new ParameterizedThreadStart(delegate { SendCaseList(AwsID, caseList); }));
                thread.Start();

                return new CaseListDataSet();
            }
            catch (Exception exp)
            {
                if (logger != null)
                    logger.PrintInfoLine("RequestCaseList exp: " + exp);
                throw new FaultException(new FaultReason(exp.Message));
            }
        }

        int MaxNumCasesPerBatch = int.Parse(ConfigurationManager.AppSettings["MaxNumCasesPerBatch"]);

        public void SendCaseList(String awsID, CaseList caselist)
        {
            int CaseListCount = caselist.List.CaseListTable.Count;

            int currentRowIndex = 0;
            CaseListDataSet.CaseListTableRow CurrentRow = null;

            if (CaseListCount > 0)
            {
                CurrentRow = (CaseListDataSet.CaseListTableRow)caselist.List.CaseListTable.Rows[currentRowIndex];
            }

            while (CaseListCount > 0)
            {
                CaseListDataSet dataset = new CaseListDataSet();
                int MaxIndexValue;

                //lock caselist
                lock (caselist.CaseListLock)
                {
                    currentRowIndex = caselist.List.CaseListTable.Rows.IndexOf(CurrentRow);

                    if (CaseListCount > MaxNumCasesPerBatch)
                    {
                        MaxIndexValue = currentRowIndex + MaxNumCasesPerBatch;
                    }
                    else
                    {
                        MaxIndexValue = currentRowIndex + CaseListCount;
                    }

                    for (int i = currentRowIndex; i < MaxIndexValue; i++)
                    {
                        CaseListDataSet.CaseListTableRow row = (CaseListDataSet.CaseListTableRow)caselist.List.CaseListTable.Rows[i];

                        dataset.CaseListTable.AddCaseListTableRow(row.CaseId, row.AnalystComment, row.ObjectId, row.FlightNumber,
                            row.Analyst, row.CaseDirectory, row.ReferenceImage, row.Result,
                            row.UpdateTime, row.Archived, row.AnalysisTime, row.CreateTime, row.Area, row.Image, row.CTI, row.AssignedId, row.DFCMatch);
                    }

                    if (CaseListCount > MaxNumCasesPerBatch)
                    {
                        CaseListCount = CaseListCount - MaxNumCasesPerBatch;
                        currentRowIndex = currentRowIndex + MaxNumCasesPerBatch;
                        CurrentRow = (CaseListDataSet.CaseListTableRow)caselist.List.CaseListTable.Rows[currentRowIndex];
                    }
                    else
                    {
                        CaseListCount = CaseListCount - CaseListCount;
                    }

                    //unlock caselist                    
                }

                try
                {
                    CaseListUpdate listupdate = new CaseListUpdate(dataset, CaseListUpdateState.Add);
                    ICaseRequestManagerCallback callback = connectionList[awsID];

                    lock (m_UpdateCaseListLock)
                    {
                        callback.UpdatedCaseList(listupdate);
                    }
                }
                catch (Exception exp)
                {
                    if (logger != null)
                    {
                        logger.PrintLine("SendCaseList exp: " + exp);
                    }
                    connectionList.Remove(awsID);
                    break;
                }

            }

            if (logger != null)
            {
                logger.PrintInfoLine("SendCaseList in chunks to " + awsID + " Done.");
            }

        }

        public virtual CaseRequestMessageResponse RequestCase(CaseMessage message)
        {
            CaseRequestMessageResponse response = new CaseRequestMessageResponse();
            response.caseType = Interfaces.CaseType.ArchiveCase;
            response.IsResultEnabled = bool.Parse(ConfigurationManager.AppSettings["EnableArchiveDecision"]);

            try
            {
                // search for case.xml file using the CaseList
                string caseDirectory = m_CaseList.GetCaseDirectory(message.CaseId);
                FileStream fs = File.OpenRead(caseDirectory + "\\case.xml");
                response.file = new MemoryStream();
                CaseTranslator.TranslateToCase2_0(fs, ref response.file, caseDirectory);
                if (File.Exists(caseDirectory + "\\AnalysisHistory.xml"))
                {
                    response.AdditionalFiles.Add(FileType.None, "AnalysisHistory.xml");
                }

                AssignId(message.CaseId, message.WorkstationId);
            }
            catch (Exception exp)
            {
                if (logger != null)
                    logger.PrintInfoLine("RequestCase exp: " + exp.Message);

                throw new FaultException(new FaultReason(exp.Message));
            }

            OperationContext clientContext = OperationContext.Current;
            clientContext.OperationCompleted += new EventHandler(delegate(object sender, EventArgs e)
            {
                if (response.file != null)
                    response.file.Dispose();
            });

            return response;

        }

        public virtual Stream RequestCaseData(CaseDataInfo caseDataInfo)
        {
            try
            {
                string caseDirectory = m_CaseList.GetCaseDirectory(caseDataInfo.CaseId);
                FileStream stream = new FileStream(caseDirectory + "\\" + caseDataInfo.FileName, FileMode.Open);

                OperationContext clientContext = OperationContext.Current;
                clientContext.OperationCompleted += new EventHandler(delegate(object sender, EventArgs e)
                {
                    if (stream != null)
                        stream.Dispose();
                });

                return stream;
            }
            catch (Exception exp)
            {
                if (logger != null)
                    logger.PrintInfoLine("RequestCaseData exp: " + exp.Message);

                throw new FaultException(new FaultReason(exp.Message));
            }
        }

        public virtual void UpdateCase(UpdateCaseMessage message)
        {
            try
            {
                if (message.Type == CaseUpdateEnum.AttachFile)
                {
                    try
                    {
                        caseList.UpdateCaseAddAttachment(message.CaseId, message.File, message.Filename, message.UserName, message.CreateTime, message.AttachFileType.ToString());
                    }
                    catch (Exception exp)
                    {
                        if (logger != null)
                            logger.PrintInfoLine("UpdateCase exp: " + exp.Message);
                        throw new FaultException(new FaultReason(exp.Message));
                    }
                }
                else if (message.Type == CaseUpdateEnum.SetAsReference)
                {
                    caseList.UpdateCaseAsReference(message.CaseId);
                }
                else if (message.Type == CaseUpdateEnum.ObjectID)
                {
                    caseList.updateObjectID(message.CaseId, message.ObjectId, message.CreateTime);
                }
                else if (message.Type == CaseUpdateEnum.CloseCase)
                {
                }
                else if (message.Type == CaseUpdateEnum.Result)
                {
                    caseList.UpdateCaseAddResult(message.workstationResult.CaseId, message.workstationResult.AnalysisTime.ToString("o", CultureResources.getDefaultDisplayCulture()),
                        message.workstationResult.Comment, message.workstationResult.UserName, message.workstationResult.CreateTime,
                        message.workstationResult.Reason.ToString(), message.workstationResult.WorkstationType, message.workstationResult.Decision.ToString());
                }
                else if (message.Type == CaseUpdateEnum.ReleaseCase)
                {
                }
                else
                {
                    if (logger != null)
                        logger.PrintInfoLine("UpdateCase request " + message.Type + " not supported");
                    throw new FaultException(new FaultReason("UpdateCase request " + message.Type + " not supported"));
                }
            }
            catch (Exception exp)
            {
                if (logger != null)
                    logger.PrintInfoLine("UpdateCase request " + message.Type + " not supported");
                throw new FaultException(new FaultReason(exp.Message));
            }

        }

        public virtual void Ping(String awsId)
        {
        }

        public virtual LoginResponse Login(WorkstationInfo awsInfo)
        {
            throw new NotImplementedException(ErrorMessages.INVALID_FUNCTION);
        }

        public virtual void Decision(WorkstationResult awsResult)
        {
            throw new NotImplementedException(ErrorMessages.INVALID_FUNCTION);
        }

        public virtual void Logout(LogOutInfo logOutInfo)
        {
            throw new NotImplementedException(ErrorMessages.INVALID_FUNCTION);
        }

        public virtual void UpdateProfile(string usersName, Profile profile)
        {
            throw new NotImplementedException(ErrorMessages.INVALID_FUNCTION);
        }

        public virtual void AutoSelectEnabled(bool enabled, string workstationId)
        {
            throw new NotImplementedException(ErrorMessages.INVALID_FUNCTION);
        }

        public virtual void UpdateCaseList(CaseListUpdate listupdate)
        {
            try
            {
                lock (m_UpdateCaseListLock)
                {
                    foreach (String awsId in connectionList.Keys)
                    {
                        ICaseRequestManagerCallback callback = connectionList[awsId];
                        try
                        {                           
                            callback.UpdatedCaseList(listupdate);
                        }
                        catch (Exception)
                        {
                            connectionList.Remove(awsId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        protected bool IsCaseAvailable(string caseId, string wsId)
        {
            try
            {
                string assignedId = caseList.AssignedId(caseId);

                return (String.IsNullOrWhiteSpace(assignedId) || assignedId == wsId);
            }
            catch
            {
                throw;
            }
        }

        protected void ClearAssignments(string workstationId)
        {
            caseList.ClearAssignments(workstationId);
        }

        protected void ClearAssignment(string caseId)
        {
            caseList.ClearAssignment(caseId);
        }

        protected void AssignId(string caseId, string AssignmentId)
        {
            caseList.ClearAssignments(AssignmentId);
            caseList.ModifyAssignId(caseId, AssignmentId);
        }

        protected string GetUnassignedCaseId(string workstationMode)
        {
            //query caselist to return first caseid that has an empty assigned id
            //case that has not bee assigned to any workstations.
            return caseList.GetUnassignedCaseId(workstationMode);
        }

        protected string[] GetAssignedCaseID(string workstationId)
        {
            return caseList.GetAssignedCaseID(workstationId);
        }

        #region IDisposable Memebers
        public virtual void Dispose()
        {
            clientConnectionThread.Abort();
        }
        #endregion
    }
}
