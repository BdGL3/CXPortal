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
    public partial class ScanState : UserControl
    {
        #region Private Members

        private Dispatcher _Dispatcher;

        private WidgetStatusHost _WidgetStatusHost;

        private bool _CalibrationMode;

        private int _ScanStateValue;

        private int _CalibrationStateValue;

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

        public ScanState ()
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);
        }

        public ScanState (Dispatcher dispatcher, WidgetStatusHost widgetStatusHost)
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
            if (name.Equals(OpcTags.SCAN_STATE.Name) && !_CalibrationMode)
            {
                _ScanStateValue = value;
                SetTextFromResource(ScanStateText, OpcTags.SCAN_STATE.ResourceName + "_" + value.ToString());
            }
            else if (name.Equals(OpcTags.CALIBRATION_STATE.Name) && _CalibrationMode)
            {
                _CalibrationStateValue = value;
                SetTextFromResource(ScanStateText, OpcTags.CALIBRATION_STATE.ResourceName + "_" + value.ToString());
            }
            else if (name.Equals(OpcTags.CALIBRATION_SCAN.Name))
            {
                _CalibrationMode = Convert.ToBoolean(value);
                if (_CalibrationMode)
                {
                    WidgetUpdate(OpcTags.CALIBRATION_STATE.Name, _CalibrationStateValue);
                }
                else
                {
                    WidgetUpdate(OpcTags.SCAN_STATE.Name, _ScanStateValue);
                }

            }
            else if (name.Equals(OpcTags.SCAN_AREA_CLEAR.Name))
            {
                string iconName = (value == 1) ? "AreaClear" : (value == 0) ? "AreaNotClear" : "error";
                SetImageFromResource(ScanAreaImage, iconName + ".ico");
                SetTextFromResource(ScanAreaText, OpcTags.SCAN_AREA_CLEAR.ResourceName + "_" + value.ToString());
            }
            else if (name.Equals(OpcTags.SCAN_STEP.Name))
            {
                SetTextFromString(ScanDriveStateText, value.ToString());
            }
            else if (name.Equals(OpcTags.SCAN_STEP_LAST.Name))
            {
                /* This was updated for the Portal Mode
                string iconName = (value == 0) ? "Stop" : (value == 1) ? "Forward" : (value == 2) ? "Reverse" : "error";
                SetImageFromResource(DirectionImage, iconName + ".ico");
                 * */
                SetTextFromString(LastStepText, value.ToString());
            }
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
