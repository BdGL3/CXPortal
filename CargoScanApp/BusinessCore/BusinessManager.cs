using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Subsystem.DataAccessCore;
using L3.Cargo.Subsystem.RequestManagerCore;
using L3.Cargo.Subsystem.StatusManagerCore;

namespace L3.Cargo.Scan.BusinessCore
{
    public class BusinessManager : IDisposable
    {
        /// <summary>
        /// This is a safety net to ensure that resources are disposed even if
        /// <see cref="Dispose()"/> is not called.</summary>
        ~BusinessManager() { Dispose(!Disposed); }

        /// <summary>Create and prepare a new class instance.</summary>
        /// <param name="dataAccess">
        /// Data Access specifies a <see cref="DataAccess"/> instance and must not be
        /// null...</param>
        /// <param name="eventLogger">
        /// Event Logger specifies a <see cref="EventLoggerAccess"/> instance and must not be
        /// null...</param>
        /// <exception cref="ArgumentNullException">
        /// If either <paramref name="dataAccess"/> or <paramref name="eventLogger"/> specifies
        /// null, an exception is thrown.</exception>
        public BusinessManager(DataAccess dataAccess, EventLoggerAccess eventLogger)
        {
            if (/*invalid?*/ eventLogger == null)
                throw new ArgumentNullException(ClassName + " EventLoggerAccess reference argument (eventLogger) must not be null");
            Logger = eventLogger;
            if (/*invalid?*/ dataAccess == null)
                throw new ArgumentNullException(ClassName + " DataAccess reference argument (dataAccess) must not be null");
            StatusManagerReference = new StatusManager(dataAccess, eventLogger);
            RequestManagerReference = new RequestManager(dataAccess, StatusManagerReference, eventLogger);
        }

        /// <summary>Class Name specifies the name of this class.</summary>
        public static string ClassName { get { return MethodBase.GetCurrentMethod().DeclaringType.Name; } }

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
                    if (RequestManagerReference != null)
                        RequestManagerReference.Dispose();
                }
                catch { }
                finally { RequestManagerReference = null; }
                try
                {   // Disposes SHOULD never throw exceptions ... but...
                    if (StatusManagerReference != null)
                        StatusManagerReference.Dispose();
                }
                catch { }
                finally { StatusManagerReference = null; }
                Logger = null;
                Disposed = true;
            }
        }

        /// <summary>
        /// Disposed declares whether or not the class instance has been disposed.</summary>
        [DefaultValue(false)]
        public static bool Disposed { get; private set; }

        /// <summary>Logger holds a reference to a <see cref="EventLoggerAccess"/> instance.</summary>
        [DefaultValue(null)]
        public EventLoggerAccess Logger { get; private set; }

        /// <summary>
        /// Request Manager Reference holds a reference to a <see cref="RequestManager"/>
        /// instance.</summary>
        [DefaultValue(null)]
        public RequestManager RequestManagerReference { get; private set; }

        /// <summary>
        /// Status Manager Reference holds a reference to a <see cref="StatusManager"/>
        /// instance.</summary>
        [DefaultValue(null)]
        public StatusManager StatusManagerReference { get; private set; }
    }
}
