using System;
using System.IO;
using System.ServiceModel;
using System.Windows.Threading;
using System.Xml.Serialization;
using L3.Cargo.Common.Dashboard.Display;
using L3.Cargo.Common.Dashboard.Display.Interfaces;
using L3.Cargo.Communications.Dashboard.Display.Client;
using L3.Cargo.Communications.Dashboard.Display.Host;
using L3.Cargo.Communications.Dashboard.Display.Interfaces;
using L3.Cargo.Linac.Display.Common;
using L3.Cargo.Communications.Common;

namespace L3.Cargo.Linac.Supervisor
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

            Widget energySelection = new Widget("LinacEnergySelection");
            dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                energySelection.Display = new EnergySelection(dispatcher, _SubsystemAddress, _WidgetStatusHost);
            }));
            _AssemblyDisplays.Widgets.Add(energySelection);

            return base.Initialize(passedObj);
        }

        #endregion Public Methods
    }
}
