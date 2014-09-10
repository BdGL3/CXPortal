using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Subsystem.DataAccessCore;
using L3.Cargo.Subsystem.StatusManagerCore;

namespace L3.Cargo.Subsystem.RequestManagerCore
{
    public class RequestManager : IDisposable
    {
        /// <summary>Class Name specifies the name of this class.</summary>
        public static string ClassName { get { return MethodBase.GetCurrentMethod().DeclaringType.Name; } }

        /// <summary>
        /// Data Access Reference holds a reference to a <see cref="DataAccess"/>
        /// instance.</summary>
        [DefaultValue(null)]
        public DataAccess DataAccessReference { get; private set; }

        /// <summary>
        /// Dispose resources and suppress finalization. USE THIS METHOD rather than just setting a
        /// reference to null and letting the system garbage collect! It ensures that the
        /// connection(s) are tidied, informing remote client(s)/host(s) of the
        /// stand-down.</summary>
        /// <exception cref="Exception">
        /// The method is written so that it never throws an exception.</exception>
        public void Dispose()
        {
            Dispose(!Disposed);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool isDisposing)
        {
            if (/*dispose?*/ isDisposing)
            {
                try
                {   // Disposes SHOULD never throw exceptions ... but...
                    if (StatusManagerReference != null)
                        StatusManagerReference.Dispose();
                }
                catch { }
                finally { StatusManagerReference = null; }
                try
                {   // Disposes SHOULD never throw exceptions ... but...
                    if (DataAccessReference != null)
                        DataAccessReference.Dispose();
                }
                catch { }
                finally { DataAccessReference = null; }
                Logger = null;
                Disposed = true;
            }
        }

        /// <summary>
        /// Disposed declares whether or not the class instance has been disposed.</summary>
        [DefaultValue(false)]
        public static bool Disposed { get; private set; }

        /// <summary>Logger holds a reference to a <see cref="EventLoggerAccess"/>instance.</summary>
        [DefaultValue(null)]
        public EventLoggerAccess Logger { get; private set; }

        protected virtual void ProcessControlUpdateRequest(string name, int value)
        {
            try { DataAccessReference.UpdatePLCTagValue(name, value); }
            catch (Exception ex) { Logger.LogError(ex); }
        }

        /// <summary>
        /// This is a safety net to ensure that resources are disposed even if
        /// <see cref="Dispose()"/> is not called.</summary>
        ~RequestManager() { Dispose(!Disposed); }

        /// <summary>Create and prepare a new class instance.</summary>
        /// <param name="dataAccess">
        /// Data Access specifies a <see cref="DataAccess"/> instance and must not be
        /// null...</param>
        /// <param name="statusManager">
        /// Stats Manager specifies a <see cref="StatusManager"/> instance and must not be
        /// null...</param>
        /// <param name="eventLogger">
        /// Event Logger specifies a <see cref="EventLoggerAccess"/> instance and must not be
        /// null...</param>
        /// <exception cref="ArgumentNullException">
        /// If either <paramref name="dataAccess"/> or <paramref name="eventLogger"/> specifies
        /// null, an exception is thrown.</exception>
        public RequestManager (DataAccess dataAccess, StatusManager statusManager, EventLoggerAccess eventLogger)
        {
            if (/*invalid?*/ eventLogger == null)
                throw new ArgumentNullException(ClassName + " EventLoggerAccess reference argument (eventLogger) must not be null");
            Logger = eventLogger;
            if (/*invalid?*/ dataAccess == null)
                throw new ArgumentNullException(ClassName + " DataAccess reference argument (dataAccess) must not be null");
            DataAccessReference = dataAccess;
            DataAccessReference.DisplayControlUpdateRequest += new DashboardControlUpdateHandler(ProcessControlUpdateRequest);
            if (/*invalid?*/ statusManager == null)
                throw new ArgumentNullException(ClassName + " StatusManager reference argument (statusManager) must not be null");
            StatusManagerReference = statusManager;
        }

        /// <summary>
        /// Status Manager Reference holds a reference to a <see cref="StatusManager"/>
        /// instance.</summary>
        [DefaultValue(null)]
        public StatusManager StatusManagerReference { get; private set; }
    }
}
