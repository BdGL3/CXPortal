using System;
using System.IO;
using System.Threading;
using L3.Cargo.Communications.Common;
using System.Data;
using L3.Cargo.Common;
using System.Configuration;
using L3.Cargo.Communications.Interfaces;

namespace L3.Cargo.ArchiveCaseServer
{
    public sealed class FSCaseList : CaseList
    {
        Logger logger;
        LocalDatabase m_localDB;
        private ContainerDatabase m_ContainerDB;
        private bool m_ContainerDBPresent;

        public FSCaseList (Logger l, string path, bool isReferenceMode) :
            base(l, path, isReferenceMode)
        {
            DateTime StartTime = DateTime.Now;
            DateTime EndTime;
            bool StartFullSync = false;
            try
            {
                logger = l;

                m_localDB = new LocalDatabase(path);
                m_localDB.CreateTable(base.List.CaseListTable);

                base.m_DB = m_localDB;
                base.m_DB.logger = l;

                m_ContainerDBPresent = bool.Parse(ConfigurationManager.AppSettings["ContainerDBPresent"]);

                if (m_ContainerDBPresent)
                    m_ContainerDB = new ContainerDatabase();

                logger.PrintInfoLine("Populating Case List from local database file...");
                if (m_localDB.LoadDataSet(base.List) <= 0)                
                   StartFullSync = true;

                base.SaveCaseList();

                StartMonitoringFileSystem(m_FileSystemLocation);

                //create a thread to ensure dataset populated from xml file is up to date 
                //with individual case.xml files
                m_CheckCaselistThread = new Thread(new ParameterizedThreadStart(delegate { CheckCaseListThreadMethod(StartFullSync); }));
                m_CheckCaselistThread.Start();
            }
            catch (Exception exp)
            {
                logger.PrintInfoLine("FSCaseList exp " + exp.Message);
                logger.PrintLine("Populating Case List from file system...");
                StartTime = DateTime.Now;
                PopulateCaseList();
            }
            finally
            {
                EndTime = DateTime.Now;
                StartUpdate = true;
                logger.PrintInfoLine("Populating Case List...Done. " + base.List.CaseListTable.Count + " entries, elapsed time: " +
                    (EndTime - StartTime));
            }
        }

        public override void UpdateCaseList(Boolean modifyExistingEntry, string casefile)
        {
            CaseListDataSet ds = new CaseListDataSet();

            try
            {
                base.UpdateCaseList(modifyExistingEntry, casefile);

                //if case has been added to the temporary archive and case.xml file exist
                if (/*!modifyExistingEntry && */ casefile.EndsWith("\\case.xml"))
                {
                    if (m_ContainerDBPresent)
                    {
                        String casefilepath = casefile.Substring(0, casefile.LastIndexOf("\\"));
                        String caseFile = casefile.Substring(casefile.LastIndexOf("\\") + 1);
                        CaseObject caseObj = base.GetCaseObj(casefilepath, caseFile);
                        bool DFCMatch = false;

                        //udpate Container DB
                        if (caseObj != null)
                        {
                            if (caseObj.scanInfo != null)
                            {
                                if (caseObj.scanInfo.container != null)
                                {
                                    if (caseObj.scanInfo.container.Id != null)
                                    {
                                        try
                                        {
                                            DataRow Row = m_ContainerDB.query(caseObj.scanInfo.container.Id);

                                            //If case's container Id exist in the container database table
                                            if (Row != null)
                                            {
                                                bool ImageExists = false;

                                                foreach (DataAttachment attach in caseObj.attachments.GetXrayImageAttachments())
                                                {
                                                    if (attach.attachmentId.Contains(".pxe"))
                                                    {
                                                        ImageExists = true;
                                                        break;
                                                    }
                                                }

                                                int SequenceNum = -1;
                                                int BatchNum = -1;


                                                //if case.xml lists sequence number and container DB CheckInSequence value is not listed
                                                //then go ahead and update the db from xml file value.
                                                if (caseObj.scanInfo != null &&
                                                    caseObj.scanInfo.container != null &&
                                                    !String.IsNullOrWhiteSpace(caseObj.scanInfo.container.SequenceNum) &&
                                                    m_ContainerDB.GetSequenceNumber(Row) == 0)
                                                {
                                                    SequenceNum = Convert.ToInt32(caseObj.scanInfo.container.SequenceNum);
                                                }

                                                if (caseObj.scanInfo != null && 
                                                    caseObj.scanInfo.conveyance != null &&
                                                    !String.IsNullOrWhiteSpace(caseObj.scanInfo.conveyance.BatchNum) &&
                                                    m_ContainerDB.GetBatchNumber(Row) == 0)
                                                {
                                                    BatchNum = Convert.ToInt32(caseObj.scanInfo.conveyance.BatchNum);
                                                }

                                                int StatusMajor = 30;

                                                if (caseObj.ResultsList != null && caseObj.ResultsList.Count > 0)
                                                {
                                                    result tempResult = caseObj.ResultsList[0];
                                                    DateTime tempDt = Convert.ToDateTime(tempResult.CreateTime);

                                                    foreach (result res in caseObj.ResultsList)
                                                    {
                                                        DateTime dt = Convert.ToDateTime(res.CreateTime);

                                                        if (dt.CompareTo(tempDt) > 0)
                                                        {                                                       
                                                            tempResult = res;
                                                        }
                                                    }

                                                    if (tempResult.Decision == WorkstationDecision.Clear.ToString())
                                                        StatusMajor = 40;
                                                    else if (tempResult.Decision == WorkstationDecision.Reject.ToString())
                                                        StatusMajor = 60;
                                                    else if (tempResult.Decision == WorkstationDecision.Caution.ToString())
                                                    {
                                                        if (tempResult.Reason == WorkstationReason.NoImage.ToString())
                                                            StatusMajor = 30;
                                                        else
                                                            StatusMajor = 50;
                                                    }
                                                }

                                                int rowsUpdated = 0;

                                                //update container db with statusMajor, ImageExists, SequenceNum and BatchNum
                                                if (SequenceNum > 0 && BatchNum > 0)
                                                {
                                                    rowsUpdated = m_ContainerDB.Update(Row, StatusMajor, ImageExists, SequenceNum, BatchNum);
                                                }
                                                //update container db with statusMajor, ImageExists and SequenceNum
                                                else if (SequenceNum > 0)
                                                {
                                                    rowsUpdated = m_ContainerDB.UpdateStatusImageAndSeq(Row, StatusMajor, ImageExists, SequenceNum);
                                                }
                                                //update container db with statusMajor, ImageExists and Batch
                                                else if (BatchNum > 0)
                                                {
                                                    rowsUpdated = m_ContainerDB.UpdateStatusImageAndBatch(Row, StatusMajor, ImageExists, BatchNum);
                                                }
                                                ////update container db with statusMajor and ImageExists
                                                else
                                                {
                                                    rowsUpdated = m_ContainerDB.Update(Row, StatusMajor, ImageExists);
                                                }

                                                logger.PrintInfoLine("Container DB updated " + rowsUpdated + " row(s), FlightNumber = " + Row["FlightNumber"] + " ULDNumber = " +
                                                    Row["ULDNumber"]);                                        

                                                DFCMatch = true;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            logger.PrintInfoLine(ex.Message);
                                        }
                                    }
                                }
                            }
                        }

                        base.m_isDFCMatch = DFCMatch;
                    }                    
                }
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        public override void PopulateCaseList ()
        {
            try
            {
                //get File system location
                string[] Dirs = Directory.GetDirectories(m_FileSystemLocation);

                foreach (string Dir in Dirs)
                {
                    try
                    {
                        base.UpdateCaseList(false, Path.Combine(Dir, "case.xml"));
                    }
                    catch (Exception exp)
                    {
                        String message = "populateCaseList " + Dir + " exp: " + exp.Message;
                        if (exp.InnerException != null)
                            message += " " + exp.InnerException.Message;
                        logger.PrintInfoLine(message);

                    }
                }

                base.SaveCaseList();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
