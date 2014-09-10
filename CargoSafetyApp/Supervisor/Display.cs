using System;
using System.ServiceModel;
using System.Windows.Threading;
using L3.Cargo.Common.Dashboard.Display;
using L3.Cargo.Safety.Display.Common;

namespace L3.Cargo.Safety.Supervisor
{
    public class Displays : DisplayBase
    {
        #region Constructors

        public Displays () :
            base()
        {
        }

        #endregion Constructors


        #region Public Methods

        public override AssemblyDisplays Initialize (Object passedObj)
        {
            object[] parameters = (object[])passedObj;
            Dispatcher dispatcher = (Dispatcher)parameters[0];
            string baseDirectory = (string)parameters[1];

            ReadSettings(baseDirectory);
            OpenStatusUpdateServer();
            InitializeSubsystemAddress();

            Widget PerimeterMode = new Widget("PerimeterMode");
           // Widget CollisionDetection = new Widget("CollisionDetection");
            Widget BoomSiren = new Widget("BoomSiren");

            dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                PerimeterMode.Display = new PerimeterMode(dispatcher, _SubsystemAddress, _WidgetStatusHost);
              //  CollisionDetection.Display = new CollisionDetection(dispatcher, _SubsystemAddress, _WidgetStatusHost);
                BoomSiren.Display = new BoomSiren(dispatcher, _SubsystemAddress, _WidgetStatusHost);
            }));

            _AssemblyDisplays.Widgets.Add(PerimeterMode);
           // _AssemblyDisplays.Widgets.Add(CollisionDetection);
            _AssemblyDisplays.Widgets.Add(BoomSiren);

            return base.Initialize(passedObj);
        }

        #endregion Public Methods
    }
}
