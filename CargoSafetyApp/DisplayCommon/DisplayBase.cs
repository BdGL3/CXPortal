using System;
using System.IO;
using System.ServiceModel;
using System.Windows.Threading;
using System.Xml.Serialization;
using L3.Cargo.Common.Dashboard.Display;
using L3.Cargo.Common.Dashboard.Display.Interfaces;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.Dashboard.Display.Client;
using L3.Cargo.Communications.Dashboard.Display.Host;
using L3.Cargo.Communications.Dashboard.Display.Interfaces;
using L3.Cargo.Safety.Display.Common.ViewModel;

using System.Configuration;

namespace L3.Cargo.Safety.Display.Common
{
    public class DisplayBase : IDisplays
    {
        #region Private Members

        private string _Name;

        private string _Version;

        #endregion Private Members


        #region Protected Members

        protected WidgetStatusHost _WidgetStatusHost;

        protected ServiceHost _ServiceHost;

        protected AssemblyDisplays _AssemblyDisplays;

        protected KeyValueConfigurationCollection _Settings;

        protected EndpointAddress _SubsystemAddress;

        #endregion Protected Members


        #region Public Members

        public string Name
        {
            get
            {
                return _Name;
            }
        }

        public string Version
        {
            get
            {
                return _Version;
            }
        }

        #endregion Public Members


        #region Constructors

        public DisplayBase ()
        {
            _Name = "CX-Mobile G3 Safety";
            _Version = "1.0.0";
            _WidgetStatusHost = new WidgetStatusHost();
            _AssemblyDisplays = new AssemblyDisplays(_Name);
        }

        #endregion Constructors


        #region Private Methods

        private void RequestInitialValues ()
        {
            // Request for Display update.
            WidgetRequestEndpoint widgetRequestEndpoint = new WidgetRequestEndpoint(new TCPBinding(), _SubsystemAddress);
            widgetRequestEndpoint.Open();
            widgetRequestEndpoint.SendUpdate();
            widgetRequestEndpoint.Close();
        }

        #endregion Private Methods


        #region Protected Methods

        protected void ReadSettings (string baseDirectory)
        {
            if (_Settings == null)
            {
                var map = new ExeConfigurationFileMap();
                map.ExeConfigFilename = Path.Combine(baseDirectory, "Settings.config");
                Configuration config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
                AppSettingsSection appSettingsSection = config.AppSettings;
                _Settings = appSettingsSection.Settings;
            }
        }

        protected void OpenStatusUpdateServer ()
        {
            if (_ServiceHost == null)
            {
                // Setup StatusUpdate Server
                string uri = "net.tcp://" + System.Environment.MachineName + ":" + _Settings["DisplayPort"].Value + "/SafetyComm";
                _ServiceHost = new ServiceHost(_WidgetStatusHost, new Uri[] { new Uri(uri) });
                _ServiceHost.AddServiceEndpoint(typeof(IWidgetStatus), new TCPBinding(), uri);
                _ServiceHost.Open();
            }
        }

        protected void InitializeSubsystemAddress ()
        {
            if (_SubsystemAddress == null)
            {
                // Request for Display update.
                string uri = "net.tcp://" + _Settings["SubsystemServer"].Value + ":" + _Settings["SubsystemPort"].Value + "/SafetyComm";
                _SubsystemAddress = new EndpointAddress(uri);
            }
        }

        #endregion Protected Methods


        #region Public Methods

        public virtual AssemblyDisplays Initialize (Object passedObj)
        {
            object[] parameters = (object[])passedObj;
            Dispatcher dispatcher = (Dispatcher)parameters[0];
            string baseDirectory = (string)parameters[1];

            ReadSettings(baseDirectory);
            OpenStatusUpdateServer();
            InitializeSubsystemAddress();

            // MDM Using the Portal Now
            // Widget SafetyTruck = new Widget("SafetyTruckDisplay");
            Widget SafetyPortal = new Widget("SafetyPortalDisplay");
            Widget Reset = new Widget("SafetyReset");
            Status status = new Status();
            CompleteInfo info = null;

            dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                // MDM Using now the Portal
                // SafetyTruck.Display = new TruckModel(_WidgetStatusHost, dispatcher);
                
                SafetyPortal.Display = new PortalModel(_WidgetStatusHost, dispatcher);
                Reset.Display = new ResetFaults(dispatcher, _SubsystemAddress);

                status.ErrorMessages = new ErrorMessages(_WidgetStatusHost, dispatcher);
                status.Indicator = new Indicator(_WidgetStatusHost, dispatcher);
                status.WarningMessages = new WarningMessages(_WidgetStatusHost, dispatcher);

                info = new CompleteInfo("SafetyCompleteInfo", new InfoButton(), new InfoDisplay(dispatcher, _WidgetStatusHost));
            }));
            _AssemblyDisplays.Widgets.Add(SafetyPortal);
            _AssemblyDisplays.Widgets.Add(Reset);
            _AssemblyDisplays.Statuses.Add(status);
            _AssemblyDisplays.CompleteInfos.Add(info);

            RequestInitialValues();

            return _AssemblyDisplays;
        }

        public virtual void Dispose ()
        {
            _ServiceHost.Close();
        }

        #endregion Public Methods
    }
}
