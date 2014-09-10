using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using L3.Cargo.Common.Dashboard;
using L3.Cargo.Communications.Dashboard.Display.Host;

namespace L3.Cargo.Scan.Display.Common
{
    /// <summary>
    /// Interaction logic for Indicator.xaml
    /// </summary>
    public partial class Indicator : UserControl, IDisposable
    {
        #region Private Members

        private StatusHost _StatusHost;

        private Dispatcher _Dispatcher;

        #endregion Private Members


        #region Constructors

        public Indicator (StatusHost statusHost, Dispatcher dispatcher)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);

            _Dispatcher = dispatcher;

            _StatusHost = statusHost;
            _StatusHost.IndicatorUpdate += new UpdateIndicatorHandler(UpdateIndicator);
        }

        #endregion Constructors


        #region Private Methods

        private void UpdateIndicator(string color)
        {
            _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    IndicatorColor.Color = (Color)ColorConverter.ConvertFromString(color);
                }));
        }

        #endregion Private Methods


        #region Public Methods

        public void Dispose()
        {
            _StatusHost.IndicatorUpdate -= new UpdateIndicatorHandler(UpdateIndicator);
        }

        #endregion Public Methods
    }
}
