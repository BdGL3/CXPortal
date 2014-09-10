using System.ServiceModel;

using L3.Cargo.Communications.EventsLogger.Common;

namespace L3.Cargo.Communications.EventsLogger.Interfaces
{
    [ServiceContract(Namespace = "EventAndStatsLoggerV1.0")]
    public interface IEventsLogger
    {
        [OperationContract(IsOneWay = true, Action = "*")]
        void LogEvent(Event info);
    }
}