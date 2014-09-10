using System.Configuration;

namespace L3.Cargo.Common.Configurations
{
    public class OpcSection : ConfigurationSection
    {
        [ConfigurationProperty("server", IsRequired = true)]
        public OpcServerElement Server
        {
            get
            {
                return (OpcServerElement)this["server"];
            }
            set
            {
                this["server"] = value;
            }
        }
    }


    public class OpcServerElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("channel", IsRequired = true)]
        public string Channel
        {
            get
            {
                return (string)this["channel"];
            }
            set
            {
                this["channel"] = value;
            }
        }

        [ConfigurationProperty("device", IsRequired = true)]
        public string Device
        {
            get
            {
                return (string)this["device"];
            }
            set
            {
                this["device"] = value;
            }
        }

        [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
        public OpcTagGroupCollection TagGroup
        {
            get
            {
                return (OpcTagGroupCollection)this[""];
            }
            set
            {
                this["tagGroup"] = value;
            }
        }
    }

    public class OpcTagGroupCollection : ConfigurationElementCollection
    {
        public OpcTagGroupElement this[int index]
        {
            get { return (OpcTagGroupElement)BaseGet(index); }
        }

        public OpcTagGroupElement GetElement(string element)
        {
            return (OpcTagGroupElement)BaseGet(element);
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new OpcTagGroupElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((OpcTagGroupElement)element).Name;
        }

        protected override string ElementName
        {
            get { return "tagGroup"; }
        }
    }


    public class OpcTagGroupElement : ConfigurationElement
    {
        #region Public Members

        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("updaterate", IsRequired = true)]
        public int UpdateRate
        {
            get
            {
                return (int)this["updaterate"];
            }
            set
            {
                this["updaterate"] = value;
            }
        }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public OpcTagCollection Tags
        {
            get
            {
                return (OpcTagCollection)this[""];
            }
        }

        #endregion Public Members
    }


    public class OpcTagCollection : ConfigurationElementCollection
    {
        public OpcTagElement this[int index]
        {
            get { return (OpcTagElement)BaseGet(index); }
        }

        public OpcTagElement GetElement(string element)
        {
            return (OpcTagElement)BaseGet(element);
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override ConfigurationElement CreateNewElement ()
        {
            return new OpcTagElement();
        }

        protected override object GetElementKey (ConfigurationElement element)
        {
            return ((OpcTagElement)element).Name;
        }

        protected override string ElementName
        {
            get { return "tag"; }
        }
    }


    public class OpcTagElement : ConfigurationElement
    {
        #region Public Members

        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get
            {
                return (string)this["type"];
            }
            set
            {
                this["type"] = value;
            }
        }

        [ConfigurationProperty("detail", IsRequired = false)]
        public string Detail
        {
            get
            {
                return (string)this["detail"];
            }
            set
            {
                this["detail"] = value;
            }
        }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public OpcTagValueCollection TagValues
        {
            get
            {
                return (OpcTagValueCollection)this[""];
            }
        }

        public class OpcTagValueCollection : ConfigurationElementCollection
        {
            public OpcTagValueElement this[int index]
            {
                get { return (OpcTagValueElement)BaseGet(index); }
            }

            public override ConfigurationElementCollectionType CollectionType
            {
                get { return ConfigurationElementCollectionType.BasicMap; }
            }

            protected override ConfigurationElement CreateNewElement()
            {
                return new OpcTagValueElement();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((OpcTagValueElement)element).Value;
            }

            protected override string ElementName
            {
                get { return "tagValue"; }
            }
        }

        public class OpcTagValueElement : ConfigurationElement
        {
            #region Public Members

            [ConfigurationProperty("value", IsRequired = true, IsKey = true)]
            public int Value
            {
                get
                {
                    return (int)this["value"];
                }
                set
                {
                    this["value"] = value;
                }
            }

            [ConfigurationProperty("type", IsRequired = true)]
            public string Type
            {
                get
                {
                    return (string)this["type"];
                }
                set
                {
                    this["type"] = value;
                }
            }

            #endregion Public Members
        }

        #endregion Public Members
    }
}
