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

namespace L3.Cargo.Safety.Display.Common
{
    /// <summary>
    /// Interaction logic for ResetFaults.xaml
    /// </summary>
    public partial class PerimeterMode : UserControl
    {
        private Dispatcher _Dispatcher;

        private EndpointAddress _EndpointAddress;

        private WidgetStatusHost _WidgetStatusHost;

        public PerimeterMode (Dispatcher dispatcher, EndpointAddress address, WidgetStatusHost widgetStatusHost)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);

            _Dispatcher = dispatcher;
            _EndpointAddress = address;
            _WidgetStatusHost = widgetStatusHost;
            _WidgetStatusHost.WidgetUpdateEvent += new WidgetUpdateHandler(WidgetUpdate);
        }

        private void WidgetUpdate (string name, int value)
        {
            if (name.Equals(OpcTags.PERIMETER_MODE.Name))
            {
                _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    Perimeter_Control.IsChecked = Convert.ToBoolean(value);
                }));
            }
        }

        private void Perimeter_Control_TouchDown(object sender, RoutedEventArgs e)
        {
            Perimeter_Control.IsChecked = !Perimeter_Control.IsChecked;
            Perimeter_Control_Click(sender, e);
        }

        private void Perimeter_Control_Click (object sender, RoutedEventArgs e)
        {
            if (Perimeter_Control.IsChecked == true)
            {
                SendRequest(OpcTags.PERIMETER_MODE.Name, 1);
            }
            else
            {
                SendRequest(OpcTags.PERIMETER_MODE.Name, 0);
            }
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
    }
}
