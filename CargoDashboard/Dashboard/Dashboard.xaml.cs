using System;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Dashboard.AssemblyManager;
using L3.Cargo.Dashboard.DataAccessCore;
using L3.Cargo.Dashboard.PresentationCore;

namespace Dashboard
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Private Members

        private Mutex mutex;

        private UIManager _UIManager;

        private DataAccess _DataAccess;

        private AssemblyManager _AssemblyManager;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow (IntPtr hWnd);

        #endregion Private Members


        #region Constructors

        public App()
        {
            bool createdNew = true;

            mutex = new Mutex(true, "Dashboard", out createdNew);

            if (createdNew)
            {
                if (Environment.GetCommandLineArgs().Length > 1)
                {
                    foreach (string arg in Environment.GetCommandLineArgs())
                    {
                        if (string.Compare(arg, "Operator", true) == 0 ||
                            string.Compare(arg, "Supervisor", true) == 0 ||
                            string.Compare(arg, "Maintenance", true) == 0 ||
                            string.Compare(arg, "Engineer", true) == 0)
                        {
                            ConfigurationManager.AppSettings["SystemOperationMode"] = arg;
                            break;
                        }
                    }

                    EventLoggerAccess logger = new EventLoggerAccess();
                    _UIManager = new UIManager(logger);
                    _DataAccess = new DataAccess(logger);
                    _AssemblyManager = new AssemblyManager(_UIManager, _DataAccess, logger);

                    _DataAccess.StartUp();
                    _UIManager.Show();
                    logger.LogInfo("Dashboard Client Started");
                }
                else
                {
                    Application.Current.Shutdown();
                }
            }
            else
            {
                Process current = Process.GetCurrentProcess();
                foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                {
                    SetForegroundWindow(process.MainWindowHandle);
                }

                Application.Current.Shutdown();
            }
        }

        #endregion Constructors


        #region Private Methods

        private void Application_Exit (object sender, ExitEventArgs e)
        {
            if (_DataAccess != null)
            {
                _DataAccess.Dispose();
            }

            if (mutex != null)
            {
                mutex.Dispose();
            }
        }


        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString());
        }

        #endregion Private Methods
    }
}
