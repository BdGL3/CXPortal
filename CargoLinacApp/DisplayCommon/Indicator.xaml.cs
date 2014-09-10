using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using L3.Cargo.Common.Dashboard;
using L3.Cargo.Communications.Dashboard.Display.Host;

namespace L3.Cargo.Linac.Display.Common
{
    /// <summary>
    /// Interaction logic for Indicator.xaml
    /// </summary>
    public partial class Indicator : UserControl, IDisposable
    {
        private StatusHost _StatusHost;

        private Dispatcher _Dispatcher;

        public Indicator (StatusHost statusHost, Dispatcher dispatcher)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);

            _Dispatcher = dispatcher;

            _StatusHost = statusHost;
            _StatusHost.IndicatorUpdate += new UpdateIndicatorHandler(UpdateIndicator);
        }

        void UpdateIndicator(string color)
        {
            _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    IndicatorColor.Color = (Color)ColorConverter.ConvertFromString(color);
                }));
        }

        public void Dispose()
        {
            _StatusHost.IndicatorUpdate -= new UpdateIndicatorHandler(UpdateIndicator);
        }
    }
}
