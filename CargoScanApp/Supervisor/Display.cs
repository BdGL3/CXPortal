using System;
using System.ServiceModel;
using System.Windows.Threading;
using L3.Cargo.Common.Dashboard.Display;
using L3.Cargo.Scan.Display.Common;

namespace L3.Cargo.Scan.Supervisor
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

            Widget scanClickerControls = new Widget("ScanClickerControls");

            dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                scanClickerControls.Display = new ScanClickerControls(dispatcher, _SubsystemAddress, _WidgetStatusHost);
            }));

            _AssemblyDisplays.Widgets.Add(scanClickerControls);

            return base.Initialize(passedObj);
        }

        #endregion Public Methods
    }
}
