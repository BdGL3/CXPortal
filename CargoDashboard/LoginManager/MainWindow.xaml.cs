using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using L3.Cargo.Communications.CargoHost;
using System.Configuration;
using System.Threading;
using System.ComponentModel;
using System.Management;
using System.Diagnostics;
using System.IO;
using System.Windows.Threading;

namespace LoginManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Members

        private CargoHostEndPoint _cargoHostEndPoint;

        private LoginInfo _loginInfo;

        private Timer _IsConnectedCheck;

        private Timer _IsApplicationRunningCheck;

        #endregion Private Members


        #region Constructors

        public MainWindow()
        {
            InitializeComponent();

            _cargoHostEndPoint = new CargoHostEndPoint(ConfigurationManager.AppSettings["CargoHostLocation"],
                Convert.ToInt32(ConfigurationManager.AppSettings["CargoHostPort"]));

            _loginInfo = new LoginInfo();
            this.DataContext = _loginInfo;
        }

        #endregion Constructors


        #region Private Methods

        private void LoginCheck(object sender, DoWorkEventArgs e)
        {
            try
            {
                _loginInfo.AccessLevel = _cargoHostEndPoint.Login(_loginInfo.Username, _loginInfo.Password);
            }
            catch (Exception ex)
            {
                _loginInfo.ErrorMessage = ex.Message;
            }
            finally
            {
                _loginInfo.ClearPassword();
            }
        }

        private void LoginCheckCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_loginInfo.AccessLevel == l3.cargo.corba.AuthenticationLevel.OPERATOR)
            {
                LaunchApplication();
            }
            else if (_loginInfo.AccessLevel != l3.cargo.corba.AuthenticationLevel.NONE)
            {
                GoToAccessPage();
                _loginInfo.ClearUserInfo();
            }
        }

        private void GoToLoginPage()
        {
            this.ContentHolder.Source = new Uri("/Views/Login.xaml", UriKind.RelativeOrAbsolute);
        }

        private void GoToAccessPage()
        {
            this.ContentHolder.Source = new Uri("/Views/AccessLevel.xaml", UriKind.RelativeOrAbsolute);
        }

        private void LoginButtonClick(object sender, RoutedEventArgs e)
        {
            _loginInfo.ClearErrorMessage();

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(LoginCheck);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(LoginCheckCompleted);
            bw.RunWorkerAsync();
        }

        private void LaunchButtonClick(object sender, RoutedEventArgs e)
        {
            LaunchApplication();
        }

        public static string GetDashboardApplicationPath()
        {
            return System.IO.Path.Combine(ConfigurationManager.AppSettings["ApplicationLocation"], ConfigurationManager.AppSettings["ApplicationName"]) + ".exe";
        }

        private void LaunchApplication ()
        {
            string application = GetDashboardApplicationPath();
            if (File.Exists(application))
            {
                Process.Start(application, _loginInfo.DashboardMode);
                if (_IsApplicationRunningCheck == null)
                {
                    _IsApplicationRunningCheck = new Timer(new TimerCallback(CheckApplicationRunning), null, 0, 1000);
                }
            }
            else
            {
                MessageBox.Show("Application does not exist.");
            }
        }

        private void CheckCargoHostConnection(object stateInfo)
        {
            try
            {
                if (!_cargoHostEndPoint.IsConnected)
                {
                    _cargoHostEndPoint.Open();
                }

                _loginInfo.IsConnected = _cargoHostEndPoint.IsHostAvailable;
            }
            catch
            {
                if (_cargoHostEndPoint.IsConnected)
                {
                    _cargoHostEndPoint.Close();
                }
                _loginInfo.IsConnected = false;
            }
        }

        private void CheckApplicationRunning(object stateInfo)
        {
            Process[] p = Process.GetProcessesByName(ConfigurationManager.AppSettings["ApplicationName"]);

            if (p.Length != 0)
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
                    {
                        _loginInfo.Clear();
                        GoToLoginPage();
                        this.Hide();
                    }));
            }
            else
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
                    {
                        CultureResources.ChangeCulture(CultureResources.getCultureSetting());
                        this.Show();
                        _IsApplicationRunningCheck = null;
                    }));
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GoToLoginPage();
            _IsConnectedCheck = new Timer(new TimerCallback(CheckCargoHostConnection), null, 0, 1000);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _IsConnectedCheck.Dispose();
        }

        #endregion Private Methods
    }
}
