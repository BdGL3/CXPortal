using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using L3.Cargo.Common.Dashboard;
using L3.Cargo.Communications.Dashboard.Display.Host;

namespace L3.Cargo.Scan.Display.Common
{
    /// <summary>
    /// Interaction logic for ScanDriveState.xaml
    /// </summary>
    public partial class ScanDriveState : UserControl
    {
        #region Private Members

        private Dispatcher _Dispatcher;

        private WidgetStatusHost _WidgetStatusHost;

        #endregion Private Members


        #region Public Members

        public Dispatcher UIDispatcher
        {
            set
            {
                _Dispatcher = value;
            }
        }

        public WidgetStatusHost WidgetStatusHost
        {
            set
            {
                if (_WidgetStatusHost != null)
                {
                    _WidgetStatusHost.WidgetUpdateEvent -= new WidgetUpdateHandler(WidgetUpdate);
                }

                _WidgetStatusHost = value;

                if (_WidgetStatusHost != null)
                {
                    _WidgetStatusHost.WidgetUpdateEvent += new WidgetUpdateHandler(WidgetUpdate);
                }
            }
        }

        #endregion Public Members


        #region Constructors

        public ScanDriveState ()
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);
        }

        public ScanDriveState (Dispatcher dispatcher, WidgetStatusHost widgetStatusHost)
        {
            InitializeComponent();

            _Dispatcher = dispatcher;
            _WidgetStatusHost = widgetStatusHost;
            _WidgetStatusHost.WidgetUpdateEvent += new WidgetUpdateHandler(WidgetUpdate);
        }
        
        #endregion Constructors


        #region Private Methods

        private void WidgetUpdate (string name, int value)
        {
            /*
            if (name.Equals(OpcTags.SCAN_DRIVE_HYDRAULIC_OIL_LEVEL.Name))
            {
                SetTextFromString(OilLevelText, ((double)value / 100.0).ToString("N2"));
            }
            else if (name.Equals(OpcTags.SCAN_DRIVE_HYDRAULIC_OIL_TEMPERATURE.Name))
            {
                SetTextFromString(OilTemperatureText, value.ToString());
            }
            else if (name.Equals(OpcTags.SCAN_DRIVE_STATE.Name))
            {
                SetTextFromResource(ScanDriveStateText, OpcTags.SCAN_DRIVE_STATE.ResourceName + "_" + value.ToString());
            }
             * */
        }

        private void SetImageFromResource (Image image, string iconName)
        {
            _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                image.Source = new BitmapImage(new Uri("/L3.Cargo.Scan.Display.Common;component/Resources/" + iconName, UriKind.Relative));
            }));
        }

        private void SetTextFromResource (TextBlock textBlock, string resourcename)
        {
            _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                string val = L3.Cargo.Scan.Display.Common.Resources.ResourceManager.GetString(resourcename);
                if (String.IsNullOrWhiteSpace(val))
                {
                    resourcename = "UNKNOWN_RESOURCE";
                }
                var binding = new Binding(resourcename);
                binding.Source = CultureResources.getDataProvider();
                BindingOperations.SetBinding(textBlock, TextBlock.TextProperty, binding);
            }));
        }

        private void SetTextFromString (TextBlock textBlock, string stringText)
        {
            _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                textBlock.Text = stringText;
            }));
        }

        #endregion Private Methods
    }
}
