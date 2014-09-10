using System;
using System.Threading;

using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.Detectors.Common;
using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Communications.OPC.Portal;
using L3.Cargo.Detectors.Common;
using L3.Cargo.Detectors.DataAccessCore;
using L3.Cargo.Detectors.StatusManagerCore;
using L3.Cargo.Subsystem.RequestManagerCore;
using L3.Cargo.Subsystem.StatusManagerCore;

namespace L3.Cargo.Detectors.BusinessCore
{
    public class BusinessManager : IDisposable
    {
        private DetectorsStatusManager _statusManager;
        private RequestManager _requestManager;
        private DetectorsDataAccess _dataAccess;
        private Calibration _calibration;
        private NormalizeData _normalize;
        private EventLoggerAccess _log;

        public BusinessManager(DetectorsDataAccess dataAccess, EventLoggerAccess log)
        {
            _dataAccess = dataAccess;
            _log = log;

            _statusManager = new DetectorsStatusManager(dataAccess, log);
            _requestManager = new RequestManager(dataAccess, (StatusManager)_statusManager, log);

            log.LogInfo("Using calibration: " + AppConfiguration.CalibrationMode.ToString());
            if (AppConfiguration.CalibrationMode == AppConfiguration.CalibrationModeEnum.Inline)
                _calibration = new CalibrationInline(log, _dataAccess, _statusManager);
            else if (AppConfiguration.CalibrationMode == AppConfiguration.CalibrationModeEnum.Persistent)
                _calibration = new CalibrationPersistent(log, _dataAccess, _statusManager);
            else if (AppConfiguration.CalibrationMode == AppConfiguration.CalibrationModeEnum.InlineStandstill)
                _calibration = new CalibrationInline(log, _dataAccess, _statusManager);
            _normalize = new NormalizeData(log, dataAccess, _calibration);

            _processThread = Threads.Create(ProcessDataThreadMethod, ref _processEnd, "Business Data thread");
            _processThread.Start();
        }

        public void FakeEndOfObject(bool useFakeObject, bool objectFound)
        {
            _normalize.FakeEndOfObject(useFakeObject, objectFound);
        }

        private void ProcessDataThreadMethod()
        {
            bool wasRunning = false;
            bool waitingForXRaysOff = false;
            int airDataCount = 0;

            while (!_processEnd.WaitOne(0))
            {
                try
                {
                    DataInfo dataInfo = _dataAccess.Detectors.RawDataCollection.Take(_processCancel.Token);
                    _dataAccess.RawDataAccess.AddDataLine(ref dataInfo);
                    int calibrationLinesNeeded = AppConfiguration.CalibrationDataLines * ((_dataAccess.OpcTags.LINAC_ENERGY_TYPE.Value == LINAC_ENERGY_TYPE_VALUE.Dual) ? 2 : 1);

                    if (AppConfiguration.CalibrationMode == AppConfiguration.CalibrationModeEnum.Inline)
                    {
                        // calibrate at the start of a scan
                        if (_dataAccess.OpcTags.LINAC_STATE.Value == LINAC_STATE_VALUE.XRaysOn && airDataCount < calibrationLinesNeeded)
                        {
                            airDataCount++;
                            _calibration.AddAirDataLine(dataInfo);
                        }
                        else if (_dataAccess.OpcTags.LINAC_STATE.Value == LINAC_STATE_VALUE.XRaysOn)
                        {
                            _normalize.AddDataLine(dataInfo);
                            wasRunning = true;
                        }
                        else if (wasRunning)
                        {
                            airDataCount = 0;
                            wasRunning = false;
                            _normalize.DataComplete();
                        }
                        else if (!_dataAccess.OpcTags.START_SCAN.Value)
                            _calibration.AddDarkDataLine(dataInfo);
                    }
                    else if (AppConfiguration.CalibrationMode == AppConfiguration.CalibrationModeEnum.Persistent)
                    {
                        // persistent calibration. Calibration is done on user command and used for subsequent scans
                        if (_dataAccess.OpcTags.CALIBRATION_SCAN.Value && _calibration.IsCalibrationRunning())
                        {
                            _calibration.AddDataLine(dataInfo);
							wasRunning = false;
                        }
                        else if (_dataAccess.OpcTags.LINAC_STATE.Value == LINAC_STATE_VALUE.XRaysOn && !_dataAccess.OpcTags.CALIBRATION_SCAN.Value)
                        {
                            _normalize.AddDataLine(dataInfo);
                            wasRunning = true;
                        }
                        else if (wasRunning)
                        {
                            wasRunning = false;
                            _normalize.DataComplete();
                            _log.LogInfo("Force End of Object");
                        }
#if false
                        else if (_dataAccess.OpcTags.CALIBRATION_SCAN.Value && _calibration.IsCalibrationRunning() == false)
                        {
                        }
                        else if (_dataAccess.OpcTags.CALIBRATION_SCAN.Value == false && _calibration.IsCalibrationRunning() == false)
                        {
                            wasRunning = false;
                        }
                        else if (_dataAccess.OpcTags.LINAC_STATE.Value == LINAC_STATE_VALUE.XRaysOn)
                        {
                            _normalize.AddDataLine(dataInfo);
                            wasRunning = true;
                        }
                        else if (wasRunning && (_dataAccess.OpcTags.LINAC_STATE.Value == LINAC_STATE_VALUE.XraysOff 
                                             || _dataAccess.OpcTags.LINAC_STATE.Value == LINAC_STATE_VALUE.ReadyForHVonCommand))
                        {
                            wasRunning = false;
                            _normalize.DataComplete();
                            _log.LogInfo("&&&&&&&&&&&&&&&&&&&&&&&&&&&");
                        }
#endif
                    }
                    else if (AppConfiguration.CalibrationMode == AppConfiguration.CalibrationModeEnum.InlineStandstill)
                    {
                        if (wasRunning == false &&
                            _dataAccess.OpcTags.SCAN_AREA_CLEAR.Value &&
                            airDataCount < calibrationLinesNeeded &&
                            _dataAccess.OpcTags.LINAC_STATE.Value != LINAC_STATE_VALUE.XRaysOn)
                        {
                            //_log.LogInfo("Enabling X-rays for calibration.");
                            _dataAccess.SetHostStopScanValue(true);
                            _dataAccess.EnableXray(true);
                        }
                        // calibrate at the start of a scan
                        else if (_dataAccess.OpcTags.LINAC_STATE.Value == LINAC_STATE_VALUE.XRaysOn &&
                           airDataCount < calibrationLinesNeeded)
                        {
                            if (!_dataAccess.OpcTags.HOST_STOP_SCAN.Value)
                            {
                                // prevent the user from scanning while the air data is being collected
                                //_log.LogInfo("Beginning air data collection. Setting HOST_STOP_SCAN true.");
                                _dataAccess.SetHostStopScanValue(true);
                            }
                            else
                            {
                                // collect air data
                                airDataCount++;
                                _calibration.AddAirDataLine(dataInfo);

                                if (airDataCount == calibrationLinesNeeded)
                                {
                                    _log.LogInfo("Air data complete, disabling X-rays.");
                                    _dataAccess.SetHostStopScanValue(false);
                                    _dataAccess.EnableXray(false);
                                    waitingForXRaysOff = true;
                                }
                            }
                        }
                        else if (_dataAccess.OpcTags.LINAC_STATE.Value == LINAC_STATE_VALUE.XRaysOn && waitingForXRaysOff == true)
                        {
                            // do nothing
                        }
                        else if (_dataAccess.OpcTags.LINAC_STATE.Value != LINAC_STATE_VALUE.XRaysOn && waitingForXRaysOff == true)
                            waitingForXRaysOff = false;
                        else if (_dataAccess.OpcTags.LINAC_STATE.Value == LINAC_STATE_VALUE.XRaysOn)
                        {
                            _normalize.AddDataLine(dataInfo);
                            wasRunning = true;
                        }
                        else if (wasRunning)
                        {
                            airDataCount = 0;
                            wasRunning = false;
                            _normalize.DataComplete();
                        }
                        else if (!_dataAccess.OpcTags.START_SCAN.Value)
                            _calibration.AddDarkDataLine(dataInfo);
                    }
                }
                catch (Exception ex)
                {
                    if (!_processEnd.WaitOne(0))
                        _log.LogError(ex);
                }
            }
        }
        private CancellationTokenSource _processCancel = new CancellationTokenSource();
        private ManualResetEvent _processEnd = new ManualResetEvent(false);
        private Thread _processThread;

        public void Dispose()
        {
            _processCancel.Cancel();
            try
            { 
                if (/*exists (avoid first try exceptions)?*/ _processThread != null)
                    _processThread = Threads.Dispose(_processThread, ref _processEnd, Utilities.Time10SECONDS);
            }
            catch { }
            finally { _processThread = null; }
            try
            {
                if (/*exists (avoid first try exceptions)?*/ _normalize != null)
                    _normalize.Dispose();
            }
            catch { }
            finally { _normalize = null; }
            _log = null;
        }
    }
}
