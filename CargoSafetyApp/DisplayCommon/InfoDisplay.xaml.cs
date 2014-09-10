using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using L3.Cargo.Common.Dashboard;
using L3.Cargo.Communications.Dashboard.Display.Host;

namespace L3.Cargo.Safety.Display.Common
{
    /// <summary>
    /// Interaction logic for InfoDisplay.xaml
    /// </summary>
    public partial class InfoDisplay : UserControl
    {
        public EStopStatus estopStatus
        {
            get { return EStopStatus; }
        }

        public InfoDisplay (TagUpdate tagupdate)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);
        }

        public InfoDisplay(Dispatcher dispatcher, WidgetStatusHost host)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);

            TagUpdate plcTagUpdate = new TagUpdate(EStopStatus, InterlockStatus, WarningStatus, SummaryStatus, dispatcher, host);
        }

        private void Display_MouseOrTouchDown (object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void ContentArea_MouseOrTouchDown(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }
    }
}
