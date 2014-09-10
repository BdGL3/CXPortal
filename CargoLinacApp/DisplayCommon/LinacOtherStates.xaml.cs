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
    /// Interaction logic for LinacGunDriver.xaml
    /// </summary>
    public partial class LinacOtherStates : UserControl
    {
        #region Private Members

        private const string _STEERING_1_CURRENT = "STEERING_X_CURRENT";
        private const string _STEERING_2_CURRENT = "STEERING_Y_CURRENT";

        private const string _IONPUMP_1_CURRENT = "IONPUMP_1_CURRENT";
        private const string _IONPUMP_2_CURRENT = "IONPUMP_2_CURRENT";
        private const string _IONPUMP_3_CURRENT = "IONPUMP_3_CURRENT";

        private const string _IONPUMP_1_VOLTAGE = "IONPUMP_1_VOLTAGE";
        private const string _IONPUMP_2_VOLTAGE = "IONPUMP_2_VOLTAGE";
        private const string _IONPUMP_3_VOLTAGE = "IONPUMP_3_VOLTAGE";

        private const string _TCU_TEMPERATURE = "TCU_TEMPERATURE";
        private const string _REFLECTED_POWER = "REFLECTED_POWER";
        private const string _FORWARD_POWER = "FORWARD_POWER";
        private const string _STEPPER_PV = "Stepper_PV";
        private const string _AFC_PV = "AFC_PV";

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

        public LinacOtherStates ()
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);
        }

        public LinacOtherStates(Dispatcher dispatcher, WidgetStatusHost widgetStatusHost)
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
            if (name.Equals(_STEERING_1_CURRENT))
            {
                SetTextFromString(Steering_1_Current_Text, val);
            }
            else if (name.Equals(_STEERING_2_CURRENT))
            {
                SetTextFromString(Steering_2_Current_Text, val);
            }
            else if (name.Equals(_STEPPER_PV))
            {
                SetTextFromString(Stepper_PV_Text, val);
            }
            else if (name.Equals(_IONPUMP_1_CURRENT))
            {
                SetTextFromString(Ion_Pump_1_Current_Text, val);
            }
            else if (name.Equals(_IONPUMP_2_CURRENT))
            {
                SetTextFromString(Ion_Pump_2_Current_Text, val);
            }
            else if (name.Equals(_IONPUMP_3_CURRENT))
            {
                SetTextFromString(Ion_Pump_3_Current_Text, val);
            }
            else if (name.Equals(_IONPUMP_1_VOLTAGE))
            {
                SetTextFromString(Ion_Pump_1_Voltage_Text, val);
            }
            else if (name.Equals(_IONPUMP_2_VOLTAGE))
            {
                SetTextFromString(Ion_Pump_2_Voltage_Text, val);
            }
            else if (name.Equals(_IONPUMP_3_VOLTAGE))
            {
                SetTextFromString(Ion_Pump_3_Voltage_Text, val);
            }
            //else if (name.Equals(_TCU_TEMPERATURE))
            //{
            //    SetTextFromString(TCU_Temperature_Text, value.ToString());
            //}
            else if (name.Equals(_REFLECTED_POWER))
            {
                SetTextFromString(Reflected_Power_Text, val);
            }
            else if (name.Equals(_FORWARD_POWER))
            {
                SetTextFromString(Forward_Power_Text, val);
            }
            else if (name.Equals(_AFC_PV))
            {
                SetTextFromString(AFC_PV_Text, val);
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
