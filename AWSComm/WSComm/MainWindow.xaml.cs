using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows;
using L3.Cargo.Communications.Host;
using System.Windows.Media;
using System.Windows.Threading;
using System.IO;
using L3.Cargo.Communications.Client;
using L3.Cargo.Common;
using System.ComponentModel;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace L3.Cargo.WSCommunications
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        #region Private Members

        private CaseWSCollection caseWSCollection;

        private CargoHostEndPoint m_CargoHostEndPoint_Impl;

        private WSServer m_WSServer_Impl;

        private TIPClientManager m_TIPClientManager_Impl;

        private Mutex mutex;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow (IntPtr hWnd);

        #endregion Private Members


        #region Constructors

        public MainWindow()
        {
            bool createdNew = true;

            mutex = new Mutex(true, "WSComm", out createdNew);

            if (createdNew)
            {
                InitializeComponent();

                caseWSCollection = (CaseWSCollection)FindResource("casewscollection");

                m_CargoHostEndPoint_Impl = new CargoHostEndPoint(ConfigurationManager.AppSettings["host"],
                                                     Int32.Parse(ConfigurationManager.AppSettings["port"]));

                m_WSServer_Impl = new WSServer(m_CargoHostEndPoint_Impl,
                                               ConfigurationManager.AppSettings["Alias"],
                                               ConfigurationManager.AppSettings["ConnectionUri"],
                                               ConfigurationManager.AppSettings["AllowedIPList"],
                                               ConfigurationManager.AppSettings["UserProfiles"],
                                 Boolean.Parse(ConfigurationManager.AppSettings["EnableArchiveDecision"]));

                m_WSServer_Impl.CaseListUpdateEvent += new WSServer.CaseListUpdateHandler(UpdateCaseList);
                m_WSServer_Impl.HostStatusEvent += new WSServer.HostStatusHandler(UpdateHostStatus);

                if (Boolean.Parse(ConfigurationManager.AppSettings["EnableTIP"]))
                {
                    m_TIPClientManager_Impl = new TIPClientManager(m_WSServer_Impl,
                                                                   ConfigurationManager.AppSettings["Alias"],
                                                                   ConfigurationManager.AppSettings["CTITemplateDirectory"]);
                }

                CargoHostNameText.Text = ConfigurationManager.AppSettings["host"] + ": Disconnected";

                if (Boolean.Parse(ConfigurationManager.AppSettings["EnableMCArea"]))
                {
                    MCAreaEnable_Click(this, new RoutedEventArgs());
                }
                else
                {
                    MCAreaDisable_Click(this, new RoutedEventArgs());
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

        private void UpdateItem(String caseId, String awsId, String area, Boolean additem)
        {
            Boolean caseFound = false;

            if (String.IsNullOrWhiteSpace(caseId))
            {
                foreach (CaseWS caseWS in caseWSCollection)
                {
                    if (caseWS.WS.Equals(awsId))
                    {
                        caseWS.WS = string.Empty;
                    }
                }
            }
            else
            {
                foreach (CaseWS caseWS in caseWSCollection)
                {
                    if (caseWS.Case.Equals(caseId))
                    {
                        if (additem)
                        {
                            Int32 index = caseWSCollection.IndexOf(caseWS);

                            if (area == string.Empty)
                            {
                                area = caseWSCollection[index].Area;
                            }

                            caseWSCollection.RemoveAt(index);

                            caseWS.WS = awsId;
                            caseWS.Area = area;
                            caseWSCollection.Insert(index, caseWS);
                        }
                        else
                        {
                            caseWSCollection.Remove(caseWS);
                        }
                        caseFound = true;

                        break;
                    }
                }

                if (!caseFound && additem)
                {
                    CaseWS caseWS = new CaseWS();
                    caseWS.Case = caseId;
                    caseWS.WS = awsId;
                    caseWS.Area = area;
                    caseWSCollection.Add(caseWS);
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            m_WSServer_Impl.StartUp();
            if (m_TIPClientManager_Impl != null)
            {
                m_TIPClientManager_Impl.StartUp();
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (m_TIPClientManager_Impl != null)
            {
                m_TIPClientManager_Impl.ShutDown();
            }

            if (m_WSServer_Impl != null)
            {
                m_WSServer_Impl.ShutDown();
            }

            if (mutex != null)
            {
                mutex.Dispose();
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            m_WSServer_Impl.ShutDown();
            Application.Current.Shutdown(0);
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            About aboutdlg = new About();
            aboutdlg.ShowDialog();
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            NetworkConfig networkConfig = new NetworkConfig();
            networkConfig.ShowDialog();
        }

        private void MCAreaEnable_Click(object sender, RoutedEventArgs e)
        {
            m_WSServer_Impl.AutoVerifyManualCodingCaseEnabled = false;                
            ManualCodingStatus.Fill = Brushes.Green;
            ManualCodingText.Text = "Manual Coding Area: Enabled";
            MCAreaEnable.Visibility = System.Windows.Visibility.Collapsed;
            MCAreaDisable.Visibility = System.Windows.Visibility.Visible;

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove("EnableMCArea");
            config.AppSettings.Settings.Add("EnableMCArea", "True");
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private void MCAreaDisable_Click(object sender, RoutedEventArgs e)
        {
            m_WSServer_Impl.AutoVerifyManualCodingCaseEnabled = true;
            ManualCodingStatus.Fill = Brushes.Red;
            ManualCodingText.Text = "Manual Coding Area: Disabled";
            MCAreaEnable.Visibility = System.Windows.Visibility.Visible;
            MCAreaDisable.Visibility = System.Windows.Visibility.Collapsed;

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove("EnableMCArea");
            config.AppSettings.Settings.Add("EnableMCArea", "False");
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        #endregion Private Methods


        #region Public Methods

        public void UpdateHostStatus(Boolean Connected)
        {
            ConnectStatus.Dispatcher.Invoke(DispatcherPriority.Normal,
                new Action(
                    delegate()
                    {
                        if (!Connected)
                        {
                            ConnectStatus.Fill = Brushes.Red;
                            ConnectStatus.ToolTip = "Disconnected From Host";
                            CargoHostNameText.Text = ConfigurationManager.AppSettings["host"] + ": Disconnected";
                            caseWSCollection.Clear();
                        }
                        else
                        {
                            ConnectStatus.Fill = Brushes.Green;
                            ConnectStatus.ToolTip = "Connected To Host";
                            CargoHostNameText.Text = ConfigurationManager.AppSettings["host"] + ": Connected";
                        }
                    }));

        }

        public void UpdateCaseList(String caseId, String awsId, String area, Boolean additem)
        {
            Dispatcher.BeginInvoke(new Action<String, String, String, Boolean>(UpdateItem), caseId, awsId, area, additem);
        }

        #endregion Public Methods
    }

    public class CaseWSCollection : ObservableCollection<CaseWS>
    {
    }

    public class CaseWS : INotifyPropertyChanged
    {
        private String m_Case;
        private String m_WS;
        private String m_Area;

        public String Case 
        {
            get { return m_Case; }
            set
            {
                m_Case = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Case"));
            }
        }

        public String WS 
        {
            get { return m_WS; }
            set
            {
                m_WS = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("WS"));
            }
        }

        public String Area 
        {
            get { return m_Area; }
            set
            {
                m_Area = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Area"));
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, e);
        }

        #endregion
    }
}
