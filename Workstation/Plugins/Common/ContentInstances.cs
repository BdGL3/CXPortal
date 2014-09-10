using System.Collections;
using L3.Cargo.Workstation.Plugins.Common.Interfaces;

namespace L3.Cargo.Workstation.Plugins.Common
{
    public class ContentInstances : CollectionBase
    {
        public void Add(ContentInstance contentToAdd)
        {
            this.List.Add(contentToAdd);
        }

        public void Remove(ContentInstance contentToRemove)
        {
            contentToRemove.Dispose();
            this.List.Remove(contentToRemove);
        }

        public new void Clear()
        {
            foreach (ContentInstance content in this.List)
            {
                content.Dispose();
            }

            this.List.Clear();
        }

        public ContentInstance Find(string contentName)
        {
            ContentInstance toReturn = null;

            foreach (ContentInstance content in this.List)
            {
                if (content.Instance.Name.Equals(contentName))
                {
                    toReturn = content;
                    break;
                }
            }
            return toReturn;
        }
    }

    public class ContentInstance : PluginInstance<IContent>
    {
        public ContentInstance()
        {
        }
    }
}
