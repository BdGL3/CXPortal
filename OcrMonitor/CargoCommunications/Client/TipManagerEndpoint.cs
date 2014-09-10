using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using L3.Cargo.Communications.Interfaces;

namespace L3.Cargo.Communications.Client
{
    public class TipManagerEndpoint : DuplexClientBase<ITipManager>, ITipManager
    {
        #region Constructors

        public TipManagerEndpoint(InstanceContext callbackInstance) :
            base(callbackInstance)
        {
        }

        public TipManagerEndpoint(Object callbackInstance) :
            base(callbackInstance)
        {
        }

        public TipManagerEndpoint(InstanceContext callbackInstance, ServiceEndpoint endpoint) :
            base(callbackInstance, endpoint)
        {
        }

        public TipManagerEndpoint(InstanceContext callbackInstance, String endpointConfigurationName) :
            base(callbackInstance, endpointConfigurationName)
        {
        }

        public TipManagerEndpoint(Object callbackInstance, ServiceEndpoint endpoint) :
            base(callbackInstance, endpoint)
        {
        }

        public TipManagerEndpoint(Object callbackInstance, String endpointConfigurationName) :
            base(callbackInstance, endpointConfigurationName)
        {
        }

        public TipManagerEndpoint(InstanceContext callbackInstance, Binding binding, EndpointAddress remoteAddress) :
            base(callbackInstance, binding, remoteAddress)
        {
        }

        public TipManagerEndpoint(InstanceContext callbackInstance, String endpointConfigurationName, EndpointAddress remoteAddress) :
            base(callbackInstance, endpointConfigurationName, remoteAddress)
        {
        }

        public TipManagerEndpoint(InstanceContext callbackInstance, String endpointConfigurationName, String remoteAddress) :
            base(callbackInstance, endpointConfigurationName, remoteAddress)
        {
        }

        public TipManagerEndpoint(Object callbackInstance, Binding binding, EndpointAddress remoteAddress) :
            base(callbackInstance, binding, remoteAddress)
        {
        }

        public TipManagerEndpoint(Object callbackInstance, String endpointConfigurationName, EndpointAddress remoteAddress) :
            base(callbackInstance, endpointConfigurationName, remoteAddress)
        {
        }

        public TipManagerEndpoint(Object callbackInstance, String endpointConfigurationName, String remoteAddress) :
            base(callbackInstance, endpointConfigurationName, remoteAddress)
        {
        }

        #endregion Constructors

        #region Methods

        public void ProcessedCase(string alias, string caseId)
        {
            base.Channel.ProcessedCase(alias, caseId);
        }

        public void TipResult (string tipFile, WorkstationResult workstationResult)
        {
            base.Channel.TipResult(tipFile, workstationResult);
        }

        #endregion Methods
    }
}
