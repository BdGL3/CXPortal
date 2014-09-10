using System;
using System.Configuration;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

//using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.OCR.Test;

namespace OcrTestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Members

        private OCRTestServer _testServer;

        private bool _runningServerModeIsClient = false;

        #endregion Private Members


        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }


        private void MainWindow_Loaded (object sender, RoutedEventArgs e)
        {
            try
            {
                _testServer = new OCRTestServer(IPAddress.Parse("127.0.0.1"),
                                                2056,
                                                "OCRS",
                                                "OCRS"
                                               );
            }
            catch (Exception ex)
            {
                MessageBox.Show("new OCRTestServer() threw exception: " + ex.Message);
            }

            try
            {
                ReceivedMessage.DataContext = _testServer;
                ConnectionState.DataContext = _testServer;
                LogArea.ItemsSource = _testServer.Logger;
            }
            catch (Exception ex)
            {
                MessageBox.Show("setting DataContexts threw exception: " + ex.Message);
            }
        }

        #endregion


        #region Private Methods

        private void StartTestBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_testServer.IsRunning)
                _testServer.Shutdown();

            if ((bool)ClientModeRBtn.IsChecked)
            {
                _runningServerModeIsClient = true;
                _testServer.StartClientMode();
            }
            else
            {
                _runningServerModeIsClient = false;
                _testServer.StartListenerMode();
            }
        }

        private void UnregisterBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_testServer.IsRunning)
                _testServer.SendUnregisterMessage();
        }

        private void ClientModeRBtn_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton clientModeBtn = e.Source as RadioButton;

            if (_testServer != null && (bool)clientModeBtn.IsChecked)
            {
                if (_testServer.IsRunning && !_runningServerModeIsClient)
                    _testServer.Shutdown();
            }
        }

        private void ListenerModeRBtn_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton listenerModeBtn = e.Source as RadioButton;

            if (_testServer != null && (bool)listenerModeBtn.IsChecked)
            {
                if (_testServer.IsRunning && _runningServerModeIsClient)
                    _testServer.Shutdown();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
        }

        #endregion
    }
}
