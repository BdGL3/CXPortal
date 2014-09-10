using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

using L3.Cargo.Communications.APCS.Client;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Detectors.BusinessCore;
using L3.Cargo.Detectors.Common;
using L3.Cargo.Detectors.DataAccessCore;

namespace DetectorsApp
{
    public partial class MainWindow : Window
    {
        private BusinessManager _businessManager;
        private DetectorsDataAccess _dataAccess;
        private EventLoggerAccess _eventLoggerAccess;
        private string _logMessagePrefix = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
            _eventLoggerAccess = new EventLoggerAccess();
            _eventLoggerAccess.LogMessageUpdate += new LogMessageUpdateHandler(LogMessage);
            _eventLoggerAccess.LogInfo("DetectorsApp Started");
            _dataAccess = new DetectorsDataAccess(_eventLoggerAccess);
            _businessManager = new BusinessManager(_dataAccess, _eventLoggerAccess);
            _dataAccess.Connect();
            CurrentLineIdTextBlk.DataContext = _dataAccess.Detectors;
            TestAPCS.Content = new TestAPCS(_dataAccess);
            TestNCB.Content = new TestNCB(_dataAccess, _businessManager);
            TestAPCS.Visibility = AppConfiguration.ShowDebugDisplays ? Visibility.Visible : Visibility.Collapsed;
            TestNCB.Visibility = TestAPCS.Visibility;
        }

        private void LogMessage(DateTime timeStamp, string message)
        {
            const string LineEnd = "\r";
            if (string.IsNullOrWhiteSpace(message))
                message = "no message";
            message = message.Replace("\n", LineEnd);
            message = message.Replace(LineEnd + LineEnd, LineEnd);
            while (message.EndsWith(LineEnd))
                message = message.Substring(0, message.Length - 1);
            string text = _logMessagePrefix + timeStamp.ToString() + ": " + message;
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate() { LogArea.AppendText(text); }));
            if (/*no embedded line end(s)?*/ message.IndexOf(LineEnd) < 0)
                _logMessagePrefix = LineEnd;
            else /*complex message; next time add blank line*/
                _logMessagePrefix = LineEnd + LineEnd;
        }

        private void Window_Closed(object sender, EventArgs eventArguments)
        {
            DetectorsApp.TestAPCS.SpeedMsgStop();
            try
            {
                if (/*exists (avoid first try exceptions)?*/ _businessManager != null)
                    _businessManager.Dispose();
            }
            catch { }
            finally { _businessManager = null; }
            try
            {
                if (/*exists (avoid first try exceptions)?*/_dataAccess != null)
                    _dataAccess.Dispose();
            }
            catch { }
            finally { _dataAccess = null; }
            _eventLoggerAccess.LogWarning(Utilities.ProcessKill(Process.GetCurrentProcess().Id, true));
            _eventLoggerAccess = null;
        }
    }
}
