using System.Configuration;

namespace L3.Cargo.Common.Configurations
{
    public class SettingsSection : ConfigurationSection
    {
        [ConfigurationProperty("TagSettings", IsRequired = false, IsDefaultCollection = true)]
        public TagSettingsCollection TagSettings
        {
            get
            {
                return (TagSettingsCollection)this["TagSettings"];
            }
            set
            {
                this["TagSettings"] = value;
            }
        }

        [ConfigurationProperty("ConnectionSettings", IsRequired = true)]
        public ConnectionSettingsElement ConnectionSettings
        {
            get
            {
                return (ConnectionSettingsElement)this["ConnectionSettings"];
            }
            set
            {
                this["ConnectionSettings"] = value;
            }
        }
    }

    public class TagSettingsCollection : ConfigurationElementCollection
    {
        public TagElement this[int index]
        {
            get { return (TagElement)BaseGet(index); }
        }

        public TagElement GetElement(string element)
        {
            return (TagElement)BaseGet(element);
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new TagElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TagElement)element).Name;
        }

        protected override string ElementName
        {
            get { return "tag"; }
        }
    }


    public class TagElement : ConfigurationElement
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

        [ConfigurationProperty("PLCTagName", IsRequired = true)]
        public string PLCTagName
        {
            get
            {
                return (string)this["PLCTagName"];
            }
            set
            {
                this["PLCTagName"] = value;
            }
        }       

        #endregion Public Members
    }

    public class ConnectionSettingsElement : ConfigurationElement
    {
        #region Public Members

        [ConfigurationProperty("DisplaySettings", IsRequired = true)]
        public DisplaySettingsElement DisplaySettings
        {
            get
            {
                return (DisplaySettingsElement)this["DisplaySettings"];
            }
            set
            {
                this["DisplaySettings"] = value;
            }
        }

        [ConfigurationProperty("SubsystemSettings", IsRequired = true)]
        public SubsystemSettingsElement SubsystemSettings
        {
            get
            {
                return (SubsystemSettingsElement)this["SubsystemSettings"];
            }
            set
            {
                this["SubsystemSettings"] = value;
            }
        }
        #endregion
    }

    public class DisplaySettingsElement : ConfigurationElement
    {
        #region Public Members

        [ConfigurationProperty("server", IsRequired = true, IsKey = true)]
        public string Server
        {
            get
            {
                return (string)this["server"];
            }
            set
            {
                this["server"] = value;
            }
        }

        [ConfigurationProperty("port", IsRequired = true)]
        public int Port
        {
            get
            {
                return (int)this["port"];
            }
            set
            {
                this["port"] = value;
            }
        }

        #endregion Public Members
    }

    public class SubsystemSettingsElement : ConfigurationElement
    {
        #region Public Members

        [ConfigurationProperty("server", IsRequired = true, IsKey = true)]
        public string Server
        {
            get
            {
                return (string)this["server"];
            }
            set
            {
                this["server"] = value;
            }
        }

        [ConfigurationProperty("port", IsRequired = true)]
        public int Port
        {
            get
            {
                return (int)this["port"];
            }
            set
            {
                this["port"] = value;
            }
        }

        #endregion Public Members
    }
}