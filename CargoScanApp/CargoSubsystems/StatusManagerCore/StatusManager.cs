using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;

using L3.Cargo.Common.Configurations;
using L3.Cargo.Common.Dashboard.Display;
using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Subsystem.DataAccessCore;

namespace L3.Cargo.Subsystem.StatusManagerCore
{
    public class StatusManager : IDisposable
    {
        /// <summary>Class Name specifies the name of this class.</summary>
        public static string ClassName { get { return MethodBase.GetCurrentMethod().DeclaringType.Name; } }

        private void DashboardUpdateRequest()
        {
            if (DisplayUpdateRequest != null)
                try { DisplayUpdateRequest(); }
                catch { }
            SendDisplayUpdate();
        }

        /// <summary>
        /// Data Access Reference holds a reference to a <see cref="DataAccess"/>
        /// instance.</summary>
        [DefaultValue(null)]
        public DataAccess DataAccessReference { get; private set; }

        protected DashboardUpdateRequestHandler DisplayUpdateRequest;

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

        private void PLCTagUpdate(string message, int value)
        {
            if (PLCTagUpdateHandlers != null)
                try { PLCTagUpdateHandlers(message, value); }
                catch { }
            ProcessHardwareStatusUpdate(message, value);
        }

        protected PLCTagUpdateHandler PLCTagUpdateHandlers;

        protected void ProcessHardwareStatusUpdate(string message, int value)
        {
            lock (_statusLock)
            {
                StatusElement element = _statusElements.Find(message);
                if (element != null)
                {
                    element.Value = value;
                    if (element.Type.Contains(TagTypes.Status))
                        SendStatusUpdate();
                    if (element.Type.Contains(TagTypes.Information))
                        try { DataAccessReference.UpdateWidgets(message, value); }
                        catch { }
                    if (element.Type.Contains(TagTypes.Control))
                        try { DataAccessReference.UpdateWidgets(message, value); }
                        catch { }
                }
            }
        }

        protected void ReadTagConfig()
        {
            string group = ConfigurationManager.AppSettings["TagGroup"];
            foreach (OpcTagElement tag in DataAccessReference.OpcSection.Server.TagGroup.GetElement(group).Tags)
            {
                Dictionary<int, string> map = new Dictionary<int, string>();
                try
                {
                    foreach (OpcTagElement.OpcTagValueElement element in tag.TagValues)
                        map.Add(element.Value, element.Type);
                    int value = DataAccessReference.GetTagValue(tag.Name);
                    StatusElement status = new StatusElement(tag.Name, value, tag.Type, map);
                    lock (_statusLock)
                        _statusElements.Add(status);
                }
                catch
                {
                    StatusElement status = new StatusElement(tag.Name, 0, tag.Type, map);
                    lock (_statusLock)
                        _statusElements.Add(status);
                }
            }
        }

        protected void SendDisplayUpdate()
        {
            SendStatusUpdate();
            lock (_statusLock)
                foreach (StatusElement element in _statusElements)
                {
                    if (element.Type.Contains(TagTypes.Information))
                        try { DataAccessReference.UpdateWidgets(element.Name, element.Value); }
                        catch { }
                    if (element.Type.Contains(TagTypes.Control))
                        try { DataAccessReference.UpdateWidgets(element.Name, element.Value); }
                        catch { }
                }
        }

        protected void SendStatusUpdate()
        {
            try
            {
                string color = IndicatorColors.Clear;
                List<string> errorMessages = new List<string>();
                List<string> warningMessages = new List<string>();
                lock (_statusLock)
                    foreach (StatusElement element in _statusElements)
                        try
                        {
                            if (element.Type.Contains(TagTypes.Status))
                                if (String.Compare(element.StatusType, TagValueTypes.Error, true) == 0)
                                {
                                    errorMessages.Add(element.Message);
                                    if (string.Compare(color, IndicatorColors.Unknown, true) != 0)
                                        color = IndicatorColors.Error;
                                }
                                else if (String.Compare(element.StatusType, TagValueTypes.Warning, true) == 0)
                                {
                                    warningMessages.Add(element.Message);
                                    if (string.Compare(color, IndicatorColors.Clear, true) == 0)
                                        color = IndicatorColors.Warning;
                                }
                                else if (String.Compare(element.StatusType, TagValueTypes.Unknown, true) == 0)
                                {
                                    color = IndicatorColors.Unknown;
                                    errorMessages.Clear();
                                    warningMessages.Clear();
                                    break;
                                }
                        }
                        catch (Exception ex) { Logger.LogError(ex); }
                DataAccessReference.UpdateStatusErrorMessages(errorMessages.ToArray());
                DataAccessReference.UpdateStatusWarningMessages(warningMessages.ToArray());
                DataAccessReference.UpdateStatusIndicator(color);
            }
            catch (Exception ex) { Logger.LogError(ex); }
        }

        protected StatusElements _statusElements;
        private object _statusLock = new object();

        /// <summary>
        /// This is a safety net to ensure that resources are disposed even if
        /// <see cref="Dispose()"/> is not called.</summary>
        ~StatusManager() { Dispose(!Disposed); }

        public StatusManager(DataAccess dataAccess, EventLoggerAccess eventLogger)
        {
            if (/*invalid?*/ eventLogger == null)
                throw new ArgumentNullException(ClassName + " EventLoggerAccess reference argument (eventLogger) must not be null");
            Logger = eventLogger;
            if (/*invalid?*/ dataAccess == null)
                throw new ArgumentNullException(ClassName + " DataAccess reference argument (dataAccess) must not be null");
            DataAccessReference = dataAccess;
            _statusElements = new StatusElements();
            ReadTagConfig();
            DataAccessReference.OpcTagUpdateHandlers += new PLCTagUpdateHandler(PLCTagUpdate);
            DataAccessReference.DisplayUpdateRequest += new DashboardUpdateRequestHandler(DashboardUpdateRequest);
        }
    }
}
