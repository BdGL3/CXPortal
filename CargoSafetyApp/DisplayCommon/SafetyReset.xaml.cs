using System;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using L3.Cargo.Common.Dashboard;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.Dashboard.Display.Client;

namespace L3.Cargo.Safety.Display.Common
{
    /// <summary>
    /// Interaction logic for ResetFaults.xaml
    /// </summary>
    public partial class ResetFaults : UserControl
    {
        private Dispatcher _Dispatcher;

        private EndpointAddress _EndpointAddress;

        public ResetFaults(Dispatcher dispatcher, EndpointAddress address)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);

            _Dispatcher = dispatcher;
            _EndpointAddress = address;
        }

        private void Reset_Faults_Click (object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(new ParameterizedThreadStart(delegate
            {
                SendRequest(OpcTags.SAFETY_RESET.Name, 1);
            }));
            thread.IsBackground = true;
            thread.Start();
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
