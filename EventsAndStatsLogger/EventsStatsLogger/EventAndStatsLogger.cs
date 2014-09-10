using System;
using System.Data;
using System.ServiceModel;
using L3.Cargo.Communications.EventsLogger.Common;
using L3.Cargo.Communications.EventsLogger.Host;

namespace EventAndStatsLogger
{
    public class EventsLogger : EventsLoggerHost
    {
        #region Private Members

        LogDatabase _LogDatabase;

        MainWindow _Logger;

        #endregion Private Members


        #region Constructors

        public EventsLogger(Uri baseAddress, MainWindow logger):
            base(baseAddress)
        {
            _Logger = logger;
            _LogDatabase = new LogDatabase();
        }

        public EventsLogger(Uri baseAddress, MainWindow logger, string MSMQAddress, string MSMQNamespace):
            base(baseAddress, MSMQAddress, MSMQNamespace)
        {
            _Logger = logger;
            _LogDatabase = new LogDatabase();
        }

        #endregion Constructors


        #region Public methods

        [OperationBehavior(TransactionScopeRequired = true, TransactionAutoComplete = true)]
        public override void LogEvent(Event info)
        {
            try
            {
                _LogDatabase.InsertEvent(info);
                _Logger.LogMessage(info);
            }
            catch (Exception exp)
            {
                _Logger.LogMessage(exp.Message);
            }
        }

        public override DataSet GetReport(ReportFilter filter)
        {
            DataSet ret = null;

            try
            {
                ret = _LogDatabase.GetReport(filter);
            }
            catch (Exception exp)
            {
                _Logger.LogMessage(exp.Message);
            }

            return ret;
        }

        #endregion Public methods
    }
}
