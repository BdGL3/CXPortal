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
using System.Windows.Threading;
using System.ServiceModel;
using L3.Cargo.Communications.Dashboard.Display.Host;
using L3.Cargo.Communications.Dashboard.Display.Client;
using L3.Cargo.Communications.Common;

namespace L3.Cargo.Detectors.Display.Common
{
    /// <summary>
    /// Interaction logic for StartCalibrationControls.xaml
    /// </summary>
    public partial class StartCalibrationControls : UserControl
    {
        #region Private Members

        private Dispatcher _Dispatcher;

        private EndpointAddress _EndpointAddress;

        private WidgetStatusHost _WidgetStatusHost;

        private bool _ScanAreaClear = false;

        private int _CalibrationState = 0;

        private bool _CalibrationScan = false;

        private int _LinacState = 0;

        #endregion Private Members


        #region Constructors

        public StartCalibrationControls(Dispatcher dispatcher, EndpointAddress address, WidgetStatusHost widgetStatusHost)
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

        public static string TextTidy(string /*text to be tidied*/ tdyTxt)
        {
            if (string.IsNullOrWhiteSpace(tdyTxt))
                tdyTxt = string.Empty;
            tdyTxt = tdyTxt.Trim();
            tdyTxt = tdyTxt.Replace("\n", "\r");
            while (tdyTxt.IndexOf("\r\r") >= 0)
                tdyTxt = tdyTxt.Replace("\r\r", "\r");
            return tdyTxt;
        }

        private void WidgetUpdate(string name, int value)
        {
            bool updateControls = false;
            if (name.Equals(OpcTags.SCAN_AREA_CLEAR.Name))
            {
                _ScanAreaClear = Convert.ToBoolean(value);
                updateControls = true;
            }
            else if (name.Equals(OpcTags.CALIBRATION_SCAN.Name))
            {
                _CalibrationScan = Convert.ToBoolean(value);
                updateControls = true;
            }
            else if (name.Equals(OpcTags.CALIBRATION_STATE.Name))
            {
                _CalibrationState = value;
                updateControls = true;
            }
            else if (name.Equals(OpcTags.LINAC_STATE.Name))
            {
                _LinacState = value;
                updateControls = true;
            }


            if (updateControls)
            {
                _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    Calibration_Control.IsEnabled = _CalibrationState == 0 && _ScanAreaClear && _CalibrationScan;
                    Calibration_Control.IsChecked = (_CalibrationState != 0) && (_LinacState == 4);
                }));
            }
        }

        private void Calibration_Control_TouchDown(object sender, RoutedEventArgs e)
        {
            Calibration_Control.IsChecked = !Calibration_Control.IsChecked;
            Calibration_Control_Click(sender, e);
        }

        private void Calibration_Control_Click(object sender, RoutedEventArgs e)
        {
            if (Calibration_Control.IsChecked == true)
            {
                SendRequest(OpcTags.CALIBRATION_STATE.Name, 99);
            }
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
                MessageBox.Show(TextTidy(ex.ToString()), "Anomaly!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion Private Methods
    }
}
