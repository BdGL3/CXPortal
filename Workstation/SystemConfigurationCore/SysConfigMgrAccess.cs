using System;
using L3.Cargo.Workstation.ProfileManagerCore;

namespace L3.Cargo.Workstation.SystemConfigurationCore
{
    public class SysConfigMgrAccess
    {
        #region Private Members

        private ConfigManager m_SysConfig;

        #endregion Private Members


        #region Public Members

        public ProfileManager UserProfileManager
        {
            get
            {
                return m_SysConfig.UserProfileManager;
            }
        }

        #endregion Public Members


        #region Constructors

        public SysConfigMgrAccess (ConfigManager cfgMgr)
        {
            m_SysConfig = cfgMgr;
        }

        #endregion Constructors


        #region Public Methods

        public SysConfiguration GetConfig (string SysConfigID)
        {
            try
            {
                return m_SysConfig.GetConfig(SysConfigID);
            }
            catch (Exception exp)
            {
                throw exp;
            }

        }

        public SysConfiguration GetDefaultConfig ()
        {
            try
            {
                return m_SysConfig.GetDefaultConfig();
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        public void Update (SysConfiguration sysConfig)
        {
            m_SysConfig.Update(sysConfig);
        }

        public void Add (SysConfiguration sysConfig)
        {
            try
            {
                m_SysConfig.Add(sysConfig);
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        public void Delete (String sysConfig)
        {
            try
            {
                m_SysConfig.Delete(sysConfig);
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }


        public bool Contains(String sysConfig)
        {
            try
            {
                return m_SysConfig.Contains(sysConfig);
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        #endregion Public Methods
    }    
}
