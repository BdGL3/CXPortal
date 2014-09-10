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
    public partial class ScanTypeControls : UserControl
    {
        #region Private Members

        private Dispatcher _Dispatcher;

        private EndpointAddress _EndpointAddress;

        private WidgetStatusHost _WidgetStatusHost;

        #endregion Private Members


        #region Constructors

        public ScanTypeControls (Dispatcher dispatcher, EndpointAddress address, WidgetStatusHost widgetStatusHost)
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

        private void WidgetUpdate (string name, int value)
        {
            if (name.Equals(OpcTags.CALIBRATION_SCAN.Name))
            {
                _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    Type_Control.IsChecked = Convert.ToBoolean(value);
                }));
            }
            else if (name.Equals(OpcTags.SCAN_STATE.Name))
            {
                _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    Type_Control.IsEnabled = (value == 0);
                }));
            }
        }

        private void Type_Control_TouchDown(object sender, RoutedEventArgs e)
        {
            Type_Control.IsChecked = !Type_Control.IsChecked;
            Type_Control_Click(sender, e);
        }

        private void Type_Control_Click (object sender, RoutedEventArgs e)
        {
            int value = (Type_Control.IsChecked == true) ? 1 : 0;
            SendRequest(OpcTags.CALIBRATION_SCAN.Name, value);
        }

        private void SendRequest (string name, int value)
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
