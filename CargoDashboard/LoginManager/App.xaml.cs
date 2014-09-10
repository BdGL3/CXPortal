using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace LoginManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Private Members

        private Mutex mutex;

        private MainWindow _MainWindow;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow (IntPtr hWnd);

        #endregion Private Members


        #region Constructors

        public App ()
        {
            bool createdNew = true;

            mutex = new Mutex(true, "LoginApp", out createdNew);

            if (createdNew)
            {
                _MainWindow = new LoginManager.MainWindow();
                _MainWindow.Show();
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
            _MainWindow.Close();

            if (mutex != null)
            {
                mutex.Dispose();
            }
        }

        #endregion Private Methods

        private void Application_DispatcherUnhandledException (object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.ToString());
        }
    }
}
