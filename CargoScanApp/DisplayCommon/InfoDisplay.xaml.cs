using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using L3.Cargo.Common.Dashboard;
using L3.Cargo.Communications.Dashboard.Display.Host;

namespace L3.Cargo.Scan.Display.Common
{
    /// <summary>
    /// Interaction logic for InfoDisplay.xaml
    /// </summary>
    public partial class InfoDisplay : UserControl
    {
        #region Constructors

        public InfoDisplay (Dispatcher dispatcher, WidgetStatusHost widgetStatusHost)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);

            ScanState.UIDispatcher = dispatcher;
            ScanState.WidgetStatusHost = widgetStatusHost;
        }

        #endregion Constructors


        #region Private Methods

        private void Display_MouseOrTouchDown (object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void ContentArea_MouseOrTouchDown(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        #endregion Private Methods
    }
}
