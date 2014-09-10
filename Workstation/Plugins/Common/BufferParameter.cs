using System.Windows.Controls;
using L3.Cargo.Common.Xml.History_1_0;

namespace L3.Cargo.Workstation.Plugins.Common
{
    public class BufferParameter
    {
        public DockPanel dockPanel;
        public History History;

        public BufferParameter (DockPanel panel, History history)
        {
            dockPanel = panel;
            History = history;
        }
    }
}
