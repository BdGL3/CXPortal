using System;
using System.Collections.Generic;
using System.Reflection;
using L3.Cargo.Workstation.Plugins.Common;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;
using L3.Cargo.Workstation.SystemConfigurationCore;

namespace L3.Cargo.Workstation.Plugins.Manager
{
    public class MainPanelPluginManager : PluginManagerBase<IMainPanel>
    {
        #region Constuctors

        public MainPanelPluginManager(SysConfigMgrAccess sysConfig)
            : base(sysConfig)
        {
            List<string> keywords = new List<string>();
            keywords.Add("L3MainPanel");
            keywords.Add(".dll");

            String path = AppDomain.CurrentDomain.BaseDirectory + "MainPanel\\";

            PluginSearchCriteria filterSearchCriteria = new PluginSearchCriteria(path, keywords);

            base.FindPlugins(filterSearchCriteria);
        }

        #endregion Constuctors


        #region Public Methods 
       
        public MainPanelInstance GetCasesMainPanelInstance(MainPanelParameter parameters)
        {
            foreach (string fileName in base.m_PluginAssemblies)
            {
                Assembly filterAssembly = Assembly.LoadFrom(fileName);

                foreach (Type assemblyType in filterAssembly.GetTypes())
                {
                    Type typeInterface = assemblyType.GetInterface("IMainPanel", true);

                    if (assemblyType.IsPublic && !assemblyType.IsAbstract && typeInterface != null)
                    {                 
                        MainPanelInstance newMainPanel = new MainPanelInstance();                       

                        newMainPanel.Instance = Activator.CreateInstance(assemblyType) as IMainPanel;

                        if (newMainPanel.Instance.Name.Contains("Cases"))
                        {
                            try
                            {
                                newMainPanel.Instance.Initialize(parameters);
                                return newMainPanel;
                            }
                            catch (System.Windows.Markup.XamlParseException exp)
                            {
                                return newMainPanel;
                            }
                            catch (Exception ex)
                            {
                                //TODO: Log a message here since the instance couldn't be made
                                return null;
                            }
                        }
                    }
                }
            }

            return null;
        }

        public MainPanelInstances GetInstances(MainPanelParameter parameters)
        {
            MainPanelInstances instances = new MainPanelInstances();

            foreach (string fileName in base.m_PluginAssemblies)
            {
                Assembly filterAssembly = Assembly.LoadFrom(fileName);

                foreach (Type assemblyType in filterAssembly.GetTypes())
                {
                    Type typeInterface = assemblyType.GetInterface("IMainPanel", true);

                    if (assemblyType.IsPublic && !assemblyType.IsAbstract && typeInterface != null && !assemblyType.FullName.Contains("Cases"))
                    {
                        MainPanelInstance newMainPanel = new MainPanelInstance();

                        newMainPanel.Instance = Activator.CreateInstance(assemblyType) as IMainPanel;

                        try
                        {
                            newMainPanel.Instance.Initialize(parameters);
                            instances.Add(newMainPanel);
                        }
                        catch (Exception ex)
                        {
                            //TODO: Log a message here since the instance couldn't be made
                        }

                        newMainPanel = null;
                    }

                    typeInterface = null;
                }

                filterAssembly = null;
            }

            return instances;
        }

        #endregion Public Methods
    }
}
