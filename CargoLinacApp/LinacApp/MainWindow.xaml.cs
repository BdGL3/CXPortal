using System.Windows;
using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Linac.BusinessCore;
using L3.Cargo.Linac.DataAccessCore;
using L3.Cargo.Subsystem.DataAccessCore;

namespace LinacApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BusinessManager _BusinessManager;

        LinacDataAccess _DataAccess;

        EventLoggerAccess _Logger;

        public MainWindow ()
        {
            InitializeComponent();

            _Logger = new EventLoggerAccess();

            _DataAccess = new LinacDataAccess(_Logger);
            _BusinessManager = new BusinessManager(_DataAccess, _Logger);

            _DataAccess.Open();


           
        }

        private void Window_Closing (object sender, System.ComponentModel.CancelEventArgs e)
        {
            _DataAccess.Dispose();
        }
    }
}
