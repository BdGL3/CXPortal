using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using L3.Cargo.Common.Dashboard;

namespace L3.Cargo.Dashboard.PresentationCore
{
    /// <summary>
    /// Interaction logic for LogoutButton.xaml
    /// </summary>
    public partial class LogoutButton : UserControl
    {
        public LogoutButton ()
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);
        }

        private void Border_MouseDown(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Border_TouchDown(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
