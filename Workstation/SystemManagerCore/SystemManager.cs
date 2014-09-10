using L3.Cargo.Workstation.CaseHandlerCore;
using L3.Cargo.Workstation.DataSourceCore;
using L3.Cargo.Workstation.SystemConfigurationCore;

namespace L3.Cargo.Workstation.SystemManagerCore
{
    public class SystemManager
    {
        #region Public Members

        public SystemManagerAccess SysMgrAccess;

        #endregion Public Members


        #region Constructors

        public SystemManager (SysConfigMgrAccess sysConfig, DataSourceAccess dataSourceAccess)
        {
            //create case handler with IDAL
            CaseHandler m_caseHandler = new CaseHandler(sysConfig, dataSourceAccess);

            //create system mode manager with ISysConfig and IDAL and case handler
            SysModeManager m_sysModeMgr = new SysModeManager(sysConfig, dataSourceAccess, m_caseHandler);

            //create system manager interface with System mode manager
            SysMgrAccess = new SystemManagerAccess(m_sysModeMgr);
        }

        #endregion Constructors
    }
}
