using System;
using System.Data;
using System.IO;
using L3.Cargo.Common;
using L3.Cargo.Common.Xml.Profile_1_0;
using L3.Cargo.Communications.Interfaces;
using L3.Cargo.Workstation.Common;

namespace L3.Cargo.Workstation.DataSourceCore
{
    public class DataSourceAccess
    {
        #region Private Members

        private CaseSourceManager m_CaseSourceManager;

        #endregion Private Members


        #region Constructors

        public DataSourceAccess (CaseSourceManager caseSourceManager)
        {
            m_CaseSourceManager = caseSourceManager;
            m_CaseSourceManager.StartUp();
        }

        #endregion Constructors


        #region Public Methods

        public void RequestSources (SourceType sourceType, out CaseSourcesList sourceList)
        {
            m_CaseSourceManager.RequestSources(sourceType, out sourceList);
        }

        public LoginResponse Login(String sourceAlias, WorkstationInfo awsInfo)
        {
            return m_CaseSourceManager.Login(sourceAlias, awsInfo);
        }
        
        public void GetCaseList (String sourceAlias, out DataSet CaseList)
        {
            m_CaseSourceManager.GetCaseList(sourceAlias, out CaseList);
        }

        public void UpdateProfile(string sourceAlias, string userName, Profile profile)
        {
            m_CaseSourceManager.UpdateProfile(sourceAlias, userName, profile);
        }

        public void Logout (LogOutInfo logOutInfo)
        {
            m_CaseSourceManager.Logout(logOutInfo);
        }

        public CaseObject RequestCase (String sourceAlias, String caseId, Boolean isEditable)
        {
            return m_CaseSourceManager.RequestCase(sourceAlias, caseId, isEditable);
        }

        public Stream RequestFile (String sourceAlias, String caseId, String filename, FileType filetype)
        {
            return m_CaseSourceManager.RequestFile(sourceAlias, caseId, filename, filetype);
        }

        public void GetManifestList(String sourceAlias, out ObservableCollectionEx<String> manifestList)
        {
            m_CaseSourceManager.GetManifestList(sourceAlias, out manifestList);
        }

        public void UpdateCase(String sourceAlias, String caseId, CaseUpdateEnum type, String filename, Stream file, AttachFileTypeEnum attachFileType,
            WorkstationResult result, String ContainerNum, String UserName, String CreateTime, L3.Cargo.Communications.Interfaces.CaseType caseType)
        {
            m_CaseSourceManager.UpdateCase(sourceAlias, caseId, type, filename, file, attachFileType, result, ContainerNum, UserName, CreateTime, caseType);
        }

        public void Shutdown()
        {
            m_CaseSourceManager.Shutdown();
        }

        public void AutoSelectEnabled(bool enabled)
        {
            m_CaseSourceManager.AutoSelectEnabled(enabled);
        }

        #endregion Public Methods
    }
}
