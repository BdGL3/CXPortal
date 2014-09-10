using System.Collections;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;
using System;

namespace L3.Cargo.Workstation.Plugins.Common
{
    public class FilterInstances : CollectionBase
    {
        public void Add(FilterInstance filterToAdd)
        {
            this.List.Add(filterToAdd);
        }

        public void Remove(FilterInstance filterToRemove)
        {
            filterToRemove.Dispose();
            this.List.Remove(filterToRemove);
        }

        public void Insert(int index, FilterInstance filterToRemove)
        {
            this.List.Insert(index, filterToRemove);
        }

        public new void Clear()
        {
            foreach (FilterInstance filter in this.List)
            {
                filter.Dispose();
            }

            this.List.Clear();
        }

        public FilterInstance Find(string filterName)
        {
            FilterInstance toReturn = null;

            foreach (FilterInstance filter in this.List)
            {
                if (filter.Instance.Name.Equals(filterName))
                {
                    toReturn = filter;
                    break;
                }
            }
            return toReturn;
        }
    }

    public class FilterInstance : PluginInstance<IFilter>
    {
        public FilterInstance()
        {
        }
    }
}
