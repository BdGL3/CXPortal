using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Subsystem.DataAccessCore;
using L3.Cargo.Subsystem.RequestManagerCore;
using L3.Cargo.Subsystem.StatusManagerCore;

namespace L3.Cargo.Safety.BusinessCore
{
    public class BusinessManager
    {
        private StatusManager _StatusManager;

        private RequestManager _RequestManager;

        public BusinessManager (DataAccess dataAccess, EventLoggerAccess logger)
        {
            _StatusManager = new StatusManager(dataAccess, logger);
            _RequestManager = new RequestManager(dataAccess, _StatusManager, logger);
        }
    }
}
