using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using L3.Cargo.Common.Dashboard;
using L3.Cargo.Communications.Dashboard.Display.Host;

namespace L3.Cargo.Linac.Display.Common
{
    /// <summary>
    /// Interaction logic for LinacState.xaml
    /// </summary>
    public partial class LinacState : UserControl
    {
        private Dispatcher _Dispatcher;

        private WidgetStatusHost _WidgetStatusHost;


        public Dispatcher Dispatcher
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
                _WidgetStatusHost.WidgetUpdateEvent += new WidgetUpdateHandler(WidgetUpdate);
            }
        }

        public LinacState()
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);
        }

        public LinacState (Dispatcher dispatcher, WidgetStatusHost widgetStatusHost)
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);

            _Dispatcher = dispatcher;
            _WidgetStatusHost = widgetStatusHost;
            _WidgetStatusHost.WidgetUpdateEvent += new WidgetUpdateHandler(WidgetUpdate);
        }

        private void WidgetUpdate (string name, int value)
        {
            if (name.Equals(OpcTags.LINAC_STATUS.Name))
            {
                string iconName = (value == 0) ? "Clear" : (value == 1) ? "MajorFailure" : (value == 2) ? "Failure" : "error";
                SetImageFromResource(CurrentStatusImage, iconName + ".ico");
                SetTextFromResource(CurrentStatusText, OpcTags.LINAC_STATUS.ResourceName + "_WIDGET_" + value.ToString());
            }
            
            if (name.Equals(OpcTags.LINAC_STATE.Name))
            {
                string iconName = (value == 0) ? "SwitchOff" : (value == 1) ? "SwitchOn" : (value == 2) ? "WarmUp" : (value == 3) ? "Waiting" : (value == 4) ? "XraysOff" : (value == 5) ? "XraysOn" : "error";
                SetImageFromResource(LinacStatusImage, iconName + ".ico");
                string resKey = OpcTags.LINAC_STATE.ResourceName + "_" + value.ToString();
                SetTextFromResource(LinacStatusText, resKey);

                // change the text style for the xray state
                if (resKey.Equals("LINAC_STATE_5")) 
                {
                    _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                    {
                        LinacStatusText.Foreground = Brushes.Red;
                        LinacStatusText.FontWeight = FontWeights.Bold;
                    }));
                }
                else
                {
                    _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                    {
                        LinacStatusText.Foreground = Brushes.Black;
                        LinacStatusText.FontWeight = FontWeights.Normal;
                    }));
                }
            }

            if (name.Equals(OpcTags.LINAC_ENERGY_TYPE_STATE.Name))
            {
                string iconName = "DualEnergy";

                switch (value)
                {
                    case 0:
                        iconName = "DualEnergy";
                        break;
                    case 1:
                        iconName = "HighEnergy";
                        break;
                    case 2:
                        iconName = "LowEnergy";
                        break;
                    case 3:
                        iconName = "LowDoseLowEnergy";
                        break;
                }

                _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    SetImageFromResource(EnergyStateImage, iconName + ".ico");
                    SetTextFromResource(EnergyStateText, OpcTags.LINAC_ENERGY_TYPE_STATE.ResourceName + "_" + value.ToString());
                }));
            }
        }

        private void SetImageFromResource (Image image, string iconName)
        {
            _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                image.Source = new BitmapImage(new Uri("/L3.Cargo.Linac.Display.Common;component/Resources/" + iconName, UriKind.Relative));
            }));
        }

        private void SetTextFromResource (TextBlock textBlock, string resourcename)
        {
            _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                string val = L3.Cargo.Linac.Display.Common.Resources.ResourceManager.GetString(resourcename);
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
    }
}
