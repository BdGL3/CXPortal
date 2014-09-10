using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using L3.Cargo.Common.Dashboard;
using L3.Cargo.Communications.Dashboard.Display.Host;

namespace L3.Cargo.Detectors.Display.Common
{
    /// <summary>
    /// Interaction logic for InfoDisplay.xaml
    /// </summary>
    public partial class InfoDisplay : UserControl
    {
        private Dispatcher _Dispatcher;

        private EndpointAddress _EndpointAddress;

        private WidgetStatusHost _WidgetStatusHost;

        private ObservableCollection<int> _badDetectorsList;

        public InfoDisplay (Dispatcher dispatcher, EndpointAddress address, WidgetStatusHost widgetStatusHost)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);;

            _Dispatcher = dispatcher;
            _EndpointAddress = address;
            _WidgetStatusHost = widgetStatusHost;
            _WidgetStatusHost.WidgetUpdateEvent += new WidgetUpdateHandler(WidgetUpdate);

            _badDetectorsList = new ObservableCollection<int>();
            BadDetectorsListView.DataContext = _badDetectorsList;
        }

        private void Display_MouseOrTouchDown(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void ContentArea_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void WidgetUpdate (string name, int value)
        {
            if (name.Equals("BAD_DETECTORS"))
            {
                _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    _badDetectorsList.Add(value);
                }));
            }                                  
        }

    }
}
