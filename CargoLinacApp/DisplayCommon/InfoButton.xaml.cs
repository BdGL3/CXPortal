using System.Windows.Controls;
using System.Windows.Data;
using L3.Cargo.Common.Dashboard;

namespace L3.Cargo.Linac.Display.Common
{
    /// <summary>
    /// Interaction logic for InfoButton.xaml
    /// </summary>
    public partial class InfoButton : UserControl
    {
        public InfoButton ()
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);
        }
    }
}
