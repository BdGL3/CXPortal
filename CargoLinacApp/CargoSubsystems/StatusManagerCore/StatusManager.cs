using System;
using System.Collections.Generic;
using System.Configuration;
using L3.Cargo.Common.Configurations;
using L3.Cargo.Common.Dashboard.Display;
using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Subsystem.DataAccessCore;

namespace L3.Cargo.Subsystem.StatusManagerCore
{
    public class StatusManager
    {
        #region Private Members

        private object _StatusLock;

        #endregion Private Members


        #region Protected Members

        protected EventLoggerAccess Logger;

        protected DataAccess DataAccess;

        protected PLCTagUpdateHandler TagUpdate;

        protected DashboardUpdateRequestHandler DisplayUpdateRequest;

        protected StatusElements _Statuses;

        #endregion


        #region Constructors

        public StatusManager (DataAccess dataAccess, EventLoggerAccess logger)
        {
            Logger = logger;
            _Statuses = new StatusElements();
            DataAccess = dataAccess;
            
            _StatusLock = new object();

            ReadTagConfig();

            DataAccess.TagUpdate += new PLCTagUpdateHandler(PLCTagUpdate);
            DataAccess.DisplayUpdateRequest += new DashboardUpdateRequestHandler(DashboardUpdateRequest);
        }

        #endregion Constructors


        #region Private Methods

        private void PLCTagUpdate (string message, int value)
        {
            if (TagUpdate != null)
            {
                TagUpdate(message, value);
            }

            ProcessHardwareStatusUpdate(message, value);
        }

        private void DashboardUpdateRequest ()
        {
            if (DisplayUpdateRequest != null)
            {
                DisplayUpdateRequest();
            }

            SendDisplayUpdate();
        }

        protected void ReadTagConfig ()
        {
            string tagGroup = ConfigurationManager.AppSettings["TagGroup"];
            foreach (OpcTagElement tagElement in DataAccess.OpcSection.Server.TagGroup.GetElement(tagGroup).Tags)
            {
                Dictionary<int, string> valueMapping = new Dictionary<int, string>();

                try
                {
                    foreach (OpcTagElement.OpcTagValueElement valueElement in tagElement.TagValues)
                    {
                        valueMapping.Add(valueElement.Value, valueElement.Type);
                    }

                    int value = DataAccess.GetTagValue(tagElement.Name);
                    StatusElement statusElement = new StatusElement(tagElement.Name, value, tagElement.Type, valueMapping);
                    lock (_StatusLock)
                    {
                        _Statuses.Add(statusElement);
                    }
                }
                catch
                {
                    StatusElement statusElement = new StatusElement(tagElement.Name, 0, tagElement.Type, valueMapping);
                    lock (_StatusLock)
                    {
                        _Statuses.Add(statusElement);
                    }
                }
            }
        }

        protected void SendDisplayUpdate ()
        {
            SendStatusUpdate();

            lock (_StatusLock)
            {
                foreach (StatusElement statusElement in _Statuses)
                {
                    if (statusElement.Type.Contains(TagTypes.Information))
                    {
                        DataAccess.UpdateWidgets(statusElement.Name, statusElement.Value);
                    }

                    if (statusElement.Type.Contains(TagTypes.Control))
                    {
                        DataAccess.UpdateWidgets(statusElement.Name, statusElement.Value);
                    }
                }
            }
        }        

        #endregion Private Methods


        #region Protected Methods

        protected void ProcessHardwareStatusUpdate(string message, int value)
        {
            lock (_StatusLock)
            {
                StatusElement statusElement = _Statuses.Find(message);
                if (statusElement != null)
                {
                    statusElement.Value = value;

                    if (statusElement.Type.Contains(TagTypes.Status))
                    {
                        SendStatusUpdate();
                    }

                    if (statusElement.Type.Contains(TagTypes.Information))
                    {
                        DataAccess.UpdateWidgets(message, value);
                    }

                    if (statusElement.Type.Contains(TagTypes.Control))
                    {
                        DataAccess.UpdateWidgets(message, value);
                    }
                }
            }
        }

        protected void SendStatusUpdate ()
        {
            try
            {
                List<string> errorMessages = new List<string>();
                List<string> warningMessages = new List<string>();
                string color = IndicatorColors.Clear;

                lock (_StatusLock)
                {
                    foreach (StatusElement statusElement in _Statuses)
                    {
                        try
                        {
                            if (statusElement.Type.Contains(TagTypes.Status))
                            {
                                if (String.Compare(statusElement.StatusType, TagValueTypes.Error, true) == 0)
                                {
                                    errorMessages.Add(statusElement.Message);
                                    if (string.Compare(color, IndicatorColors.Unknown, true) != 0)
                                    {
                                        color = IndicatorColors.Error;
                                    }
                                }
                                else if (String.Compare(statusElement.StatusType, TagValueTypes.Warning, true) == 0)
                                {
                                    warningMessages.Add(statusElement.Message);
                                    if (string.Compare(color, IndicatorColors.Clear, true) == 0)
                                    {
                                        color = IndicatorColors.Warning;
                                    }
                                }
                                else if (String.Compare(statusElement.StatusType, TagValueTypes.Unknown, true) == 0)
                                {
                                    errorMessages.Clear();
                                    warningMessages.Clear();
                                    color = IndicatorColors.Unknown;
                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex);
                        }
                    }
                }

                DataAccess.UpdateStatusErrorMessages(errorMessages.ToArray());
                DataAccess.UpdateStatusWarningMessages(warningMessages.ToArray());
                DataAccess.UpdateStatusIndicator(color);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        #endregion Protected Methods


        #region Public Methods


        #endregion Public Methods
    }
}
