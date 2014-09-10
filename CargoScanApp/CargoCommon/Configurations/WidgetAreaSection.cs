using System;
using System.Collections;
using System.Text;
using System.Configuration;
using System.Xml;

namespace L3.Cargo.Common.Configurations
{
    public class WidgetAreaSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsDefaultCollection=true)]
        public WidgetPageCollection WidgetPage
        {
            get
            {
                return (WidgetPageCollection)this[""];
            }
        }
    }


    public class WidgetPageCollection : ConfigurationElementCollection
    {
        public WidgetPageElement this[int index]
        {
            get { return (WidgetPageElement)BaseGet(index); }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new WidgetPageElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((WidgetPageElement)element).Page;
        }

        protected override string ElementName
        {
            get { return "widgetPage"; }
        }
    }


    public class WidgetPageElement : ConfigurationElement
    {
        [ConfigurationProperty("page", DefaultValue = "1", IsRequired = true, IsKey=true)]
        [IntegerValidator(MinValue = 1)]
        public int Page
        {
            get
            {
                return (int)this["page"];
            }
            set
            {
                this["page"] = value;
            }
        }
        
        [ConfigurationProperty("rows", DefaultValue = "1", IsRequired = true)]
        public int Rows
        {
            get
            {
                return (int)this["rows"];
            }
            set
            {
                this["rows"] = value;
            }
        }

        [ConfigurationProperty("columns", DefaultValue = "1", IsRequired = true)]
        public int Columns
        {
            get
            {
                return (int)this["columns"];
            }
            set
            {
                this["columns"] = value;
            }
        }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public WidgetDisplayCollection WidgetDisplay
        {
            get
            {
                return (WidgetDisplayCollection)this[""];
            }
        }
    }


    public class WidgetDisplayCollection : ConfigurationElementCollection
    {
        public new WidgetDisplayElement this[string name]
        {
            get
            {
                return (WidgetDisplayElement)BaseGet(name);
            }
        }

        public WidgetDisplayElement this[int index]
        {
            get
            {
                return (WidgetDisplayElement)BaseGet(index);
            }
        }

        public int IndexOf(string name)
        {
            name = name.ToLower();

            for (int idx = 0; idx < base.Count; idx++)
            {
                if (this[idx].Name.ToLower() == name)
                    return idx;
            }
            return -1;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new WidgetDisplayElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((WidgetDisplayElement)element).Name;
        }

        protected override string ElementName
        {
            get { return "widgetDisplay"; }
        }
    }


    public class WidgetDisplayElement : ConfigurationElement
    {
        #region Public Members

        [ConfigurationProperty("name", DefaultValue = "unknown", IsRequired = true)]
        [StringValidator(MinLength = 1, MaxLength = 60)]
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

        [ConfigurationProperty("row", DefaultValue = "0", IsRequired = true)]
        [IntegerValidator(MinValue = 0)]
        public int Row
        {
            get
            {
                return (int)this["row"];
            }
            set
            {
                this["row"] = value;
            }
        }

        [ConfigurationProperty("column", DefaultValue = "0", IsRequired = true)]
        [IntegerValidator(MinValue = 0)]
        public int Column
        {
            get
            {
                return (int)this["column"];
            }
            set
            {
                this["column"] = value;
            }
        }

        [ConfigurationProperty("rowspan", DefaultValue = "1", IsRequired = true)]
        [IntegerValidator(MinValue = 1)]
        public int RowSpan
        {
            get
            {
                return (int)this["rowspan"];
            }
            set
            {
                this["rowspan"] = value;
            }
        }

        [ConfigurationProperty("columnspan", DefaultValue = "1", IsRequired = true)]
        [IntegerValidator(MinValue = 1)]
        public int ColumnSpan
        {
            get
            {
                return (int)this["columnspan"];
            }
            set
            {
                this["columnspan"] = value;
            }
        }

        #endregion Public Members


        #region Constructors

        public WidgetDisplayElement()
        {
        }

        public WidgetDisplayElement(string name)
        {
            Name = name;
        }

        #endregion Constructors
    }
}
