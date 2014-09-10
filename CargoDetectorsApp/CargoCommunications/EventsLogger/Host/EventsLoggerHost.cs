using System;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel;
using System.Windows;

using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.EventsLogger.Common;
using L3.Cargo.Communications.EventsLogger.Interfaces;

namespace L3.Cargo.Communications.EventsLogger.Host
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class EventsLoggerHost : IEventsLogger
    {
        public EventsLoggerHost(Uri address)
        {
            List<DiscoveryMetadata> metaData = new List<DiscoveryMetadata>();
            metaData.Add(new DiscoveryMetadata(CommuncationInfo.MetaDataIPAddresses, "*"));
            _host = new NetworkHost<IEventsLogger>(this, address, metaData);
        }
        public EventsLoggerHost(Uri address, string msmqAddress, string nameSpace)
        {
            List<DiscoveryMetadata> metaData = new List<DiscoveryMetadata>();
            metaData.Add(new DiscoveryMetadata(CommuncationInfo.MetaDataIPAddresses, "*"));
            _host = new NetworkHost<IEventsLogger>(this, address, msmqAddress, metaData, nameSpace);
        }

        public virtual DataSet GetReport(ReportFilter filter) { return null; }

        [OperationBehavior(TransactionScopeRequired = true, TransactionAutoComplete = true)]
        public virtual void LogEvent(Event info) { }

        public virtual void Start() { _host.Open(); }

        public virtual void Stop() { _host.Close(); }

        protected NetworkHost<IEventsLogger> _host;
    }
}
