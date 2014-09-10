using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

using l3.cargo.corba;
using L3.Cargo.Communications.APCS.Client;
using L3.Cargo.Communications.APCS.Common;
using L3.Cargo.Communications.CargoHost;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.Detectors.Client;
using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Communications.OPC.Portal;
using L3.Cargo.Detectors.Common;
using L3.Cargo.Subsystem.DataAccessCore;
using System.Threading;

namespace L3.Cargo.Detectors.DataAccessCore
{
    public class DetectorsDataAccess : DataAccess, IDisposable
    {
        #region Private Members

        private DetectorsAccess _detectorsAccess;

        private ApcsAccess _apcsAccess;

        private OpcTags _OpcTags;

        private CargoHostEndPoint _cargoHostEndPoint;

        private RawDataAccess _rawDataAccess;

        private RealTimeViewer _realTimeViewer;

        #endregion Private Members


        #region Public Members

        public DetectorsAccess Detectors
        {
            get { return _detectorsAccess; }
        }

        public ApcsAccess Apcs
        {
            get { return _apcsAccess; }
        }

        public RawDataAccess RawDataAccess
        {
            get { return _rawDataAccess; }
        }

        public CargoHostEndPoint CargoHostEndPoint
        {
            get { return _cargoHostEndPoint; }
        }

        public RealTimeViewer RealTimeViewer
        {
            get { return _realTimeViewer; }
        }

        public OpcTags OpcTags
        {
            get { return _OpcTags; }
        }

        public EventLoggerAccess Logger { get { return _logger; } }

        public event ConnectionStateChangeHandler DetectorConnectionStateUpdate;

        public event ConnectionStateChangeHandler APCSConnectionStateUpdate;

        #endregion Public Members


        #region Constructors

        public DetectorsDataAccess(EventLoggerAccess logger) :
            base(logger)
        {
            _logger = logger;

            _detectorsAccess = new DetectorsAccess(_logger);
            _detectorsAccess.ReadyEvent += new ConnectionStateChangeHandler(OnDetectorsChange);

            _apcsAccess = new ApcsAccess(logger);
            _apcsAccess.ReadyEvent += new ConnectionStateChangeHandler(OnApcsChange);

            _apcsAccess.Start();
            _detectorsAccess.Start();

            _OpcTags = new OpcTags();
            base.TagUpdate += new PLCTagUpdateHandler(_OpcTags.DataAccess_TagUpdate);
            base.TagUpdate += new PLCTagUpdateHandler(DetectorsDataAccess_TagUpdate);

            _cargoHostEndPoint = new CargoHostEndPoint(AppConfiguration.CargoHostServer, AppConfiguration.CargoHostPort);
            _logger.LogInfo("Cargo Host HostEndPoint is " + _cargoHostEndPoint.IPAddress + ":" + _cargoHostEndPoint.IPPort.ToString());

            _rawDataAccess = new RawDataAccess(_logger, _detectorsAccess);
            _realTimeViewer = new RealTimeViewer(_logger);
        }

        #endregion Constructors


        #region Private Methods

        private void OnDetectorsChange(bool isReady)
        {
            if (DetectorConnectionStateUpdate != null)
                try { DetectorConnectionStateUpdate(isReady); }
                catch { }
        }

        private void OnApcsChange(bool isReady)
        {
            if (APCSConnectionStateUpdate != null)
                try { APCSConnectionStateUpdate(isReady); }
                catch { }

            if (isReady)
            {
                OperatingMode opMode;
                switch (AppConfiguration.APCSOperatingMode)
                {
                    case 1:
                        opMode = OperatingMode.AdaptiveMobile;
                            break;
                    case 2:
                        opMode = OperatingMode.AdaptivePortal;
                            break;
                    case 3:
                        opMode = OperatingMode.NonAdaptiveMobile;
                            break;
                    case 4:
                        opMode = OperatingMode.NonAdpativePortal;
                            break;
                    default:
                        opMode = OperatingMode.NonAdaptiveMobile;
                            break;
                }

                bool bSuccess = _apcsAccess.SetOperatingMode(
                        opMode, 
                        AppConfiguration.AdaptiveMinimumFrequency, AppConfiguration.AdaptiveMaximumFrequency, true);

                if (opMode == OperatingMode.AdaptiveMobile || opMode == OperatingMode.AdaptivePortal)
                {
                    _apcsAccess.SetAdaptiveModeTriggerRatio(opMode, AppConfiguration.AdaptiveModeTriggerRatio, true);
                    if (AppConfiguration.EnableAdaptiveSpeedFeedback)
                        _apcsAccess.SetAdaptiveSpeedFeedbackConfiguration(
                                AdaptiveSpeedFeedbackConfig.EnabledWithFreq,
                                AppConfiguration.AdaptiveSpeedFeedbackFrequency, true);
                }

                _apcsAccess.SetPWMOutput(PWMOutputConfig.OutputEnabled, true);
                StartDetectorDataAcq();
                SetEnergyType((LINAC_ENERGY_TYPE_VALUE)_OpcTags.LINAC_ENERGY_TYPE_STATE.Value);
                DetectorsDataAccess_TagUpdate(_OpcTags.LINAC_ENERGY_TYPE_STATE.Name,
                                              (int)_OpcTags.LINAC_ENERGY_TYPE_STATE.Value);
            }
        }

        private void DetectorsDataAccess_TagUpdate(string name, int value)
        {
            if (name == _OpcTags.LINAC_ENERGY_TYPE_STATE.Name)
            {
                LINAC_ENERGY_TYPE_VALUE energyValue = (LINAC_ENERGY_TYPE_VALUE)Enum.ToObject(typeof(LINAC_ENERGY_TYPE_VALUE), value);

                if (energyValue == LINAC_ENERGY_TYPE_VALUE.Dual)
                {
                    if (_realTimeViewer != null)
                        _realTimeViewer.IsDualEnergy = true;
                    if (_apcsAccess.Connected)
                        try
                        {
                            _apcsAccess.SetScanEnergyMode(ScanEnergyMode.Dual, true);
                            _apcsAccess.SetStaticPulseFrequency((OperatingMode)AppConfiguration.APCSOperatingMode, AppConfiguration.DualPulseFrequency);
                            _apcsAccess.SetCurrentPulseWidth(PulseWidth.PulseWidth1, true);
                        }
                        catch { }
                }
                else if (energyValue == LINAC_ENERGY_TYPE_VALUE.High)
                {
                    if (_realTimeViewer != null)
                        _realTimeViewer.IsDualEnergy = false;
                    if (_apcsAccess.Connected)
                        try
                        {
                            _apcsAccess.SetScanEnergyMode(ScanEnergyMode.High, true);
                            _apcsAccess.SetStaticPulseFrequency((OperatingMode)AppConfiguration.APCSOperatingMode, AppConfiguration.HighPulseFrequency);
                            _apcsAccess.SetCurrentPulseWidth(PulseWidth.PulseWidth2, true);
                        }
                        catch { }
                }
                else if (energyValue == LINAC_ENERGY_TYPE_VALUE.Low)
                {
                    if (_realTimeViewer != null)
                        _realTimeViewer.IsDualEnergy = false;
                    if (_apcsAccess.Connected)
                        try
                        {
                            _apcsAccess.SetScanEnergyMode(ScanEnergyMode.Low, true);
                            _apcsAccess.SetStaticPulseFrequency((OperatingMode)AppConfiguration.APCSOperatingMode, AppConfiguration.LowPulseFrequency);
                            _apcsAccess.SetCurrentPulseWidth(PulseWidth.PulseWidth3, true);
                        }
                        catch { }
                }
#if LinacLowDose
                else if (energyValue == LINAC_ENERGY_TYPE_VALUE.LowDose)
                {
                    _realTimeViewer.IsDualEnergy = false;

                    if (_apcsAccess.Connected)
                    {
                        try
                        {
                            _apcsAccess.SetScanEnergyMode(ScanEnergyMode.LowDose, true);
                            _apcsAccess.SetStaticPulseFrequency((OperatingMode)AppConfiguration.APCSOperatingMode, AppConfiguration.LowPulseFrequency, true);
                            _apcsAccess.SetCurrentPulseWidth(PulseWidth.PulseWidth3, true);
                        }
                        catch { }
                    }
                }
#endif
            }
        }


        #endregion Private Methods


        #region Public Methods

        public void Connect()
        {
            _apcsAccess.Start();
            _cargoHostEndPoint.Open();
            _rawDataAccess.Open();
        }

        /*!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
         * StartDetectorDataAcq is invoked upon establishment of the
         * APCS instance's connection. At that time, there is no
         * guarantee that the Detectors instance has already established
         * all of its underlying NCB command connections. APCS initiates
         * its connection when the above public method, Connect, is
         * invoked, which establishes the APCS connection. Connect may be
         * invoked also from DetectorsApp/MainWindow.xaml.cs.
         *!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/
        public bool StartDetectorDataAcq(string reason = null)
        {
            _detectorsAccess.SourcesSynchronized = false;
            
            // If all of the detector servers are connected, stop the
            // detector timing pulses, then, reset the line identities
            // of the detector servers, then, start the detectors' data
            // connections and, finally, restart the the timing pulses.
            if (!Detectors.CommandIsReady)
            {
                _logger.LogError("All NCB(s) not yet ready");
                return false;
            }
            if (/*fail?*/ !Apcs.DetectorsTimingEnable(false))
            {
                _logger.LogError("Apcs.DetectorsTimingEnable(false) failed");
                return false;
            }
            try { Detectors.ResetLineCount(); }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return false;
            }
            if (/*fail?*/ !Apcs.DetectorsTimingEnable(true))
            {
                _logger.LogError("Apcs.DetectorsTimingEnable(true) failed");
                return false;
            }

            _detectorsAccess.SourcesSynchronized = true;

            // All NCBs are connected and APCS timing has been started: the NCBs' LineID counters
            // have been synchronized.
            return true;
        }

        public void StartCaseManagerScan()
        {
            if (_cargoHostEndPoint.IsHostAvailable)
            {
                if (!_cargoHostEndPoint.IsScanAreaScanning())
                {
                    string[] caseIds = _cargoHostEndPoint.GetScanAreaCases();
                    string caseId = (caseIds.Length > 0) ? caseId = caseIds[0] : _cargoHostEndPoint.CreateScanAreaCase();
                    _logger.LogInfo("Case created with Id: " + caseId); 
                    _cargoHostEndPoint.ScanAreaStartScan(caseId);
                }
            }
        }

        public string GetCaseManagerScanCaseId()
        {
            if (_cargoHostEndPoint.IsHostAvailable && _cargoHostEndPoint.IsScanAreaScanning())
            {
                XCase xcase = _cargoHostEndPoint.GetCase(_cargoHostEndPoint.GetScanAreaScanningCase());
                return xcase.getId();
            }

            return null;
        }

        public bool IsCaseManagerScanning()
        {
            if (_cargoHostEndPoint.IsHostAvailable)
            {
                return _cargoHostEndPoint.IsScanAreaScanning();
            }
            
            return false;
        }

        public void StopCaseManagerScan(string pxeFile)
        {
            if (_cargoHostEndPoint.IsHostAvailable && _cargoHostEndPoint.IsScanAreaScanning())
            {
                XCase xcase = _cargoHostEndPoint.GetCase(_cargoHostEndPoint.GetScanAreaScanningCase());
                xcase.addImage(pxeFile);
                _cargoHostEndPoint.ScanAreaStopScan();
            }
        }

        public void AbortCaseManagerScan()
        {
            if (_cargoHostEndPoint.IsHostAvailable && _cargoHostEndPoint.IsScanAreaScanning())
            {
                _cargoHostEndPoint.AbortScanCase();
            }
        }

        public void EnableXray(bool enable)
        {
            base.UpdatePLCTagValue(_OpcTags.LINAC_TURN_ON_XRAYS.Name, Convert.ToInt32(enable));
        }

        public void StopScan()
        {
            base.UpdatePLCTagValue(_OpcTags.HOST_STOP_SCAN.Name, 1);
        }

        public void SetEnergyType(LINAC_ENERGY_TYPE_VALUE value)
        {
            int energy = (int)Enum.ToObject(typeof(LINAC_ENERGY_TYPE_VALUE), value);
            base.UpdatePLCTagValue(_OpcTags.LINAC_ENERGY_TYPE.Name, energy);
        }

        public void SetCalibrationState(CALIBRATION_STATE_VALUE value)
        {
            UpdatePLCTagValue(_OpcTags.CALIBRATION_STATE.Name, (int)value);
        }

        public void SetHostStopScanValue(bool hostStopScan)
        {
            UpdatePLCTagValue(_OpcTags.HOST_STOP_SCAN.Name, Convert.ToInt32(hostStopScan));
        }

        public override void Dispose()
        {
            try
            {
                if (/*exists (avoid first try exceptions)?*/ _rawDataAccess != null)
                    _rawDataAccess.Dispose();
            }
            catch { }
            finally { _rawDataAccess = null; }

            try
            {
                if (/*exists (avoid first try exceptions)?*/ _realTimeViewer != null)
                    _realTimeViewer.Dispose();
            }
            catch { }
            finally { _realTimeViewer = null; }

            try
            {
                if (/*exists (avoid first try exceptions)?*/ _detectorsAccess != null)
                    _detectorsAccess.Dispose();
            }
            catch { }
            finally { _detectorsAccess = null; }

            try
            {
                if (/*exists (avoid first try exceptions)?*/ _apcsAccess != null)
                    _apcsAccess.Dispose();
            }
            catch { }
            finally { _apcsAccess = null; }

            try
            {
                if (/*exists (avoid first try exceptions)?*/ _cargoHostEndPoint != null)
                    _cargoHostEndPoint.Close();
            }
            catch { }
            finally { _cargoHostEndPoint = null; }

            try { base.Dispose(); } catch { }
        }

        #endregion Public Methods
    }
}
