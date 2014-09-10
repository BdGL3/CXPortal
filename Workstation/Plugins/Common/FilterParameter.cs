using System.Windows.Controls;
using L3.Cargo.Common.Xml.History_1_0;
using L3.Cargo.Common;
using L3.Cargo.Workstation.SystemConfigurationCore;

namespace L3.Cargo.Workstation.Plugins.Common
{
    public class FilterParameter
    {
        public DockPanel dockPanel;
        public History History;
        public int Width;
        public int Height;
        public SysConfiguration SysConfig;

        public FilterParameter (DockPanel panel, int width, int height, History history, SysConfiguration config)
        {
            dockPanel = panel;
            Width = width;
            Height = height;
            History = history;
            SysConfig = config;
        }
    }
}
