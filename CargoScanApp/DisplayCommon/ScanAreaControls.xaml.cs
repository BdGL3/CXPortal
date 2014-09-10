using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using L3.Cargo.Common.Dashboard;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.Dashboard.Display.Client;
using L3.Cargo.Communications.Dashboard.Display.Host;

namespace L3.Cargo.Scan.Display.Common
{
    /// <summary>
    /// Interaction logic for EnergySelection.xaml
    /// </summary>
    public partial class ScanAreaControls : UserControl
    {
        #region Private Members

        private Dispatcher _Dispatcher;

        private EndpointAddress _EndpointAddress;

        private WidgetStatusHost _WidgetStatusHost;

        private bool _SiteReadyNextScan = true;

        private int _ScanState = 0;

        private int _CalibrationState;

        private bool _CalibrationScan;

        #endregion Private Members


        #region Constructors

        public ScanAreaControls(Dispatcher dispatcher, EndpointAddress address, WidgetStatusHost widgetStatusHost)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);

            _Dispatcher = dispatcher;
            _EndpointAddress = address;
            _WidgetStatusHost = widgetStatusHost;
            _WidgetStatusHost.WidgetUpdateEvent += new WidgetUpdateHandler(WidgetUpdate);
        }

        #endregion Constructors


        #region Private Methods

        private void WidgetUpdate(string name, int value)
        {
            bool updateControlenEnable = false;
            if (name.Equals(OpcTags.SCAN_AREA_CLEAR.Name))
            {
                _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    Clear_Control.IsChecked = Convert.ToBoolean(value);
                }));
            }
            else if (name.Equals(OpcTags.SCAN_STATE.Name))
            {
                _ScanState = value;
                updateControlenEnable = true;
            }
            else if (name.Equals(OpcTags.SITE_READY_NEXT_SCAN.Name))
            {
                _SiteReadyNextScan = Convert.ToBoolean(value);
                updateControlenEnable = true;
            }
            else if (name.Equals(OpcTags.CALIBRATION_STATE.Name))
            {
                _CalibrationState = value;
                updateControlenEnable = true;
            }
            else if (name.Equals(OpcTags.CALIBRATION_SCAN.Name))
            {
                _CalibrationScan = Convert.ToBoolean(value);
                updateControlenEnable = true;
            }

            if (updateControlenEnable)
            {
                bool enableButton = (_ScanState == 0 && _SiteReadyNextScan) || (_CalibrationState == 0 && _CalibrationScan);
                _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    Clear_Control.IsEnabled = enableButton;
                }));
            }

        }

        private void Clear_Control_TouchDown(object sender, RoutedEventArgs e)
        {
            Clear_Control.IsChecked = !Clear_Control.IsChecked;
            Clear_Control_Click(sender, e);
        }

        private void Clear_Control_Click(object sender, RoutedEventArgs e)
        {
            int value = (Clear_Control.IsChecked == true) ? 1 : 0;
            SendRequest(OpcTags.SCAN_AREA_CLEAR.Name, value);
        }

        private void SendRequest(string name, int value)
        {
            try
            {
                WidgetRequestEndpoint widgetRequestEndpoint = new WidgetRequestEndpoint(new TCPBinding(), _EndpointAddress);
                widgetRequestEndpoint.Open();
                widgetRequestEndpoint.Request(name, value);
                widgetRequestEndpoint.Close();
            }
            catch (Exception ex)
            {
                // TODO: log event here
            }
        }

        #endregion Private Methods
    }
}
