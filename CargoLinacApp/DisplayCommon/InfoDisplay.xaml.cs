using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using L3.Cargo.Communications.Dashboard.Display.Client;
using L3.Cargo.Communications.Dashboard.Display.Host;
using L3.Cargo.Communications.Common;
using L3.Cargo.Common.Dashboard;
using System.Windows.Data;
using L3.Cargo.Controls;

namespace L3.Cargo.Linac.Display.Common
{
    /// <summary>
    /// Interaction logic for InfoDisplay.xaml
    /// </summary>
    public partial class InfoDisplay : UserControl
    {
        public InfoDisplay (Dispatcher dispatcher, WidgetStatusHost widgetStatusHost)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);

            this.linacStatesPage1.Dispatcher = dispatcher;
            this.linacStatesPage1.WidgetStatusHost = widgetStatusHost;

            this.linacStatesPage2.Dispatcher = dispatcher;
            this.linacStatesPage2.WidgetStatusHost = widgetStatusHost;

            this.linacStatesPage3.Dispatcher = dispatcher;
            this.linacStatesPage3.WidgetStatusHost = widgetStatusHost;
        }

        private void Display_MouseOrTouchDown (object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void ContentArea_MouseOrTouchDown(object sender, InputEventArgs e)
        {
            e.Handled = true;
        }

        private void PreviewTabItem_TouchDown(object sender, TouchEventArgs e)
        {
            PreviewTabControl.SelectedItem = sender as PreviewTabItem;
        }
    }
}
