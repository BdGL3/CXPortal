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
    public partial class LinacGunDriver : UserControl
    {
        #region Private Members
        
        private const string _GD_CATHODE_VOLTAGE = "GD_CATHODE_VOLTAGE";
        private const string _GD_HEATER_VOLTAGE = "GD_HEATER_VOLTAGE";
        private const string _GD_HEATER_CURRENT = "GD_HEATER_CURRENT";
        private const string _GD_GRID_DRIVE_A_VOLTAGE = "GD_GRID_DRIVE_A_VOLTAGE";
        private const string _GD_GRID_DRIVE_B_VOLTAGE = "GD_GRID_DRIVE_B_VOLTAGE";
        private const string _GD_GRID_BIAS = "GD_GRID_BIAS";
        private const string _GD_BEAM_CURRENT = "GD_BEAM_CURRENT";
        private const string _GD_STATE = "GD_STATE";

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

        public LinacGunDriver ()
        {
            InitializeComponent();
            CultureResources.registerDataProvider(this);
        }

        public LinacGunDriver(Dispatcher dispatcher, WidgetStatusHost widgetStatusHost)
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
            if (name.Equals(_GD_CATHODE_VOLTAGE))
            {
                SetTextFromString(GD_Cathode_Voltage_Text, val);
            }
            else if (name.Equals(_GD_HEATER_VOLTAGE))
            {
                SetTextFromString(GD_Heater_Voltage_Text, val);
            }
            else if (name.Equals(_GD_HEATER_CURRENT))
            {
                SetTextFromString(GD_Heater_Current_Text, val);
            }
            else if (name.Equals(_GD_GRID_DRIVE_A_VOLTAGE))
            {
                SetTextFromString(GD_Grid_A_Voltage_Text, val);
            }
            else if (name.Equals(_GD_GRID_DRIVE_B_VOLTAGE))
            {
                SetTextFromString(GD_Grid_B_Voltage_Text, val);
            }
            else if (name.Equals(_GD_GRID_BIAS))
            {
                SetTextFromString(GD_Grid_Bias_Text, val);
            }
            else if (name.Equals(_GD_BEAM_CURRENT))
            {
                SetTextFromString(GD_Beam_Current_Text, val);
            }
            else if (name.Equals(_GD_STATE))
            {
                SetTextFromResource(GD_State_Text, OpcTags.LINAC_ENERGY_TYPE_STATE.ResourceName + "_" + value.ToString());
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
