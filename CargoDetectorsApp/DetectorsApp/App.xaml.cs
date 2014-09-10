using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

using L3.Cargo.Communications.Common;

namespace DetectorsApp
{
    public partial class App : Application
    {
        public App()
        {
            bool createdNew = true;
            mutex = new Mutex(true, "DetectorsApp", out createdNew);
            if (createdNew)
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
            else
            {
                Process current = Process.GetCurrentProcess();
                foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                    SetForegroundWindow(process.MainWindowHandle);
                Application.Current.Shutdown();
            }
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
#if DEBUG
            MessageBox.Show(Utilities.TextTidy(e.ToString()), "Unhandled Anomaly!", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (mutex != null)
                mutex.Dispose();
        }

        private Mutex mutex;
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
