using System;
using L3.Cargo.Communications.EventsLogger.Client;

namespace L3.Cargo.Dashboard.DataAccessCore
{
    public delegate void SubsystemServiceListUpdateHandler (string alias, SubsystemUpdateEnum? update, string filenameWithPath);

    public class DataAccess: IDisposable
    {
        #region Private Members

        SubsystemServices _SubsystemServices;

        EventLoggerAccess _Logger;

        #endregion Private Members


        #region Constructors

        public DataAccess(EventLoggerAccess logger)
        {
            _Logger = logger;
            _SubsystemServices = new SubsystemServices(logger);
        }

        #endregion Constructors


        #region Public Methods

        public void setEvent(SubsystemServiceListUpdateHandler handler)
        {
            _SubsystemServices.SubsystemServiceListUpdateEvent += handler; 
        }

        public void StartUp()
        {
            try
            {
                _SubsystemServices.StartUp();
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        public void Shutdown()
        {
            try
            {
                _SubsystemServices.ShutDown();
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex);
            }
        }

        public void Dispose()
        {
            if (_SubsystemServices != null)
            {
                _SubsystemServices.ShutDown();
            }
        }

        #endregion Public Methods
    }
}
