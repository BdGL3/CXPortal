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

namespace L3.Cargo.Linac.Display.Common
{
    /// <summary>
    /// Interaction logic for EnergySelection.xaml
    /// </summary>
    public partial class EnergySelection : UserControl
    {
        private Dispatcher _Dispatcher;

        private EndpointAddress _EndpointAddress;

        private WidgetStatusHost _WidgetStatusHost;

        public EnergySelection (Dispatcher dispatcher, EndpointAddress address, WidgetStatusHost widgetStatusHost)
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
            if (name.Equals(OpcTags.LINAC_ENERGY_TYPE.Name) || name.Equals(OpcTags.LINAC_ENERGY_TYPE_STATE.Name))
            {
                _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    if (value == 1)
                    {
                        High_Energy.IsChecked = true;
                    }
                    else if (value == 2)
                    {
                        Low_Energy.IsChecked = true;
                    }
                    else if (value == 3)
                    {
                        Low_Dose_Low_Energy.IsChecked = true;
                    }
                    else if (value == 0)
                    {
                        Dual_Energy.IsChecked = true;
                    }
                }));
            }
        }

        private void High_Energy_Click (object sender, RoutedEventArgs e)
        {
            //SendRequest(OpcTags.LINAC_ENERGY_TYPE.Name, 1);
        }

        private void Low_Energy_Click (object sender, RoutedEventArgs e)
        {
            //SendRequest(OpcTags.LINAC_ENERGY_TYPE.Name, 2);
        }

        private void Low_Dose_Low_Energy_Click(object sender, RoutedEventArgs e)
        {
            //SendRequest(OpcTags.LINAC_ENERGY_TYPE.Name, 3);
        }

        private void Dual_Energy_Click(object sender, RoutedEventArgs e)
        {
            //SendRequest(OpcTags.LINAC_ENERGY_TYPE.Name, 0);
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
