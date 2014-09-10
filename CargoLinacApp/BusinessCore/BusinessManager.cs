using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Linac.DataAccessCore;
using L3.Cargo.Subsystem.RequestManagerCore;
using L3.Cargo.Subsystem.StatusManagerCore;
using L3.Cargo.Detectors.StatusManagerCore;

namespace L3.Cargo.Linac.BusinessCore
{
    public class BusinessManager
    {
        private StatusManager _StatusManager;

        private RequestManager _RequestManager;

        public BusinessManager (LinacDataAccess dataAccess, EventLoggerAccess logger)
        {
            _StatusManager = new LinacStatusManager(dataAccess, logger);
            _RequestManager = new RequestManager(dataAccess, _StatusManager, logger);
        }
    }
}
