using System;
using System.Reflection;
using L3.Cargo.Workstation.Plugins.Common;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;
using L3.Cargo.Workstation.SystemConfigurationCore;

namespace L3.Cargo.Workstation.Plugins.Manager
{
    public class ContentPluginManager : PluginManagerBase<IContent>
    {
        #region Constuctors

        public ContentPluginManager (SysConfigMgrAccess sysConfig)
            : base(sysConfig)
        {
            base.FindPlugins(new PluginSearchCriteria());
        }

        #endregion Constuctors


        #region Public Methods

        public ContentInstances GetInstances(ContentParameter parameters)
        {
            ContentInstances instances = new ContentInstances();

            foreach (string fileName in base.m_PluginAssemblies)
            {
                Assembly contentAssembly = Assembly.LoadFrom(fileName);

                foreach (Type assemblyType in contentAssembly.GetTypes())
                {
                    Type typeInterface = assemblyType.GetInterface("IContent", true);

                    if (assemblyType.IsPublic && !assemblyType.IsAbstract && typeInterface != null)
                    {
                        ContentInstance newContent = new ContentInstance();

                        newContent.Instance = Activator.CreateInstance(assemblyType) as IContent;

                        try
                        {
                            newContent.Instance.Initialize(parameters);
                            instances.Add(newContent);
                        }
                        catch (Exception ex)
                        {
                            //TODO: Log a message here since the instance couldn't be made
                        }

                        newContent = null;
                    }

                    typeInterface = null;
                }

                contentAssembly = null;
            }

            return instances;
        }

        #endregion Public Methods
    }
}
