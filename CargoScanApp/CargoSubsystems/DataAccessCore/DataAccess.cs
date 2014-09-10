using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.ServiceModel;

using L3.Cargo.Common.Configurations;
using L3.Cargo.Communications.Dashboard.Display.Client;
using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Communications.OPC;
using L3.Cargo.Communications.Common;
using L3.Cargo.Subsystem.DataAccessCore;

namespace L3.Cargo.Subsystem.DataAccessCore
{
    public class DataAccess : DataAccessBase, IDisposable
    {
        /// <summary>Class Name specifies the name of this class.</summary>
        public static string ClassName { get { return MethodBase.GetCurrentMethod().DeclaringType.Name; } }

        /// <summary>
        /// This is a safety net to ensure that resources are disposed even if
        /// <see cref="Dispose()"/> is not called.</summary>
        ~DataAccess() { Dispose(!Disposed); }

        /// <summary>Create and prepare a new class instance.</summary>
        /// <param name="eventLogger">
        /// Event Logger specifies a <see cref="EventLoggerAccess"/> instance and must not be
        /// null...</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="eventLogger"/> specifies null, an exception is thrown.</exception>
        public DataAccess(EventLoggerAccess eventLogger) :
            base(eventLogger)
        {
            if (/*invalid?*/ eventLogger == null)
                throw new ArgumentNullException(ClassName + " EventLoggerAccess reference argument (eventLogger) must not be null");
            Logger = eventLogger;
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            _opcClient = new OpcClient(configuration.GetSection("opcSection") as OpcSection, ConfigurationManager.AppSettings["TagGroup"], Logger);
            _opcClient.OpcTagUpdate += new OpcTagUpdateHandler(OpcTagUpdate);
            _opcClient.Open();
        }

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
                OpcTagUpdateHandlers = null;
                try
                {   // Disposes SHOULD never throw exceptions ... but...
                    if (_opcClient != null)
                    {
                        try { _opcClient.Close(); }
                        catch { }
                        _opcClient.Dispose();
                    }
                }
                catch { }
                finally { _opcClient = null; }
                Logger = null;
                Disposed = true;
            }
        }

        /// <summary>
        /// Disposed declares whether or not the class instance has been disposed.</summary>
        [DefaultValue(false)]
        public static bool Disposed { get; private set; }

        /// <summary/>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetTagValue(string name)
        {
            int value = -1;
            try { value = _opcClient.ReadValue(name); }
            catch (Exception ex) { Logger.LogError(ex); }
            return value;
        }

        private OpcClient _opcClient;

        /// <summary/>
        public OpcSection OpcSection { get { return _opcClient.OPCSection; } }

        private void OpcTagUpdate(string name, int value)
        {
            if (OpcTagUpdateHandlers != null)
                try { OpcTagUpdateHandlers(name, value); }
                catch { }
        }

        /// <summary/>
        public event PLCTagUpdateHandler OpcTagUpdateHandlers;

        /// <summary/>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void UpdatePLCTagValue(string name, int value)
        {
            try { _opcClient.WriteShort(name, Convert.ToInt16(value)); }
            catch (Exception ex) { Logger.LogError(ex); }
        }

        /// <summary>Logger holds a reference to a <see cref="EventLoggerAccess"/> instance.</summary>
        [DefaultValue(null)]
        public EventLoggerAccess Logger { get; private set; }
    }
}
