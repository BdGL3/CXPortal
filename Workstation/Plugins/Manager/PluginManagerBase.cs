using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using L3.Cargo.Workstation.Plugins.Common;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;
using L3.Cargo.Workstation.SystemConfigurationCore;

namespace L3.Cargo.Workstation.Plugins.Manager
{
    public abstract class PluginManagerBase<T> : IDisposable
        where T : IPlugin
    {
        #region Protected Members

        protected List<string> m_PluginAssemblies;

        protected SysConfigMgrAccess m_Sysconfig;

        #endregion Protected Members


        #region Constructors

        public PluginManagerBase ()
        {
            m_Sysconfig = null;
            m_PluginAssemblies = new List<string>();
        }

        public PluginManagerBase (SysConfigMgrAccess sysConfig)
        {
            m_Sysconfig = sysConfig;
            m_PluginAssemblies = new List<string>();
        }

        #endregion Constructors


        #region Protected Methods

        protected void FindPlugins (PluginSearchCriteria pluginSearchCriteria)
        {
            foreach (string fileName in Directory.GetFiles(pluginSearchCriteria.Path))
            {
                Boolean IsFileFound = true;

                foreach (string keyword in pluginSearchCriteria.Keywords)
                {
                    if (!fileName.Contains(keyword))
                    {
                        IsFileFound = false;
                        break;
                    }
                }

                if (IsFileFound)
                {
                    m_PluginAssemblies.Add(fileName);
                }
            }
        }

        #endregion Protected Methods


        #region Public Methods

        public void Dispose()
        {
            m_PluginAssemblies.Clear();
            m_PluginAssemblies = null;

            m_Sysconfig = null;
        }

        #endregion Public Methods
    }
}