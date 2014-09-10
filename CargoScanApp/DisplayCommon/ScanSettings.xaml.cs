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
    /// Interaction logic for LinacState.xaml
    /// </summary>
    public partial class ScanSettings : UserControl
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

        public ScanSettings ()
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);
        }

        public ScanSettings (Dispatcher dispatcher, WidgetStatusHost widgetStatusHost)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);

            _Dispatcher = dispatcher;
            _WidgetStatusHost = widgetStatusHost;
            _WidgetStatusHost.WidgetUpdateEvent += new WidgetUpdateHandler(WidgetUpdate);
        }

        #endregion Constructors


        #region Private Methods

        private void WidgetUpdate (string name, int value)
        {
            if (name.Equals(OpcTags.CALIBRATION_SCAN.Name))
            {
                string iconName = (value == 0) ? "ScanMode" : (value == 1) ? "CalibrationMode" : "error";
                SetImageFromResource(ScanTypeImage, iconName + ".ico");
                SetTextFromResource(ScanTypeText, OpcTags.CALIBRATION_SCAN.ResourceName + "_" + value.ToString());
            }
            else if (name.Equals(OpcTags.SCAN_MULTIPLE_OBJECTS.Name))
            {
                string iconName = (value == 1) ? "Multiscan" : (value == 0) ? "Single" : "error";
                SetImageFromResource(ScanModeImage, iconName + ".ico");
                SetTextFromResource(ScanModeText, OpcTags.SCAN_MULTIPLE_OBJECTS.ResourceName + "_" + value.ToString());
            }
            /*else if (name.Equals(OpcTags.SCAN_DRIVE_SELECTED_SPEED.Name))
            {
                string iconName = (value == 0) ? "CustomSpeed" : (value == 1) ? "SlowSpeed" : (value == 2) ? "MediumSpeed" : (value == 3) ? "FastSpeed" : "Error";
                SetImageFromResource(SelectedSpeedImage, iconName + ".ico");
                SetTextFromResource(SelectedSpeedText, OpcTags.SCAN_DRIVE_SELECTED_SPEED.ResourceName + "_" + value.ToString());
            }*/
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

        #endregion Private Methods
    }
}
