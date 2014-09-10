using System;
using System.Configuration;
using System.IO;
using System.ServiceModel;
using System.Windows.Threading;
using L3.Cargo.Common.Dashboard.Display;
using L3.Cargo.Common.Dashboard.Display.Interfaces;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.Dashboard.Display.Client;
using L3.Cargo.Communications.Dashboard.Display.Host;
using L3.Cargo.Communications.Dashboard.Display.Interfaces;

namespace L3.Cargo.Detectors.Display.Common
{
    public class DisplayBase : IDisplays
    {
        #region Private Members

        private string _Name;

        private string _Version;

        private RealTimeViewerDisplay _RealTimeViewerDisplay;

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
            _Name = "CX-Mobile G3 Detectors";
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
                string uri = "net.tcp://" + System.Environment.MachineName + ":" + _Settings["DisplayPort"].Value + "/DetectorsComm";
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
                string uri = "net.tcp://" + _Settings["SubsystemServer"].Value + ":" + _Settings["SubsystemPort"].Value + "/DetectorsComm";
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

            Widget realTimeViewer = new Widget("DetectorRealTimeViewer");
            Widget startCalibrationControls = new Widget("StartCalibrationControls");

            Status status = new Status();

            dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
            {
                _RealTimeViewerDisplay = new RealTimeViewerDisplay(_Settings["RealTimeViewerMulticastIPAddress"].Value, int.Parse(_Settings["RealTimeViewerDataPort"].Value));
                startCalibrationControls.Display = new StartCalibrationControls(dispatcher, _SubsystemAddress, _WidgetStatusHost);
                realTimeViewer.Display = _RealTimeViewerDisplay;

                status.ErrorMessages = new ErrorMessages(_WidgetStatusHost, dispatcher);
                status.Indicator = new Indicator(_WidgetStatusHost, dispatcher);
                status.WarningMessages = new WarningMessages(_WidgetStatusHost, dispatcher);
            }));
            _AssemblyDisplays.Widgets.Add(realTimeViewer);
            _AssemblyDisplays.Widgets.Add(startCalibrationControls);
            _AssemblyDisplays.Statuses.Add(status);

            RequestInitialValues();

            return _AssemblyDisplays;
        }

        public virtual void Dispose ()
        {
            _RealTimeViewerDisplay.Dispose();
            _ServiceHost.Close();
        }

        #endregion Public Methods
    }
}
