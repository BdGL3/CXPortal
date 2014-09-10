using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using L3.Cargo.Communications.EventsLogger.Common;
using L3.Cargo.Communications.EventsLogger.Interfaces;

namespace L3.Cargo.Communications.EventsLogger.Client
{
    internal class EventsLoggerEndpoint : ClientBase<IEventsLogger>, IEventsLogger
    {
        #region Constructors

        public EventsLoggerEndpoint()
        {
        }
        
        public EventsLoggerEndpoint(string endpointConfigurationName) : 
                base(endpointConfigurationName)
        {
        }
        
        public EventsLoggerEndpoint(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress)
        {
        }
        
        public EventsLoggerEndpoint(string endpointConfigurationName, EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress)
        {
        }

        public EventsLoggerEndpoint(Binding binding, EndpointAddress remoteAddress) : 
                base(binding, remoteAddress)
        {
        }

        #endregion Constructors


        #region Public Methods

        public void LogEvent(Event e)
        {
            try
            {
                base.Channel.LogEvent(e);
            }
            catch { }
        }

        #endregion Public Methods
    }
}
