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
       

        private Dispatcher _Dispatcher;

        private WidgetStatusHost _WidgetStatusHost;

        private static string STATUS_OBJECTS_TAG_NAME = "StatusObjectsGroup";

        private Dictionary<string, PortalObject> _portalObjects;

        private PortalViewModel _viewModel;

        public VehicleType VehicleStatus
        {
            get;
            set;
        }
        

        #region Constructors

        public PortalModel(WidgetStatusHost widgetStatusHost, Dispatcher dispatcher)
        {
            InitializeComponent();

            CultureResources.registerDataProvider(this);

            _viewModel = new PortalViewModel();

            createStatusObjects();

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

        private void createStatusObjects()
        {
            ModelVisual3D visualObjectsGroup = FindName(STATUS_OBJECTS_TAG_NAME) as ModelVisual3D;

            if (visualObjectsGroup != null)
            {
                Model3DGroup groupScene = new Model3DGroup();

                PortalModelFactory modelFactory = new PortalModelFactory(ref groupScene);

                _portalObjects = modelFactory.getPortalModelObjects();
                visualObjectsGroup.Content = groupScene;
            }
        }

        private void WidgetUpdate(string name, int value)
        {
            _Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {

                try
                {
                    System.Console.WriteLine("########################" + name);
                    if(name.Contains("VEHICLE_TYPE"))
                    {
                        _viewModel.setVehicleStatus(value);
                    }
                    else {
                        _portalObjects[name].applyBehavior(this, name, value);
                    }
                }
                catch (Exception e)
                {
                    System.Console.WriteLine("Exception thrown at name: " + name + e);
                }

            }));

        }
        #endregion Private Members

    }
}
