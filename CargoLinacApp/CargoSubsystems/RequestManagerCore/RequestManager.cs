using System;
using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Subsystem.DataAccessCore;
using L3.Cargo.Subsystem.StatusManagerCore;

namespace L3.Cargo.Subsystem.RequestManagerCore
{
    public class RequestManager
    {
        #region Protected Members

        protected DataAccess _DataAccess;

        protected StatusManager _StatusManager;

        protected EventLoggerAccess _Logger;

        #endregion Protected Members


        #region Constructors

        public RequestManager (DataAccess dataAccess, StatusManager statusManager, EventLoggerAccess logger)
        {
            _Logger = logger;
            _DataAccess = dataAccess;
            _DataAccess.DisplayControlUpdateRequest += new DashboardControlUpdateHandler(ProcessControlUpdateRequest);
            _StatusManager = statusManager;
        }

        #endregion Constructors


        #region Protected Methods

        protected virtual void ProcessControlUpdateRequest (string name, int value)
        {
            try
            {
                _DataAccess.UpdatePLCTagValue(name, value);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        #endregion Protected Methods


        #region Public Methods


        #endregion Public Methods
    }
}
