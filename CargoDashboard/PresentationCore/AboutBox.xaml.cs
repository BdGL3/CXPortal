using System.Windows;
using System.Windows.Controls;
using L3.Cargo.Common.Dashboard;
using L3.Cargo.Common.Dashboard.Configurations;

namespace L3.Cargo.Dashboard.PresentationCore
{
    /// <summary>
    /// Interaction logic for AboutBox.xaml
    /// </summary>
    public partial class AboutBox : UserControl
    {
        #region Constructors

        public AboutBox ()
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);

            VersionNumber.Text = DashboardConfiguration.VersionNumber;
            BuildNumber.Text = DashboardConfiguration.BuildNumber;
            BuildDate.Text = DashboardConfiguration.BuildDate;
        }

        #endregion Constructors


        #region Private Methods

        private void Display_MouseOrTouchDown (object sender, RoutedEventArgs e)
        {
            HideBox();
        }

        #endregion Private Methods


        #region Public Methods

        public void ShowBox ()
        {
            this.Visibility = Visibility.Visible;
        }

        public void HideBox ()
        {
            this.Visibility = Visibility.Collapsed;
        }

        #endregion Public Methods
    }
}
