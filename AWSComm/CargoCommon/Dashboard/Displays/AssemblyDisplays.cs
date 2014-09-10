using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3.Cargo.Common.Dashboard.Display
{
    public class AssemblyDisplays
    {
        public List<CompleteInfo> CompleteInfos;

        public List<Widget> Widgets;

        public List<Status> Statuses;

        public string Name;

        public AssemblyDisplays(string name)
        {
            Name = name;
            CompleteInfos = new List<CompleteInfo>();
            Widgets = new List<Widget>();
            Statuses = new List<Status>();
        }
    }
}
