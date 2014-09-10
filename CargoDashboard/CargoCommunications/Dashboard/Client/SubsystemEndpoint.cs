using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using L3.Cargo.Communications.Dashboard.Interfaces;

namespace L3.Cargo.Communications.Dashboard.Client
{
    public class SubsystemEndpoint : ClientBase<ISubsystem>, ISubsystem, IDisposable
    {
        #region Constructors

        public SubsystemEndpoint() :
            base()
        {
        }

        public SubsystemEndpoint(ServiceEndpoint endpoint) :
            base(endpoint)
        {
        }

        public SubsystemEndpoint(string endpointConfigurationName) :
            base(endpointConfigurationName)
        {
        }

        public SubsystemEndpoint(string endpointConfigurationName, string remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        public SubsystemEndpoint(string endpointConfigurationName, EndpointAddress remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        public SubsystemEndpoint(Binding binding, EndpointAddress remoteAddress) :
            base(binding, remoteAddress)
        {
        }

        #endregion Constructors


        #region Methods

        public SubsystemAssembly GetAssembly(GetAssemblyParameterMessage message)
        {
            return base.Channel.GetAssembly(message);
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