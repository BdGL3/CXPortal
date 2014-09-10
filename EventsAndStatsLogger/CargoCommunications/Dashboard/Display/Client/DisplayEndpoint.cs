using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using L3.Cargo.Communications.Dashboard.Display.Interfaces;
using System.ServiceModel.Channels;

namespace L3.Cargo.Communications.Dashboard.Display.Client
{
    public class DisplayEndpoint : ClientBase<IDisplay>, IDisplay, IDisposable
    {
        #region Constructors

        public DisplayEndpoint ()
        {
        }

        public DisplayEndpoint (string endpointConfigurationName) :
            base(endpointConfigurationName)
        {
        }

        public DisplayEndpoint (string endpointConfigurationName, string remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        public DisplayEndpoint (string endpointConfigurationName, EndpointAddress remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        public DisplayEndpoint (Binding binding, EndpointAddress remoteAddress) :
            base(binding, remoteAddress)
        {
        }

        #endregion Constructors


        #region Methods

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
