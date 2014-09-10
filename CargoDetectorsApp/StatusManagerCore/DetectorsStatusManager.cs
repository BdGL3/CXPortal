using System;
using System.Collections.Generic;
using L3.Cargo.Common.Configurations;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Detectors.DataAccessCore;
using L3.Cargo.Subsystem.StatusManagerCore;

namespace L3.Cargo.Detectors.StatusManagerCore
{
    public class DetectorsStatusManager: StatusManager
    {
        #region private members

        private const string _DetectorConnectTag = "DETECTORS_CONNECTED";

        private const string _APCSConnectTag = "APCS_CONNECTED";

        private const string _BadDetectorsTag = "BAD_DETECTORS";

        private const string _DetectorStatusTag = "DETECTORS_STATUS";

        EventLoggerAccess _logger;

        #endregion


        #region public members

        #endregion


        #region constructors

        public DetectorsStatusManager(DetectorsDataAccess dataAccess, EventLoggerAccess logger) :
            base(dataAccess, logger)
        {
            _logger = logger;
            Dictionary<int, string> ConnectionValueMapping = new Dictionary<int, string>();
            ConnectionValueMapping.Add(0, TagValueTypes.Error);
            ConnectionValueMapping.Add(1, TagValueTypes.Clear);

            StatusElement detectorConnect = new StatusElement(_DetectorConnectTag, 0, TagTypes.Status, ConnectionValueMapping);
            //detectorConnect.Value = 
            _Statuses.Add(detectorConnect);


            StatusElement apcsConnect = new StatusElement(_APCSConnectTag, 0, TagTypes.Status, ConnectionValueMapping);
            _Statuses.Add(apcsConnect);

            dataAccess.DetectorConnectionStateUpdate +=
                    new ConnectionStateChangeHandler(OnDetectorsConnectionChange);
            dataAccess.APCSConnectionStateUpdate +=
                    new ConnectionStateChangeHandler(OnApcsConnectionChange);

            Dictionary<int, string> DetectorsValueMapping = new Dictionary<int, string>();
            DetectorsValueMapping.Add(0, TagValueTypes.Clear);
            DetectorsValueMapping.Add(1, TagValueTypes.Warning);
            DetectorsValueMapping.Add(2, TagValueTypes.Error);
            DetectorsValueMapping.Add(3, TagValueTypes.Warning);

            StatusElement detectorStatus = new StatusElement(_DetectorStatusTag, 0, TagTypes.Status, DetectorsValueMapping);
            _Statuses.Add(detectorStatus);
        }

        #endregion


        #region Private Methods

        private void OnDetectorsConnectionChange(bool isConnected)
        {
            UpdateStatusTag(_DetectorConnectTag, Convert.ToInt32(isConnected));
        }

        private void OnApcsConnectionChange(bool isConnected)
        {
            UpdateStatusTag(_APCSConnectTag, Convert.ToInt32(isConnected));
        }

        private void UpdateStatusTag(string tagName, int value)
        {
            StatusElement statusElement = _Statuses.Find(tagName);
            statusElement.Value = value;

            _logger.LogInfo("*******" + tagName + " = " + statusElement.Value + " = , Actual Value = " + value);

            base.SendStatusUpdate();
        }

        #endregion Private Methods


        #region Public Methods

        public void BadDetectorsInfo(int value)
        {
            base.DataAccess.UpdateWidgets(_BadDetectorsTag, value);
        }

        public void DetectorsUpdateStatus(DetectorsStatus value)
        {
            UpdateStatusTag(_DetectorStatusTag, (int)value);
        }

        #endregion Public Methods
    }
}
