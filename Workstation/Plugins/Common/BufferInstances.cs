using System.Collections;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;
using System;

namespace L3.Cargo.Workstation.Plugins.Common
{
    public class BufferInstances : CollectionBase
    {
        public void Add (BufferInstance bufferToAdd)
        {
            this.List.Add(bufferToAdd);
        }

        public void Remove (BufferInstance bufferToRemove)
        {
            bufferToRemove.Dispose();
            this.List.Remove(bufferToRemove);
        }

        public new void Clear()
        {
            foreach (BufferInstance filter in this.List)
            {
                filter.Dispose();
            }

            this.List.Clear();
        }

        public BufferInstance Find (string bufferName)
        {
            BufferInstance toReturn = null;

            foreach (BufferInstance buffer in this.List)
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

    public class BufferInstance : PluginInstance<IBuffer>
    {
        public BufferInstance ()
        {
        }
    }
}
