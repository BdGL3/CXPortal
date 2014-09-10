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
using System.Windows.Media.Media3D;

using L3.Cargo.Safety.Display.Common.ObjectDrawing;
using L3.Cargo.Safety.Display.Common.PortalObjects;
using L3.Cargo.Safety.Display.Common.ViewModel;
using System.Configuration;
using L3.Cargo.Common.Configurations;

namespace L3.Cargo.Safety.Display.Common
{
    public enum VehicleType
    {
        No_Detection = 0,
        Small_Vehicle = 1,
        Medium_Vehicle = 2,
        Large_Vehicle = 3
    }

    /// <summary>
    /// Interaction logic for PortalModel.xaml
    /// </summary>
    public partial class PortalModel : UserControl
    {
        private static string TRAFFIC_RED_LIGHT_XML_TAG = "M_Traffic_Light_Red";
        private static string TRAFFIC_GREEN_LIGHT_XML_TAG = "M_Traffic_Light_Green";

        private Dispatcher _Dispatcher;

        private WidgetStatusHost _WidgetStatusHost;

        private static string STATUS_OBJECTS_TAG_NAME = "StatusObjectsGroup";

        private Dictionary<string, PortalObject> _portalObjects;

        private PortalViewModel _viewModel;

        private Dictionary<String, Delegate> dictionary = 
                new Dictionary<String, Delegate>();

        public VehicleType VehicleStatus
        {
            get;
            set;
        }

        #region Constructors

        public PortalModel(WidgetStatusHost widgetStatusHost, Dispatcher dispatcher)
        {
            InitializeComponent();
            
            Map(updateInterlockDoor, "INTERLOCK_DOOR");
            Map(updateEStops, "ESTOP");
            Map(updateMDSSensor, "MDS");
            Map(updateLightCurtainEmitters, "INTERLOCK_LC");
            Map(updateTrafficLight, "TRAFFIC_LIGHT_STATUS");
            Map(updateVehicleSensor, "VEHICLE_SENSOR");
            
            CultureResources.registerDataProvider(this);

            _viewModel = new PortalViewModel();

            this.VehicleStatus = VehicleType.Small_Vehicle;

            _Dispatcher = dispatcher;
            _WidgetStatusHost = widgetStatusHost;
            _WidgetStatusHost.WidgetUpdateEvent += new WidgetUpdateHandler(WidgetUpdate);
            _WidgetStatusHost.ErrorMessageUpdate += new UpdateErrorMessageHandler(_WidgetStatusHost_ErrorMessageUpdate);
        }

        #endregion Constructors

        #region Private Members
        private void _WidgetStatusHost_ErrorMessageUpdate(string[] messages)
        {
            if (messages.Length > 0)
            {
                ErrorFadeBorder.Fade(true);
            }
            else
            {
                ErrorFadeBorder.Fade(false);
            }
        }

        private void WidgetUpdate(string name, int value)
        {
            _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {

                try
                {
                    applyBehavior(name, value);
                }
                catch (Exception e)
                {
                    System.Console.WriteLine("Exception thrown at name: " + name + e);
                }

            }));

        }

        private void applyBehavior(string name, int value)
        {
            _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                GeometryModel3D model = null;

                Boolean endOfSignal = false;

                int iter = 1;
                while (!endOfSignal)
                {
                    try
                    {
                        String nameToSearch = name + "_" + iter++;

                        model = FindName(nameToSearch) as GeometryModel3D;

                        if (model != null)
                        {
                            Call(name, model, value);
                        }
                        else
                        {
                            endOfSignal = true;
                        }
                    }
                    catch (Exception e)
                    {
                        endOfSignal = true;
                    }
                }

            }));
        }

        private void Map(Action<GeometryModel3D, int> mapper, String key)
        {
            dictionary[key] = mapper;
        }

        private void Call(String key, GeometryModel3D model, int value)
        {
            foreach (KeyValuePair<string, Delegate> entry in dictionary)
            {
                String keyName = entry.Key;

                if (key.Contains(key))
                {

                    var func = dictionary[keyName] as Action<GeometryModel3D, int>;
                    func.Invoke(model, value);
                }
            }

        }

        private void updateEStops(GeometryModel3D model, int value)
        {
            if (value != 1 && value != 2)
            {
                model.Material = (Material)FindResource("M_Interlock");
            }
            else if (value == 1)
            {
                model.Material = (MaterialGroup)FindResource("M_Error");
            }
            else if (value == 2)
            {
                model.Material = (MaterialGroup)FindResource("M_Warning");
            }
  
        }

        private void updateInterlockDoor(GeometryModel3D model, int value)
        {
            if (value != 1 && value != 2)
            {
                model.Material = (Material)FindResource("M_Interlock");
            }
            else if (value == 1)
            {
                model.Material = (MaterialGroup)FindResource("M_Error");
            }
            else if (value == 2)
            {
                model.Material = (MaterialGroup)FindResource("M_Warning");
            }
        }

        private void updateMDSSensor(GeometryModel3D model, int value)
        {
            if (value == 1)
            {
                model.Material = (MaterialGroup)FindResource("M_Error");
            }
            else if (value == 2)
            {
                model.Material = (MaterialGroup)FindResource("M_Warning");
            }
            else
            {
                model.Material = (Material)FindResource("M_Interlock");
            }
        }

        private void updateLightCurtainEmitters(GeometryModel3D model, int value)
        {
            if (value == 1)
            {
                model.Material = (MaterialGroup)FindResource("M_Error");
            }
            else if (value == 2)
            {
                model.Material = (MaterialGroup)FindResource("M_Warning");
            }
            else
            {
                model.Material = (Material)FindResource("M_Interlock");
            }
        }

        private void updateTrafficLight(GeometryModel3D model, int value)
        {
            if (value == 1)
            {
                model.Material = (MaterialGroup)FindResource(TRAFFIC_GREEN_LIGHT_XML_TAG);
            }
            else if (value == 2)
            {
                model.Material = (MaterialGroup)FindResource(TRAFFIC_RED_LIGHT_XML_TAG);
            }
        }

        private void updateVehicleSensor(GeometryModel3D model, int value)
        {
            if (value == 1)
            {
                model.Material = (MaterialGroup)FindResource("M_Vehicle_Sensor_Warning");
            }
            else if (value == 2)
            {
                model.Material = (MaterialGroup)FindResource("M_Vehicle_Sensor_Bad");
            }
            else
            {
                model.Material = (Material)FindResource("M_Vehicle_Sensor_Good");
            }
        }

        #endregion Private Members

    }
}
