using System.Windows.Controls;
using System.Windows.Data;
using L3.Cargo.Common.Dashboard;

namespace L3.Cargo.Dashboard.PresentationCore
{
    /// <summary>
    /// Interaction logic for DefaultWidget.xaml
    /// </summary>
    public partial class DefaultWidget : UserControl
    {
        public DefaultWidget()
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);
        }
    }
}
