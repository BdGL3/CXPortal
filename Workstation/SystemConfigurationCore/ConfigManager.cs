using System;
using System.Collections.Generic;
using L3.Cargo.Workstation.ProfileManagerCore;

namespace L3.Cargo.Workstation.SystemConfigurationCore
{
    public class ConfigManager
    {
        #region Private Memebers

        //dictionary used to store SystemConfigurations
        private Dictionary<string, SysConfiguration> m_SysConfigCollection;

        private ProfileManager m_ProfileManager;

        #endregion


        #region Public Members

        public ProfileManager UserProfileManager
        {
            get
            {
                return m_ProfileManager;
            }
        }

        #endregion


        #region Constructors

        public ConfigManager (ProfileManager profileMgr, SysConfiguration sysConfig)
        {
            m_ProfileManager = profileMgr;

            m_SysConfigCollection = new Dictionary<string, SysConfiguration>();
            //TODO: Remove the configuration setting here
            sysConfig.m_SysConfigID = "DefaultConfig";
            m_SysConfigCollection.Add("DefaultConfig", sysConfig);
        }

        #endregion


        #region Methods

        public SysConfiguration GetConfig (string configId)
        {
            SysConfiguration sysConfig = null;

            if (m_SysConfigCollection.ContainsKey(configId))
            {
                sysConfig = m_SysConfigCollection[configId];
            }
            else
            {
                sysConfig = GetDefaultConfig();
            }

            sysConfig.Profile = m_ProfileManager.Profile;

            return sysConfig;
        }

        public SysConfiguration GetDefaultConfig()
        {
            SysConfiguration sysConfig = m_SysConfigCollection["DefaultConfig"];
            sysConfig.Profile = m_ProfileManager.Profile;

            return sysConfig;
        }

        public void Add (SysConfiguration sysConfig)
        {
            try
            {
                m_SysConfigCollection.Add(sysConfig.ID, sysConfig);
            }
            catch (Exception ex)
            {
                //TODO Log exception here
                throw ex;
            }
        }

        public void Update (SysConfiguration sysConfig)
        {
            try
            {
                m_SysConfigCollection[sysConfig.ID] = sysConfig;
            }
            catch (Exception ex)
            {
                //TODO Log exception here
                throw ex;
            }
        }

        public void Delete(string SysConfigID)
        {
            try
            {
                m_SysConfigCollection.Remove(SysConfigID);
            }
            catch (Exception ex)
            {
                //TODO Log exception here
                throw ex;
            }
        }

        public bool Contains(string SysConfigID)
        {
            try
            {
                return m_SysConfigCollection.ContainsKey(SysConfigID);
            }
            catch (Exception ex)
            {
                //TODO Log exception here
                throw ex;
            }
        }


        #endregion
    }
}
