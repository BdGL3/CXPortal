using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace L3.Cargo.Workstation.Plugins.Common
{
    public class LayoutInfo
    {
        public String Name { get; set; }
        public PanelAssignment Panel { get; set; }
        public UserControl Display { get; set; }
        public List<StatusBarItem> StatusItems { get; set; }
        public bool BringToFront { get; set; }

        public LayoutInfo ()
        {
            BringToFront = false;
        }
    }
}
