using System;
using System.Data;
using L3.Cargo.Common;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.Interfaces;

namespace L3.Cargo.Workstation.DataSourceCore
{
    public class CaseSource<T> : IWSCommCallback, ICaseRequestManagerCallback
        where T : ICaseRequestManager
    {
        #region Private Members

        private T m_EndPoint;

        private object m_CaseListLock;

        private string m_Alias;

        private bool m_IsLoginRequired;

        private CaseListDataSet m_CaseList;

        private ObservableCollectionEx<string> m_ManifestList;

        #endregion Private Members


        #region Public Members

        public T EndPoint
        {
            get
            {
                return m_EndPoint;
            }
            set
            {
                m_EndPoint = value;
            }
        }

        public string Alias
        {
            get
            {
                return m_Alias;
            }
            set
            {
                m_Alias = value;
            }
        }

        public bool IsLoginRequired
        {
            get
            {
                return m_IsLoginRequired;
            }
        }

        public CaseListDataSet CaseList
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

        public object CaseListLock
        {
            get
            {
                return m_CaseListLock;
            }
        }

        #endregion Public Members


        #region Constructors

        public CaseSource (string alias, bool isLoginRequired)
        {
            m_Alias = alias;
            m_IsLoginRequired = isLoginRequired;
            m_CaseListLock = new object();
            m_ManifestList = new ObservableCollectionEx<string>();
        }

        #endregion Constructors


        #region Public Methods

        public void GetManifestList (out ObservableCollectionEx<string> manifestList)
        {
            manifestList = m_ManifestList;
        }

        public void UpdatedCaseList (CaseListUpdate listUpdate)
        {
            if (listUpdate.state == CaseListUpdateState.Add)
            {
                //m_CaseList.Merge(listUpdate.dsCaseList);
                foreach (CaseListDataSet.CaseListTableRow row in listUpdate.dsCaseList.CaseListTable.Rows)
                {
                    try
                    {
                        lock (m_CaseListLock)
                        {
                            m_CaseList.CaseListTable.AddCaseListTableRow(row.CaseId, row.AnalystComment, row.ObjectId, row.FlightNumber,
                                row.Analyst, row.CaseDirectory, row.ReferenceImage, row.Result,
                                row.UpdateTime, row.Archived, row.AnalysisTime, row.CreateTime, row.Area, row.Image, row.CTI, row.AssignedId, row.DFCMatch);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            else if (listUpdate.state == CaseListUpdateState.Delete)
            {
                foreach (DataRow row in listUpdate.dsCaseList.CaseListTable.Rows)
                {
                    String caseId = row[listUpdate.dsCaseList.CaseListTable.CaseIdColumn, DataRowVersion.Original].ToString();
                    Boolean IsReference = (Boolean)row[listUpdate.dsCaseList.CaseListTable.ReferenceImageColumn, DataRowVersion.Original];
                    DataRow foundRow = m_CaseList.CaseListTable.FindByCaseIdReferenceImage(caseId, IsReference);
                    if (foundRow != null)
                    {
                        foundRow.Delete();
                    }
                }
            }
            else if (listUpdate.state == CaseListUpdateState.Modify)
            {
                foreach (CaseListDataSet.CaseListTableRow row in listUpdate.dsCaseList.CaseListTable.Rows)
                {
                    String caseId = row[listUpdate.dsCaseList.CaseListTable.CaseIdColumn].ToString();
                    Boolean IsReference = (Boolean)row[listUpdate.dsCaseList.CaseListTable.ReferenceImageColumn];
                    CaseListDataSet.CaseListTableRow foundRow = m_CaseList.CaseListTable.FindByCaseIdReferenceImage(caseId, IsReference);
                    if (foundRow != null)
                    {
                        foundRow.BeginEdit();

                        if (foundRow.ObjectId != row.ObjectId && !String.IsNullOrWhiteSpace(row.ObjectId))
                            foundRow.ObjectId = row.ObjectId;

                        if (foundRow.FlightNumber != row.FlightNumber && !String.IsNullOrWhiteSpace(row.FlightNumber))
                            foundRow.FlightNumber = row.FlightNumber;

                        if (foundRow.Analyst != row.Analyst && !String.IsNullOrWhiteSpace(row.Analyst))
                            foundRow.Analyst = row.Analyst;

                        if (foundRow.AnalystComment != row.AnalystComment && !String.IsNullOrWhiteSpace(row.AnalystComment))
                            foundRow.AnalystComment = row.AnalystComment;

                        if (foundRow.AnalysisTime != row.AnalysisTime)
                            foundRow.AnalysisTime = row.AnalysisTime;

                        if (foundRow.CaseDirectory != row.CaseDirectory && !String.IsNullOrWhiteSpace(row.CaseDirectory))
                            foundRow.CaseDirectory = row.CaseDirectory;

                        if (foundRow.Result != row.Result)
                            foundRow.Result = row.Result;

                        if (foundRow.UpdateTime != row.UpdateTime && row.UpdateTime != null)
                            foundRow.UpdateTime = row.UpdateTime;

                        if (foundRow.Archived != row.Archived)
                            foundRow.Archived = row.Archived;

                        if (foundRow.Image != row.Image && row.Image != null)
                            foundRow.Image = row.Image;

                        if (foundRow.CreateTime != row.CreateTime)
                            foundRow.CreateTime = row.CreateTime;

                        if (foundRow.Area != row.Area && !String.IsNullOrWhiteSpace(row.Area))
                            foundRow.Area = row.Area;

                        if (foundRow.CTI != row.CTI)
                            foundRow.CTI = row.CTI;

                        if (foundRow.AssignedId != row.AssignedId && row.AssignedId != null)
                            foundRow.AssignedId = row.AssignedId;

                        if (foundRow.DFCMatch != row.DFCMatch)
                            foundRow.DFCMatch = row.DFCMatch;

                        foundRow.EndEdit();
                    }
                }
            }
        }

        public void UpdatedManifestList (ManifestListUpdate listUpdate)
        {
            if (listUpdate.State == ManifestListUpdateState.Add)
            {
                foreach (String manifestItem in listUpdate.List)
                {
                    m_ManifestList.Add(manifestItem);
                }
            }
            else if (listUpdate.State == ManifestListUpdateState.Delete)
            {
                foreach (String manifest in listUpdate.List)
                {
                    m_ManifestList.Remove(manifest);
                }
            }

            //TODO: Fire Event Here
        }

        #endregion Public Methods
    }
}
