using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Data;
using L3.Cargo.Communications.EventsLogger.Interfaces;
using L3.Cargo.Communications.EventsLogger.Common;
using L3.Cargo.Communications.Common;
using System.Windows;

namespace L3.Cargo.Communications.EventsLogger.Host
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class EventsLoggerHost : IEventsLogger
    {
        #region Protected Members

        protected NetworkHost<IEventsLogger> host;

        #endregion


        #region Constructors

        public EventsLoggerHost(Uri baseAddress)
        {
            List<DiscoveryMetadata> list = new List<DiscoveryMetadata>();
            list.Add(new DiscoveryMetadata(CommuncationInfo.MetaDataIPAddresses, "*"));
            host = new NetworkHost<IEventsLogger>(this, baseAddress, list);
        }

        public EventsLoggerHost (Uri baseAddress, string MSMQAddress, string MSMQNamespace)
        {
            List<DiscoveryMetadata> list = new List<DiscoveryMetadata>();
            list.Add(new DiscoveryMetadata(CommuncationInfo.MetaDataIPAddresses, "*"));
            host = new NetworkHost<IEventsLogger>(this, baseAddress, MSMQAddress, list, MSMQNamespace);
        }

        #endregion


        #region Public methods

        [OperationBehavior(TransactionScopeRequired = true, TransactionAutoComplete = true)]
        public virtual void LogEvent(Event info)
        {
        }

        public virtual DataSet GetReport(ReportFilter filter)
        {
            return null;
        }

        public virtual void Start()
        {
            host.Open();
        }

        public virtual void Stop()
        {
            host.Close();
        }

        #endregion
    }
}
