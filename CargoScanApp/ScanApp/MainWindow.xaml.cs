using System.Windows;
using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Scan.BusinessCore;
using L3.Cargo.Subsystem.DataAccessCore;

namespace ScanApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BusinessManager _BusinessManager;

        DataAccess _DataAccess;

        EventLoggerAccess _Logger;

        public MainWindow ()
        {
            InitializeComponent();

            _Logger = new EventLoggerAccess();

            _DataAccess = new DataAccess(_Logger);
            _BusinessManager = new BusinessManager(_DataAccess, _Logger);
        }

        private void Window_Closing (object sender, System.ComponentModel.CancelEventArgs e)
        {
            _DataAccess.Dispose();
        }
    }
}
