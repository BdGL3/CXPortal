using L3.Cargo.Workstation.ProfileManagerCore;

namespace L3.Cargo.Workstation.SystemConfigurationCore
{
    public class SysConfigManager
    {
        #region Private Members

        private SysConfigMgrAccess m_SysConfigMgrAccess;

        #endregion Private Members


        #region Public Members

        public SysConfigMgrAccess SysConfigAccess
        {
            get
            {
                return m_SysConfigMgrAccess;
            }
        }

        #endregion


        #region Constructors

        public SysConfigManager()
        {
            //create profile manager with default user profile
            ProfileManager profileManager = new ProfileManager();

            SysConfiguration systemConfiguration = new SysConfiguration();

            //create configuration manager with profile manager and default system configuration.
            ConfigManager m_ConfigMgr = new ConfigManager(profileManager, systemConfiguration);

            //create system configuraiton manager interface
            m_SysConfigMgrAccess = new SysConfigMgrAccess(m_ConfigMgr);
        }

        #endregion


        #region Methods

        #endregion
    }
}
