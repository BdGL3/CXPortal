using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading;
using L3.Cargo.Communications.Common;
using L3.Cargo.Common;
using L3.Cargo.Communications.Interfaces;
using System.Linq;

namespace L3.Cargo.ArchiveCaseServer
{
    public sealed class DBCaseList : CaseList
    {
        private Logger logger;

        private ArchiveDatabase m_ArchiveDB;
        private ContainerDatabase m_ContainerDB;
        private bool m_ContainerDBPresent;


        public DBCaseList (Logger l, string path, bool isReferenceMode) :
            base(l, path, isReferenceMode)
        {
            DateTime StartTime;
            DateTime EndTime;
            try
            {
                logger = l;

                m_ArchiveDB = new ArchiveDatabase();
                base.m_DB = m_ArchiveDB;
                base.m_DB.logger = l;

                m_ContainerDBPresent = bool.Parse(ConfigurationManager.AppSettings["ContainerDBPresent"]);

                if (m_ContainerDBPresent)
                    m_ContainerDB = new ContainerDatabase();

                logger.PrintInfoLine("Populating Case List from database...");
                StartTime = DateTime.Now;
                PopulateCaseList();
                EndTime = DateTime.Now;
                logger.PrintInfoLine("Populating Case List...Done. " + base.List.CaseListTable.Count + " entries, elapsed time: " +
                    (EndTime - StartTime));

                SaveCaseList();
                StartUpdate = true;

                StartMonitoringFileSystem(m_FileSystemLocation);
                //create a thread to ensure dataset populated from xml file is up to date 
                //with individual case.xml files
                m_CheckCaselistThread = new Thread(new ParameterizedThreadStart(delegate { CheckCaseListThreadMethod(false); }));
                m_CheckCaselistThread.Start();
            }
            catch (Exception exp)
            {
                logger.PrintInfoLine("DBCaseList exp: " + exp.Message);
                throw;
            }
        }

        public override void UpdateCaseList (Boolean modifyExistingEntry, string casefile)
        {
            CaseListDataSet ds = new CaseListDataSet();

            try
            {
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

                    base.UpdateCaseList(modifyExistingEntry, casefile);
                }
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        public override void PopulateCaseList()
        {           
            CaseListDataSet ds = new CaseListDataSet();

            try
            {
                // get all entries from the database
                ds = m_ArchiveDB.getAllEntries(0);
                List.Merge(ds);

            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
