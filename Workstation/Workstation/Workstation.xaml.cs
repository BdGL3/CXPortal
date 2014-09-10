using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using L3.Cargo.Workstation.DataSourceCore;
using L3.Cargo.Workstation.PresentationCore;
using L3.Cargo.Workstation.SystemConfigurationCore;
using L3.Cargo.Workstation.SystemManagerCore;

namespace Workstation
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private SysConfigManager SysConfigMgr;

        private DataSource dataSource;

        private SystemManager SysMgr;

        private Presentation defaultUI;

        private Mutex mutex;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public App()
        {
            bool createdNew = true;

            mutex = new Mutex(true, "Workstation", out createdNew);

            if (createdNew)
            {
                //create System Configuration Manager with default System configuration and user profile.
                SysConfigMgr = new SysConfigManager();

                //create Data Access Layer with System Configuration Manager Interface
                dataSource = new DataSource(SysConfigMgr.SysConfigAccess);

                //create system manager with Data Access Layer Interface and System Configuration Manager Interface
                SysMgr = new SystemManager(SysConfigMgr.SysConfigAccess, dataSource.SourceAccess);

                //create Presentation Layer with System configuration manager interface and system manager interface
                defaultUI = new Presentation(SysConfigMgr.SysConfigAccess, SysMgr.SysMgrAccess);
                defaultUI.Show();
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

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (dataSource != null)
            {
                dataSource.Dispose();
            }

            if (mutex != null)
            {
                mutex.Dispose();
            }
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            //ignore this exception because it is being handled by observableCollectinEx
            if (e.Exception.InnerException != null &&
                e.Exception.InnerException.GetType() != typeof(System.InvalidOperationException) &&
                e.Exception.InnerException.Message.Contains("Added item does not appear at given index"))
            {
                MessageBox.Show(e.Exception.ToString(), "Dispatcher unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            MessageBox.Show(ex.ToString(), "Uncaught Thread Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
