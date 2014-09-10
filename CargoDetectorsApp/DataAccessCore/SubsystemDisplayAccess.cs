using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using L3.Cargo.Communications.Dashboard.Display.Host;
using System.ServiceModel;
using L3.Cargo.Communications.Dashboard.Display.Interfaces;

namespace L3.Cargo.Detectors.DataAccessCore
{
    public delegate void DashboardUpdateRequestHandler ();

    public delegate void DashboardControlUpdateHandler (string name, int value);

    internal class SubsystemDisplayAccess : IDisposable
    {
        private WidgetRequestHost _WidgetRequestHost;

        private ServiceHost _ServiceHost;

        public event DashboardUpdateRequestHandler DisplayUpdateRequest;

        public event DashboardControlUpdateHandler ControlUpdateRequest;

        public SubsystemDisplayAccess ()
        {
            string port = ConfigurationManager.AppSettings["ServerPort"];
            string uri = "net.tcp://" + System.Environment.MachineName + ":" + port + "/ScanComm";

            _WidgetRequestHost = new WidgetRequestHost();
            _WidgetRequestHost.SendUpdateEvent += new SendUpdateHandler(SendUpdate);
            _WidgetRequestHost.ControlUpdateEvent += new ControlUpdateHandler(ControlUpdate); 

            _ServiceHost = new ServiceHost(_WidgetRequestHost, new Uri[] { new Uri(uri) });
            _ServiceHost.AddServiceEndpoint(typeof(IWidgetRequest), new NetTcpBinding(), uri);
            _ServiceHost.Open();
        }

        private void ControlUpdate (string name, int value)
        {
            if (ControlUpdateRequest != null)
            {
                ControlUpdateRequest(name, value);
            }
        }

        private void SendUpdate ()
        {
            if (DisplayUpdateRequest != null)
            {
                DisplayUpdateRequest();
            }
        }

        public void Dispose ()
        {
            _WidgetRequestHost.SendUpdateEvent -= new SendUpdateHandler(SendUpdate);
            _WidgetRequestHost.ControlUpdateEvent -= new ControlUpdateHandler(ControlUpdate); 
            _ServiceHost.Close();
        }
    }
}
