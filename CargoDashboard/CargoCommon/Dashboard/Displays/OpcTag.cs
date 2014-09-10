using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3.Cargo.Dashboard.Assembly.Common
{
    public class OpcTag
    {
        public string Name {get; set;}
        public string ResourceName { get; set; }

        public OpcTag ()
        {
        }

        public OpcTag (string name, string resourceName)
        {
            Name = name;
            ResourceName = resourceName;
        }
    }
}
