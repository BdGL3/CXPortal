using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using L3.Cargo.Communications.Dashboard.Display.Host;
using L3.Cargo.Common.Dashboard;
using L3.Cargo.Controls;

namespace L3.Cargo.Linac.Display.Common
{
    /// <summary>
    /// Interaction logic for LinacModulatorSolenoid.xaml
    /// </summary>
    public partial class LinacModulatorSolenoid : UserControl
    {
        #region Private Members

        private const string _MD_HVPS_VOLTAGE = "MD_HVPS_VOLTAGE";
        private const string _MD_HEATER_VOLTAGE = "MD_HEATER_VOLTAGE";
        private const string _MD_HEATER_CURRENT = "MD_HEATER_CURRENT";
        private const string _MD_STATE = "MD_STATE";
        private const string _MAGNETRON_CURRENT = "MAGNETRON_CURRENT";
        private const string _SOLENOID_CURRENT = "SOLENOID_CURRENT";
        private const string _SOLENOID_VOLTAGE = "SOLENOID_VOLTAGE";

        private Dispatcher _Dispatcher;

        private WidgetStatusHost _WidgetStatusHost;

        #endregion Private Members


        #region Public Members

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

        #endregion Public Members


        #region Constructors

        public LinacModulatorSolenoid ()
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);
        }

        public LinacModulatorSolenoid(Dispatcher dispatcher, WidgetStatusHost widgetStatusHost)
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
            string val = "";
            if (value != int.MinValue)
            {
                float adjustedFloat = Convert.ToSingle(value) / 100;
                val = adjustedFloat.ToString();
            }
            if (name.Equals(_MD_HVPS_VOLTAGE))
            {
                SetTextFromString(MD_HVPS_Voltage_Text, val);
            }
            else if (name.Equals(_MD_HEATER_VOLTAGE))
            {
                SetTextFromString(MD_Filament_Voltage_Text, val);
            }
            else if (name.Equals(_MD_HEATER_CURRENT))
            {
                SetTextFromString(MD_Filament_Current_Text, val);
            }
            else if (name.Equals(_MD_STATE))
            {
                SetTextFromResource(MD_State_Text, OpcTags.LINAC_ENERGY_TYPE_STATE.ResourceName + "_" + value.ToString());
            }
            else if (name.Equals(_MAGNETRON_CURRENT))
            {
                SetTextFromString(Magnetron_Current_Text, val);
            }
            else if (name.Equals(_SOLENOID_CURRENT))
            {
                SetTextFromString(Solenoid_Current_Text, val);
            }
            else if (name.Equals(_SOLENOID_VOLTAGE))
            {
                SetTextFromString(Solenoid_Voltage_Monitor_Text, val);
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
                    textBlock.Text = "";
                }
                else
                {
                    var binding = new Binding(resourcename);
                    binding.Source = CultureResources.getDataProvider();
                    BindingOperations.SetBinding(textBlock, TextBlock.TextProperty, binding);
                }
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
