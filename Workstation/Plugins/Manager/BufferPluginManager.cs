using System;
using System.Collections.Generic;
using System.Reflection;
using L3.Cargo.Workstation.Plugins.Common;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;

namespace L3.Cargo.Workstation.Plugins.Manager
{
    public class BufferPluginManager : PluginManagerBase<IBuffer>
    {
        #region Constuctors

        public BufferPluginManager ()
            : base()
        {
            List<string> keywords = new List<string>();
            keywords.Add("L3Buffer");
            keywords.Add(".dll");

            String path = AppDomain.CurrentDomain.BaseDirectory + "Buffers\\";

            PluginSearchCriteria filterSearchCriteria = new PluginSearchCriteria(path, keywords);

            base.FindPlugins(filterSearchCriteria);
        }

        #endregion Constuctors


        #region Public Methods

        public BufferInstances GetInstances()
        {
            BufferInstances instances = new BufferInstances();

            foreach (string fileName in base.m_PluginAssemblies)
            {
                Assembly filterAssembly = Assembly.LoadFrom(fileName);

                foreach (Type assemblyType in filterAssembly.GetTypes())
                {
                    Type typeInterface = assemblyType.GetInterface("IBuffer", true);

                    if (assemblyType.IsPublic && !assemblyType.IsAbstract && typeInterface != null)
                    {
                        BufferInstance newBuffer = new BufferInstance();

                        newBuffer.Instance = Activator.CreateInstance(assemblyType) as IBuffer;

                        if (newBuffer.Instance != null)
                        {
                            instances.Add(newBuffer);
                        }

                        newBuffer = null;
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
