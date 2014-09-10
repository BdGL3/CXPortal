using System.Collections;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;
using System;

namespace L3.Cargo.Workstation.Plugins.Common
{
    public class MainPanelInstances : CollectionBase
    {
        public void Add(MainPanelInstance bufferToAdd)
        {
            this.List.Add(bufferToAdd);
        }

        public void Remove(MainPanelInstance bufferToRemove)
        {
            bufferToRemove.Dispose();
            this.List.Remove(bufferToRemove);
        }

        public new void Clear()
        {
            foreach (MainPanelInstance filter in this.List)
            {
                filter.Dispose();
            }

            this.List.Clear();
        }

        public MainPanelInstance Find(string bufferName)
        {
            MainPanelInstance toReturn = null;

            foreach (MainPanelInstance buffer in this.List)
            {
                if (buffer.Instance.Name.Equals(bufferName))
                {
                    toReturn = buffer;
                    break;
                }
            }
            return toReturn;
        }
    }

    public class MainPanelInstance : PluginInstance<IMainPanel>
    {
        public MainPanelInstance()
        {
        }
    }
}