using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using L3.Cargo.Communications.Dashboard.Display.Interfaces;
using System.ServiceModel.Channels;

namespace L3.Cargo.Communications.Dashboard.Display.Client
{
    public class WidgetRequestEndpoint : ClientBase<IWidgetRequest>, IWidgetRequest, IDisposable
    {
        #region Constructors

        public WidgetRequestEndpoint ()
        {
        }

        public WidgetRequestEndpoint (string endpointConfigurationName) :
            base(endpointConfigurationName)
        {
        }

        public WidgetRequestEndpoint (string endpointConfigurationName, string remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        public WidgetRequestEndpoint (string endpointConfigurationName, EndpointAddress remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        public WidgetRequestEndpoint (Binding binding, EndpointAddress remoteAddress) :
            base(binding, remoteAddress)
        {
        }

        #endregion Constructors


        #region Methods

        public void Request (string name, int value)
        {
            base.Channel.Request(name, value);
        }

        public void SendUpdate ()
        {
            base.Channel.SendUpdate();
        }

        public void Dispose ()
        {
            try
            {
                if (this.State == CommunicationState.Opened)
                {
                    this.Close();
                }
            }
            catch { }
        }

        #endregion Methods
    }
}
