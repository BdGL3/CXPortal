using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Detectors.DataAccessCore;
using L3.Cargo.Detectors.StatusManagerCore;
using L3.Cargo.Subsystem.DataAccessCore;
using L3.Cargo.Subsystem.RequestManagerCore;

namespace L3.Cargo.Detectors.RequestManagerCore
{
    public delegate void RequestUpdateHandler(string name, int value);

    public class DetectorRequestManager : RequestManager
    {
        #region Private Members

        private DetectorsDataAccess _dataAccess;

        private EventLoggerAccess _logger;

        #endregion Private Members


        #region Public Members

        public event RequestUpdateHandler RequestUpdateEvent;

        #endregion


        #region Constructors

        public DetectorRequestManager(DetectorsDataAccess dataAccess, DetectorsStatusManager statusManager, EventLoggerAccess logger)
            : base (dataAccess, statusManager, logger)
        {
            _logger = logger;
            _dataAccess = dataAccess;
            _dataAccess.DisplayControlUpdateRequest += new DashboardControlUpdateHandler(ProcessControlUpdateRequest);
        }

        #endregion Constructors


        #region Protected Methods

        protected override void ProcessControlUpdateRequest (string name, int value)
        {
            switch (name)
            {
                case "SEND_REALTIME_VIEW_DATA":
                    if (RequestUpdateEvent != null)
                    {
                        RequestUpdateEvent(name, value);
                    }
                    break;            
            }
        }

        #endregion Protected Methods


        #region Public Methods


        #endregion Public Methods
    }
}
