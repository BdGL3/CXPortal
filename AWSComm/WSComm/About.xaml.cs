using System.Windows;
using System.Configuration;

namespace L3.Cargo.WSCommunications
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();

            VerionNumber.Text = (string)ConfigurationManager.AppSettings["VersionNumber"];
            BuildDate.Text = (string)ConfigurationManager.AppSettings["BuildDate"];
        }
    }
}
