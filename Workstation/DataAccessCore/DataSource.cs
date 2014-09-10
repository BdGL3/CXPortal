using System;
using L3.Cargo.Workstation.SystemConfigurationCore;

namespace L3.Cargo.Workstation.DataSourceCore
{
    public class DataSource : IDisposable
    {
        private DataSourceAccess m_DataAccessInterface;

        private CaseSourceManager m_CaseSourceManager;

        public DataSourceAccess SourceAccess
        {
            get
            {
                return m_DataAccessInterface;
            }
        }

        public DataSource (SysConfigMgrAccess sysConfigMgrAccess)
        {
            m_CaseSourceManager = new CaseSourceManager(sysConfigMgrAccess);
            m_DataAccessInterface = new DataSourceAccess(m_CaseSourceManager);
        }

        public void Dispose ()
        {
            m_CaseSourceManager.Shutdown();
        }
    }
}
