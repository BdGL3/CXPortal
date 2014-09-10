using System;
using System.Collections.Generic;
using System.Reflection;
using L3.Cargo.Workstation.Plugins.Common;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;

namespace L3.Cargo.Workstation.Plugins.Manager
{
    public class FilterPluginManager : PluginManagerBase<IFilter>
    {
        #region Constuctors

        public FilterPluginManager()
            : base()
        {
            List<string> keywords = new List<string>();
            keywords.Add("L3Filter");
            keywords.Add(".dll");

            String path = AppDomain.CurrentDomain.BaseDirectory + "Filters\\";

            PluginSearchCriteria filterSearchCriteria = new PluginSearchCriteria(path, keywords);

            base.FindPlugins(filterSearchCriteria);
        }

        #endregion Constuctors


        #region Public Methods

        public FilterInstances GetInstances()
        {
            FilterInstances instances = new FilterInstances();

            foreach (string fileName in base.m_PluginAssemblies)
            {
                Assembly filterAssembly = Assembly.LoadFrom(fileName);

                foreach (Type assemblyType in filterAssembly.GetTypes())
                {
                    Type typeInterface = assemblyType.GetInterface("IFilter", true);

                    if (assemblyType.IsPublic && !assemblyType.IsAbstract && typeInterface != null)
                    {
                        FilterInstance newFilter = new FilterInstance();

                        newFilter.Instance = Activator.CreateInstance(assemblyType) as IFilter;

                        if (newFilter.Instance != null)
                        {
                            if (newFilter.Instance.Name == "DensityAlarm")
                            {
                                // add the density alarm first
                                instances.Insert(0, newFilter);
                            }
                            else
                            {
                                instances.Add(newFilter);
                            }
                        }
                        
                        newFilter = null;
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
