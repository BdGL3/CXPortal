using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using L3.Cargo.Common.PxeAccess;
using L3.Cargo.Communications.APCS.Common;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.Detectors.Common;
using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Communications.OPC.Portal;
using L3.Cargo.Detectors.Common;
using L3.Cargo.Detectors.DataAccessCore;
using L3.Cargo.Detectors.StatusManagerCore;
using L3.Cargo.Subsystem.DataAccessCore;

namespace L3.Cargo.Detectors.BusinessCore
{
    class CalibrationPersistent : Calibration
    {
        private DetectorsStatusManager _statusMgr;

        private BlockingCollection<DataInfo> _rawDataColl;

        private Dictionary<PulseWidth, CalibrationDataCollection> _DualDataCollection;
        private Dictionary<PulseWidth, CalibrationDataCollection> _HighDataCollection;
        private Dictionary<PulseWidth, CalibrationDataCollection> _LowDataCollection;
#if LinacLowDose
        private Dictionary<PulseWidth, CalibrationDataCollection> _LowDoseDataCollection;
#endif

        //CHANGE THIS TO THIS VALUE FOR RELEASE
        //        private const int _defaultTimerTimeout = 15000;
        //        private const int _calibrationTimerTimeout = 300000;  
        //        private const int _dataCollectionTableTimeout = 30000;

        private const int _defaultTimerTimeout = 150000;
        private const int _calibrationTimerTimeout = 150000;
        private const int _dataCollectionTableTimeout = 1500000;

        private PulseWidth _currentPulseWidth;
        private PulseWidth _setPulseWidth;
        private PulseWidth _startingPulseWidth;

        private ScanEnergyMode _currentApcsEnergyMode;
        private ScanEnergyMode _setApcsEnergyMode;
        private ScanEnergyMode _startingApcsEnergyMode;

        private LINAC_ENERGY_TYPE_VALUE _setLinacEnergyType;
        private LINAC_ENERGY_TYPE_VALUE _startingLinacEnergyType;

        private LINAC_STATE_VALUE _lastLinacState;
#if false
        private CALIBRATION_STATE_VALUE _lastCalibrationState;
#endif

        private bool _IsPerformingCalibration;
        private bool _IsCollectingData;
        private bool _allCalibrationDataFound;
        private bool _calibrationLoaded;

        private string _calibrationStorageLocation;

        private Timer _calibrationTimer;
        private Timer _xrayOnTimer;

        private static AutoResetEvent _linacEnergyTagUpdated;
        private static AutoResetEvent _apcsEnergyValueUpdated;
        private static AutoResetEvent _apcsPulseWidthValueUpdated;
        private static AutoResetEvent _linacStateTagUpdated;

        private int _warningMaxNumBadDetectors;
        private int _errorMaxNumBadDetectors;
        private bool _contiguousNumBadDetectorsFound;
        private int _numBadDetectors;

        #region Public Members

        public override bool IsCalibrationRunning()
        {
            return _IsPerformingCalibration;
        }

        public override bool IsCalibrationValid()
        {
            return _calibrationLoaded & _allCalibrationDataFound;
        }

        public bool ContiguousBadDetectorsFound
        {
            get { return _contiguousNumBadDetectorsFound; }
        }

        public int NumBadDectectors
        {
            get { return _numBadDetectors; }
        }

        public int WarningMaxNumBadDetectors
        {
            get { return _warningMaxNumBadDetectors; }
        }

        public int ErrorMaxNumBadDetectors
        {
            get { return _errorMaxNumBadDetectors; }
        }
        //public Dictionary<PulseWidth, CalibrationDataCollection> DualDataCollection
        //{
        //    get { return _DualDataCollection; }
        //}

        //public Dictionary<PulseWidth, CalibrationDataCollection> HighDataCollection
        //{
        //    get { return _HighDataCollection; }
        //}

        //public Dictionary<PulseWidth, CalibrationDataCollection> LowDataCollection
        //{
        //    get { return _LowDataCollection; }
        //}

        public bool IsPerformingCalibration
        {
            get { return _IsPerformingCalibration; }
        }

        #endregion

        /// <summary>
        /// This is a safety net to ensure that resources are disposed even if
        /// <see cref="Dispose()"/> is not called.</summary>
        ~CalibrationPersistent() { Dispose(); }

        public CalibrationPersistent(EventLoggerAccess log, DetectorsDataAccess dataAccess, DetectorsStatusManager statusManager)
        {
            SetupCalibration(log, dataAccess, statusManager);

            ///////////////////
            LoadStoredData();
        }

        public new void Dispose()
        {
            Dispose(!Disposed);
            GC.SuppressFinalize(this);
        }
        protected void Dispose(bool isDisposing)
        {
            if (/*dispose?*/ isDisposing)
                try
                {
                    if (/*exists (avoid first try exceptions)?*/ _stopCalibrationThread != null)
                        _stopCalibrationThread = Threads.Dispose(_stopCalibrationThread, ref _stopCalibrationEnd);
                }
                catch { }
                finally { _stopCalibrationThread = null; }
            base.Dispose(isDisposing);
        }

        public override void AddDataLine(DataInfo dataInfo)
        {
            if (_IsCollectingData)
                if (!_rawDataColl.TryAdd(dataInfo, AppConfiguration.DataLineProcessTimeout))
                    _logger.LogError("Not able to add data line to Calibration data collection");
        }

        public override void AddReferenceCorrection(XRayInfoIDStruct lineInfo, double referenceData)
        {
            GetCurrentCollection().AddReferenceData(lineInfo.Energy, referenceData);
        }

        public override void ResetReferenceCorrection(XRayInfoIDStruct lineInfo)
        {
            GetCurrentCollection().ClearReferenceData(lineInfo.Energy);
        }

        public override Pixel[] GetAirData(XRayInfoIDStruct lineInfo)
        {
            return GetCurrentCollection().GetAirData(lineInfo.Energy);
        }

        public override Pixel[] GetDarkData(XRayInfoIDStruct lineInfo)
        {
            return GetCurrentCollection().GetDarkData(lineInfo.Energy);
        }

        public override Pixel[] GetAirDataCollection(XRayInfoIDStruct lineInfo)
        {
            return GetCurrentCollection().GetAirDataCollection(lineInfo.Energy);
        }

        public override Pixel[] GetDarkDataCollection(XRayInfoIDStruct lineInfo)
        {
            return GetCurrentCollection().GetDarkDataCollection(lineInfo.Energy);
        }

        public override float[] GetScaleFactor(XRayInfoIDStruct lineInfo)
        {
            return GetCurrentCollection().GetScaleFactor(lineInfo.Energy);
        }

        public override float[] GetReferenceCorrections(XRayInfoIDStruct lineInfo)
        {
            return GetCurrentCollection().GetReferenceData(lineInfo.Energy);
        }

        private CalibrationDataCollection GetCurrentCollection()
        {
            // check if data is loaded
            if (!_calibrationLoaded && !_IsPerformingCalibration)
            {
                LoadStoredData();
            }

            return GetCalibrationCollection(_dataAccess.OpcTags.LINAC_ENERGY_TYPE.Value, _dataAccess.Apcs.CurrentPulseWidth);
        }

        protected override void SetupCalibration(EventLoggerAccess log, DetectorsDataAccess dataAccess, DetectorsStatusManager statusManager)
        {
            base.SetupCalibration(log, dataAccess, statusManager);

            _statusMgr = statusManager;
            
            _warningMaxNumBadDetectors = Math.Max((int)(AppConfiguration.WarningPercentBadDetectors * dataAccess.Detectors.PixelsPerColumn), 1);
            _errorMaxNumBadDetectors = Math.Max((int)(AppConfiguration.ErrorPercentBadDetectors * dataAccess.Detectors.PixelsPerColumn), 1);
            _contiguousNumBadDetectorsFound = false;

            _allCalibrationDataFound = false;
            _calibrationStorageLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Calibrations");

            _DualDataCollection = new Dictionary<PulseWidth, CalibrationDataCollection>();
            _HighDataCollection = new Dictionary<PulseWidth, CalibrationDataCollection>();
            _LowDataCollection = new Dictionary<PulseWidth, CalibrationDataCollection>();
            //_LowDoseDataCollection = new Dictionary<PulseWidth, CalibrationDataCollection>();

            //foreach (PulseWidth pulseWidth in Enum.GetValues(typeof(PulseWidth)))
            {
                _DualDataCollection.Add(PulseWidth.PulseWidth1, new CalibrationDataCollection());
                _HighDataCollection.Add(PulseWidth.PulseWidth2, new CalibrationDataCollection());
                _LowDataCollection.Add(PulseWidth.PulseWidth3, new CalibrationDataCollection());
                //_LowDoseDataCollection.Add(PulseWidth.PulseWidth3, new CalibrationDataCollection());
            }
            _calibrationLoaded = false;

            _linacEnergyTagUpdated = new AutoResetEvent(false);
            _apcsEnergyValueUpdated = new AutoResetEvent(false);
            _apcsPulseWidthValueUpdated = new AutoResetEvent(false);
            _linacStateTagUpdated = new AutoResetEvent(false);

            //subscribe to tag updates
            _dataAccess.TagUpdate += new PLCTagUpdateHandler(DataAccess_TagUpdate);
            _dataAccess.Apcs.ApcsUpdate += new ApcsUpdateHandler(Apcs_ApcsUpdate);
        }

        private void Apcs_ApcsUpdate(CommandEnum command, ActionEnum action, byte subAction, object data)
        {
            if (command == CommandEnum.PulseWidth && action == ActionEnum.Response)
                _currentPulseWidth = (PulseWidth)PulseWidth.ToObject(typeof(PulseWidth), subAction);
            else if (command == CommandEnum.ScanMode && action == ActionEnum.Response)
                _currentApcsEnergyMode = (ScanEnergyMode)ScanEnergyMode.ToObject(typeof(ScanEnergyMode), subAction);
        }

        private void DataAccess_TagUpdate(string name, int value)
        {
            if (_dataAccess.OpcTags.CALIBRATION_SCAN.Value && _dataAccess.OpcTags.CALIBRATION_STATE.Value == CALIBRATION_STATE_VALUE.TriggerStart)
            {
                if (_dataAccess.OpcTags.CALIBRATION_STATE.Value != CALIBRATION_STATE_VALUE.NotReady)
                {
                    _logger.LogInfo("Starting Calibration");
                    _lastLinacState = LINAC_STATE_VALUE.WarmingUp;
                    StartCalibration();
                }
            }
            else if (_IsPerformingCalibration)
            {
                if (_dataAccess.OpcTags.CALIBRATION_SCAN.Name == name && !Convert.ToBoolean(value))
                {
                    _logger.LogInfo("****   Calibration Cancelled");
                    StopCalibration();
                }
                else if (_dataAccess.OpcTags.CALIBRATION_STATE.Value == CALIBRATION_STATE_VALUE.Completed)
                {
                    _logger.LogInfo("****  Calibration Complete");
                    StopCalibration();
                }
                else if (_dataAccess.OpcTags.LINAC_STATE.Name == name && (LINAC_STATE_VALUE)Enum.ToObject(typeof(LINAC_STATE_VALUE), value) != _lastLinacState)
                {
                    _lastLinacState = (LINAC_STATE_VALUE)Enum.ToObject(typeof(LINAC_STATE_VALUE), value);

                    //_logger.LogInfo("#### Linac state [" + _dataAccess.OpcTags.LINAC_STATE.Name + "] = " + value.ToString());
                    StopXrayTimer();
                    _linacStateTagUpdated.Set();

                    if (_dataAccess.OpcTags.CALIBRATION_STATE.Value != CALIBRATION_STATE_VALUE.Completed && AppConfiguration.CalibrationMode == AppConfiguration.CalibrationModeEnum.Persistent)
                    {
                        //_logger.LogInfo("****  Starting Calibration - called with " + ((LINAC_STATE_VALUE)Enum.ToObject(typeof(LINAC_STATE_VALUE), value) == LINAC_STATE_VALUE.XRaysOn));
                        StartDataCollection((LINAC_STATE_VALUE)Enum.ToObject(typeof(LINAC_STATE_VALUE), value) == LINAC_STATE_VALUE.XRaysOn);
                    }
                }
                else if (_dataAccess.OpcTags.LINAC_ENERGY_TYPE_STATE.Name == name && (LINAC_ENERGY_TYPE_VALUE)Enum.ToObject(typeof(LINAC_ENERGY_TYPE_VALUE), value) == _setLinacEnergyType)
                    _linacEnergyTagUpdated.Set();
            }
            else if (_dataAccess.OpcTags.LINAC_STATE.Name == name)
            {
                StopXrayTimer();
                //_logger.LogInfo("Getting LINAC_STATE: " + value.ToString());
                _linacStateTagUpdated.Set();
            }
            else if (_dataAccess.OpcTags.LINAC_ENERGY_TYPE_STATE.Name == name)
                _linacEnergyTagUpdated.Set();
        }

        private void StartCalibration()
        {
            try
            {
                _logger.LogInfo("Trying to Clear Old Calibration Data");

                _DualDataCollection[PulseWidth.PulseWidth1].Clear();
                _HighDataCollection[PulseWidth.PulseWidth2].Clear();
                _LowDataCollection[PulseWidth.PulseWidth3].Clear();
                //_LowDoseDataCollection[PulseWidth.PulseWidth3].Clear();
            }
            catch
            {
                _logger.LogInfo("Clearing Calibration Data Failed");
            }
            finally
            {
                _logger.LogInfo("Cleared last calibration");
            }

            _logger.LogInfo("Stopping Timers");
            StopTimers();
            _IsPerformingCalibration = true;

            _dataAccess.SetCalibrationState(CALIBRATION_STATE_VALUE.Ready);

            if ((_setupThread == null) || !_setupThread.IsAlive)
            {
                if (_setupThread != null)
                    try { _setupThread = Threads.Dispose(_setupThread, ref _setupEnd); }
                    catch { }
                _setupThread = Threads.Create(SetupAgent, ref _setupEnd, "Calibration Setup thread");
                _setupThread.Start();
            }

            _calibrationTimer = new Timer(new TimerCallback(FailedCalibration), "Calibration", _calibrationTimerTimeout, Timeout.Infinite);
        }

        private void StopCalibration()
        {
            _stopCalibrationThread = new Thread(new ParameterizedThreadStart(delegate
            {
                _IsPerformingCalibration = false;
                _IsCollectingData = false;

                //_logger.LogInfo("Set X-Rays off");
                SetXrayOn(false);

                //_logger.LogInfo("Stopping setup thread");
                try
                {
                    if (/*exists (avoid first try exceptions)?*/ _setupThread != null)
                        _setupThread = Threads.Dispose(_setupThread, ref _setupEnd);
                }
                catch { }
                finally { _setupThread = null; }

                //_logger.LogInfo("Stopping dark data thread");
                try
                {
                    if (/*exists (avoid first try exceptions)?*/ _collectDarkDataThread != null)
                        _collectDarkDataThread = Threads.Dispose(_collectDarkDataThread, ref _collectDarkDataEnd);
                }
                catch { }
                finally { _collectDarkDataThread = null; }

                //_logger.LogInfo("Stopping air data thread");
                try
                {
                    if (/*exists (avoid first try exceptions)?*/ _collectAirDataThread != null)
                        _collectAirDataThread = Threads.Dispose(_collectAirDataThread, ref _collectAirDataEnd);
                }
                catch { }
                finally { _collectAirDataThread = null; }

                //_logger.LogInfo("Stopping Timers");
                StopTimers();

                //Thread.Sleep(1000);
                //_logger.LogInfo("Setting Linac Energy");
                //SetLinacEnergyMode(_startingLinacEnergyType);
                //Thread.Sleep(1000);
                //_logger.LogInfo("Setting Pulse Width");
                SetApcsPulseWidth(_startingPulseWidth);
                Thread.Sleep(1000);
                //_logger.LogInfo("Setting APCS energy");

                switch(_dataAccess.OpcTags.LINAC_ENERGY_TYPE_STATE.Value)
                {
                    case LINAC_ENERGY_TYPE_VALUE.Dual:
                        SetApcsEnergyMode(ScanEnergyMode.Dual);
                        break;
                    case LINAC_ENERGY_TYPE_VALUE.High:
                        SetApcsEnergyMode(ScanEnergyMode.High);
                        break;
                    case LINAC_ENERGY_TYPE_VALUE.Low:
                        SetApcsEnergyMode(ScanEnergyMode.Low);
                        break;
#if LinacLowDose
                    case LINAC_ENERGY_TYPE_VALUE.LowDose:
                        SetApcsEnergyMode(ScanEnergyMode.LowDose);
                        break;
#endif
                }
                //_logger.LogInfo("Calibrations Stopped");
                _linacStateTagUpdated.Reset();
                _linacEnergyTagUpdated.Reset();
                _apcsEnergyValueUpdated.Reset();
                _apcsPulseWidthValueUpdated.Reset();
            }));
            _stopCalibrationThread.IsBackground = true;
            _stopCalibrationThread.Name = "Calibration Stop thread";
            _stopCalibrationEnd.Reset();
            _stopCalibrationThread.Start();
        }
        private ManualResetEvent _stopCalibrationEnd = new ManualResetEvent(false);
        private Thread _stopCalibrationThread;

        private void StopTimers()
        {
            StopXrayTimer();
            StopCalibrationTimer();
        }

        private void StopXrayTimer()
        {
            try
            {
                if (/*exists (avoid first try exceptions)?*/ _xrayOnTimer != null)
                    _xrayOnTimer.Dispose();
            }
            catch { }
            finally { _xrayOnTimer = null; }
        }

        private void StopCalibrationTimer()
        {
            try
            {
                if (/*exists (avoid first try exceptions)?*/ _calibrationTimer != null)
                    _calibrationTimer.Dispose();
            }
            catch { }
            finally { _calibrationTimer = null; }
        }

        private void SetupAgent()
        {
            _setApcsEnergyMode = ScanEnergyMode.Dual;
            _setLinacEnergyType = LINAC_ENERGY_TYPE_VALUE.Dual;
            _setPulseWidth = PulseWidth.PulseWidth1;

            _logger.LogInfo("Getting APCS status");

            //GetApcsStatus();

            _startingApcsEnergyMode = _currentApcsEnergyMode;
            _startingLinacEnergyType = _dataAccess.OpcTags.LINAC_ENERGY_TYPE_STATE.Value;
            _startingPulseWidth = _currentPulseWidth;

            _rawDataColl = new BlockingCollection<DataInfo>();

            if (_dataAccess.OpcTags.LINAC_STATE.Value == LINAC_STATE_VALUE.XRaysOn)
            {
                _logger.LogInfo("Setting x-rays off");
                SetXrayOn(false);
            }
            else
            {
                _logger.LogInfo("Start Collecting Data");

                //if(AppConfiguration.CalibrationMode != AppConfiguration.CalibrationModeEnum.Persistent)
                    StartDataCollection(false);
            }
        }
        private ManualResetEvent _setupEnd = new ManualResetEvent(false);
        private Thread _setupThread;

        private void StartDataCollection(bool isXraysEnabled)
        {
            if (isXraysEnabled)
            {
                _logger.LogInfo("Collecting Air Data");
                try { _collectAirDataThread = Threads.Dispose(_collectAirDataThread, ref _collectAirDataEnd); }
                catch { }
                _collectAirDataThread = Threads.Create(CollectAirDataAgent, ref _collectAirDataEnd, "Air Data Collection thread");
                _collectAirDataThread.Start();
            }
            else
            {
                _logger.LogInfo("Collecting Dark Data");
                try { _collectDarkDataThread = Threads.Dispose(_collectDarkDataThread, ref _collectDarkDataEnd); }
                catch { }
                _collectDarkDataThread = Threads.Create(CollectDarkDataAgent, ref _collectDarkDataEnd, "Dark Data Collection thread");
                _collectDarkDataThread.Start();
            }
        }

        private void CollectDarkDataAgent()
        {
            if (_IsPerformingCalibration)
                _dataAccess.SetCalibrationState(CALIBRATION_STATE_VALUE.DarkData);

            if (_IsPerformingCalibration)
            {
                _logger.LogInfo("Setting Linac energy type in begin dark data");
                SetLinacEnergyMode(_setLinacEnergyType);
                _logger.LogInfo("Finished setting Linac energy type in begin dark data");
            }

            if (_IsPerformingCalibration)
            {
                _logger.LogInfo("Setting apcs energy type in begin dark data");
                SetApcsEnergyMode(_setApcsEnergyMode);
                _logger.LogInfo("Finished setting apcs energy type in begin dark data");
            }

            if (_IsPerformingCalibration)
            {
                _logger.LogInfo("Setting pulse width in begin dark data");
                SetApcsPulseWidth(_setPulseWidth);
                _logger.LogInfo("Finished Setting Config for Dark");
            }

            if (_IsPerformingCalibration)
            {
                _logger.LogInfo("Collecting Dark Data");
                CollectData(PixelDataType.Dark);
                _logger.LogInfo("Finished Collecting Dark Data");
            }

            if (_IsPerformingCalibration)
                _dataAccess.SetCalibrationState(CALIBRATION_STATE_VALUE.DarkDataComplete);

            if (_IsPerformingCalibration)
            {
                _logger.LogInfo("Set X-Rays on");
                SetXrayOn(true);
                _logger.LogInfo("Finished setting X-ray on");
            }
        }
        private ManualResetEvent _collectDarkDataEnd = new ManualResetEvent(false);
        private Thread _collectDarkDataThread;

        private void CollectAirDataAgent()
        {
            if (_IsPerformingCalibration)
            {
                //CALIBRATION_STATE_VALUE state = (_dataAccess.OpcTags.LINAC_ENERGY_TYPE_STATE.Value == LINAC_ENERGY_TYPE_VALUE.Dual) ? CALIBRATION_STATE_VALUE.DualEnergy :
                //                                    (_dataAccess.OpcTags.LINAC_ENERGY_TYPE_STATE.Value == LINAC_ENERGY_TYPE_VALUE.High) ? CALIBRATION_STATE_VALUE.HighEnergy : CALIBRATION_STATE_VALUE.LowEnergy;
                CALIBRATION_STATE_VALUE state;
                switch (_dataAccess.OpcTags.LINAC_ENERGY_TYPE_STATE.Value)
                {
                    case LINAC_ENERGY_TYPE_VALUE.Dual:
                        state = CALIBRATION_STATE_VALUE.DualEnergy;
                        break;
                    case LINAC_ENERGY_TYPE_VALUE.High:
                        state = CALIBRATION_STATE_VALUE.HighEnergy;
                        break;
                    case LINAC_ENERGY_TYPE_VALUE.Low:
                        state = CALIBRATION_STATE_VALUE.LowEnergy;
                        break;
#if LinacLowDose
                    case LINAC_ENERGY_TYPE_VALUE.LowDose:
                        state = CALIBRATION_STATE_VALUE.LowDoseLowEnergy;
                        break;
#endif
                    default:
                        state = CALIBRATION_STATE_VALUE.DualEnergy;
                        break;
                } 
                
                _dataAccess.SetCalibrationState(state);
            }

            if (_IsPerformingCalibration)
            {
                _logger.LogInfo("Collecting Air Data");
                CollectData(PixelDataType.Air);
                _logger.LogInfo("Finished Collecting Air Data");
            }

            if (_IsPerformingCalibration)
            {
                CALIBRATION_STATE_VALUE state;
                
                switch (_dataAccess.OpcTags.LINAC_ENERGY_TYPE_STATE.Value)
                {
                    case LINAC_ENERGY_TYPE_VALUE.Dual:
                        state = CALIBRATION_STATE_VALUE.DualEnergyComplete;
                        break;
                    case LINAC_ENERGY_TYPE_VALUE.High:
                        state = CALIBRATION_STATE_VALUE.HighEnergyComplete;
                        break;
                    case LINAC_ENERGY_TYPE_VALUE.Low:
                        state = CALIBRATION_STATE_VALUE.LowEnergyComplete;
                        break;
#if LinacLowDose
                    case LINAC_ENERGY_TYPE_VALUE.LowDose:
                        state = CALIBRATION_STATE_VALUE.LowDoseLowEnergyComplete;
                        break;
#endif
                    default:
                        state = CALIBRATION_STATE_VALUE.DualEnergyComplete;
                        break;
                }
                _dataAccess.SetCalibrationState(state);
            }

            if (_IsPerformingCalibration)
            {
                _logger.LogInfo("Getting Next Config");
                SetNextConfiguration();
                _logger.LogInfo("Finished Next Config");
            }

            if (_IsPerformingCalibration)
            {
                _logger.LogInfo("Set X-Rays off");
                SetXrayOn(false);
                _logger.LogInfo("Finished setting X-ray off");
            }
        }
        private ManualResetEvent _collectAirDataEnd = new ManualResetEvent(false);
        private Thread _collectAirDataThread;

        private void CollectData(PixelDataType dataType)
        {
            bool once = false;
            List<Pixel[]> highDataList = new List<Pixel[]>();
            List<Pixel[]> lowDataList = new List<Pixel[]>();
            LINAC_ENERGY_TYPE_VALUE linacEnergy = _dataAccess.OpcTags.LINAC_ENERGY_TYPE_STATE.Value;

            _IsCollectingData = true;
            Timer collectionTimer = new Timer(new TimerCallback(FailedCalibration), dataType.ToString() + " Data", _dataCollectionTableTimeout, Timeout.Infinite);

            _logger.LogInfo("Starting data collection loop.");
            _logger.LogInfo("Linac energy is " + linacEnergy.ToString());

            while (_IsPerformingCalibration && _IsCollectingData)
            {
                DataInfo dataInfo;
                //_logger.LogInfo("Try to take raw data.");

                if (_rawDataColl.TryTake(out dataInfo, 500))
                {
                    CalibrationDataCollection collection;
                    XRayEnergyEnum energy = (dataInfo.XRayInfo.Energy == XRayEnergyEnum.HighEnergy) ? XRayEnergyEnum.HighEnergy : XRayEnergyEnum.LowEnergy;
                    //_logger.LogInfo("Data line energy is " + energy.ToString() + " and Linac energy is " + linacEnergy.ToString());

                    switch(linacEnergy)
                    {
                        case LINAC_ENERGY_TYPE_VALUE.Dual:
                            collection = _DualDataCollection[PulseWidth.PulseWidth1];
                            break;
                        case LINAC_ENERGY_TYPE_VALUE.High:
                            collection = _HighDataCollection[PulseWidth.PulseWidth2];
                            break;
                        case LINAC_ENERGY_TYPE_VALUE.Low:
                            collection = _LowDataCollection[PulseWidth.PulseWidth3];
                            break;
#if LinacLowDose
                    case LINAC_ENERGY_TYPE_VALUE.LowDose:
                        collection = _LowDoseDataCollection[PulseWidth.PulseWidth3];
                        break;
#endif
                        default:
                            collection = _DualDataCollection[PulseWidth.PulseWidth1];
                            break;
                    }
                    collection.AddData(energy, dataType, dataInfo.LineData);
                    if (once == false)
                    {
                        _logger.LogInfo("Adding Data of " + energy.ToString() + ", " + dataType.ToString());
                        once = true;
                    }

                    _IsCollectingData = !collection.IsComplete(energy, dataType);
                    //_logger.LogInfo("Still collecting data? " + _IsCollectingData);
                }
            }
            collectionTimer.Dispose();
        }

        private void SetNextConfiguration()
        {
            if (_dataAccess.OpcTags.LINAC_ENERGY_TYPE_STATE.Value == LINAC_ENERGY_TYPE_VALUE.Dual)
            {
                _setLinacEnergyType = LINAC_ENERGY_TYPE_VALUE.High;
                _setApcsEnergyMode = ScanEnergyMode.High;
                _setPulseWidth = PulseWidth.PulseWidth2;
            }
            else if (_dataAccess.OpcTags.LINAC_ENERGY_TYPE_STATE.Value == LINAC_ENERGY_TYPE_VALUE.High)
            {
                _setLinacEnergyType = LINAC_ENERGY_TYPE_VALUE.Low;
                _setApcsEnergyMode = ScanEnergyMode.Low;
                _setPulseWidth = PulseWidth.PulseWidth3;
            }
#if LinacLowDose
            else if (_dataAccess.OpcTags.LINAC_ENERGY_TYPE_STATE.Value == LINAC_ENERGY_TYPE_VALUE.Low)
            {
                _setLinacEnergyType = LINAC_ENERGY_TYPE_VALUE.LowDose;
                _setApcsEnergyMode = ScanEnergyMode.LowDose;
                _setPulseWidth = PulseWidth.PulseWidth3;
            }
#endif
            else
            {
                SaveDataToPXE();
                _dataAccess.SetCalibrationState(CALIBRATION_STATE_VALUE.Completed);
                _calibrationLoaded = true;
                _logger.LogInfo("****   Stopping Calibration");
                StopCalibration();
            }
        }

        private void SaveDataToPXE()
        {
            _logger.LogInfo("Calibration saving data to pxe...");

            PxeWriteAccess pxeWriteAccess = new PxeWriteAccess();

            if (!Directory.Exists(_calibrationStorageLocation))
            {
                Directory.CreateDirectory(_calibrationStorageLocation);
            }

            //foreach (PulseWidth pulseWidth in Enum.GetValues(typeof(PulseWidth)))
            //{
                foreach (LINAC_ENERGY_TYPE_VALUE energy in Enum.GetValues(typeof(LINAC_ENERGY_TYPE_VALUE)))
                {
                    try
                    {
                        if (energy == LINAC_ENERGY_TYPE_VALUE.LowDose)
                            continue;

                        PulseWidth pulseWidth;

                        switch(energy)
                        {
                            case LINAC_ENERGY_TYPE_VALUE.Dual:
                                pulseWidth = PulseWidth.PulseWidth1;
                                break;
                            case LINAC_ENERGY_TYPE_VALUE.High:
                                pulseWidth = PulseWidth.PulseWidth2;
                                break;
                            case LINAC_ENERGY_TYPE_VALUE.Low:
                                pulseWidth = PulseWidth.PulseWidth3;
                                break;
#if LinacLowDose
                            case LINAC_ENERGY_TYPE_VALUE.LowDose:
                                pulseWidth = PulseWidth.PulseWidth3;
                                break;
#endif
                            default:
                                pulseWidth = PulseWidth.PulseWidth1;
                                break;
                        }

                        string pxeFile = Path.Combine(_calibrationStorageLocation, "Calibration_" + energy.ToString() + "_" + pulseWidth.ToString() + ".pxe");

                        if (File.Exists(pxeFile))
                        {
                            File.Delete(pxeFile);
                        }

                        pxeWriteAccess.CreatePXE(pxeFile);

                        if (energy == LINAC_ENERGY_TYPE_VALUE.Dual || energy == LINAC_ENERGY_TYPE_VALUE.High)
                        {
                            pxeWriteAccess.CreateHiPXEHeader(1, (uint)GetCalibrationCollection(energy, pulseWidth).GetScaleFactor(XRayEnergyEnum.HighEnergy).Length);
                            pxeWriteAccess.WriteHighEngDarkSample(PixelConverter.Convert(GetCalibrationCollection(energy, pulseWidth).GetDarkData(XRayEnergyEnum.HighEnergy)));
                            pxeWriteAccess.WriteHighEngAirSample(PixelConverter.Convert(GetCalibrationCollection(energy, pulseWidth).GetAirData(XRayEnergyEnum.HighEnergy)));
                        }

                        if (energy == LINAC_ENERGY_TYPE_VALUE.Dual
                                || energy == LINAC_ENERGY_TYPE_VALUE.Low
#if LinacLowDose
                                || energy == LINAC_ENERGY_TYPE_VALUE.LowDose
#endif
                            )
                        {
                            pxeWriteAccess.CreateLoPXEHeader(1, (uint)GetCalibrationCollection(energy, pulseWidth).GetScaleFactor(XRayEnergyEnum.LowEnergy).Length);
                            pxeWriteAccess.WriteLowEngDarkSample(PixelConverter.Convert(GetCalibrationCollection(energy, pulseWidth).GetDarkData(XRayEnergyEnum.LowEnergy)));
                            pxeWriteAccess.WriteLowEngAirSample(PixelConverter.Convert(GetCalibrationCollection(energy, pulseWidth).GetAirData(XRayEnergyEnum.LowEnergy)));
                        }

                        pxeWriteAccess.ClosePXEWrite();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex);
                    }
                //}
            }

            _logger.LogInfo("Calibration saving data to pxe done.");
        }

        private void FailedCalibration(Object parameter)
        {
            _dataAccess.SetCalibrationState(CALIBRATION_STATE_VALUE.Failed);
            _logger.LogError("Calibration Failed: " + parameter.ToString());
            StopCalibration();
            LoadStoredData();
        }

        private void SetXrayOn(bool enable)
        {
            _linacStateTagUpdated.Reset();

            if ((enable && _dataAccess.OpcTags.LINAC_STATE.Value != LINAC_STATE_VALUE.XRaysOn) ||
                (!enable && _dataAccess.OpcTags.LINAC_STATE.Value != LINAC_STATE_VALUE.XraysOff))
            {
                _logger.LogInfo("Set Xrays On " + enable.ToString());
                _dataAccess.EnableXray(enable);

                if (_xrayOnTimer == null)
                {
                    _xrayOnTimer = new Timer(new TimerCallback(FailedCalibration), "Xray On unable to be set.", _defaultTimerTimeout, Timeout.Infinite);
                }

                if (!_linacStateTagUpdated.WaitOne(_defaultTimerTimeout, false))
                {
                    FailedCalibration("Linac State unable to be set.");
                }
                else
                {
                    _linacStateTagUpdated.Reset();
                }
            }
        }

        private void SetLinacEnergyMode(LINAC_ENERGY_TYPE_VALUE energyType)
        {
            _linacEnergyTagUpdated.Reset();

            if (_dataAccess.OpcTags.LINAC_ENERGY_TYPE_STATE.Value != energyType)
            {
                _dataAccess.SetEnergyType(energyType);

                if (!_linacEnergyTagUpdated.WaitOne(_defaultTimerTimeout, false))
                {
                    FailedCalibration("Linac Energy Type unable to be set.");
                }
                else
                {
                    _linacEnergyTagUpdated.Reset();
                }
            }
        }

        private void SetApcsEnergyMode(ScanEnergyMode energyMode)
        {
            Thread.Sleep(1000);
            try
            {
                _dataAccess.Apcs.SetScanEnergyMode(energyMode, true);
            }
            catch (ThreadAbortException) { /*suppress*/ }
            catch (Exception ex)
            {
                FailedCalibration("APCS Energy Mode unable to be set.\n" + ex.ToString());
            }
        }

        private void SetApcsPulseWidth(PulseWidth width)
        {
            Thread.Sleep(1000);
            try
            {
                _dataAccess.Apcs.SetCurrentPulseWidth(width, false);
            }
            catch (Exception ex)
            {
                FailedCalibration("APCS Pulse Width unable to be set.\n" + ex.ToString());
            }
        }

        private void GetApcsStatus()
        {
            while (!_dataAccess.Apcs.Connected)
            {
                Thread.Sleep(1000);
            }

            while (_dataAccess.Apcs.Connected)
            {
                try
                {
                    bool success = false;

                    ScanEnergyMode? energyMode = null; 
                    PulseWidth? pulseWidth = null;

                    Thread.Sleep(500);
                    success = _dataAccess.Apcs.GetScanEnergyMode(out energyMode, false);
                    Thread.Sleep(500);
                    success = _dataAccess.Apcs.GetCurrentPulseWidth(out pulseWidth, false);

                    _currentApcsEnergyMode = (ScanEnergyMode) energyMode;
                    _currentPulseWidth =  (PulseWidth) pulseWidth;

                    //_currentApcsEnergyMode = _dataAccess.Apcs.GetScanEnergyMode();
                    //_currentPulseWidth = _dataAccess.Apcs.GetCurrentPulseWidth();
                    break;
                }
                catch { }
            }
        }

        private void LoadStoredData()
        {
            PxeWriteAccess pxeWriteAccess = new PxeWriteAccess();

            LINAC_ENERGY_TYPE_VALUE energy;
            PulseWidth pulseWidth;

            try 
            {
                string fileName;

                energy = LINAC_ENERGY_TYPE_VALUE.Dual;
                pulseWidth = PulseWidth.PulseWidth1;
                
                fileName = Path.Combine(_calibrationStorageLocation, "Calibration_" + energy.ToString() + "_" + pulseWidth.ToString() + ".pxe");

                if (File.Exists(fileName) && pxeWriteAccess.OpenPXEFile(fileName) == true)
                {
                    ReadStoredEnergyData(pxeWriteAccess, energy, pulseWidth);
                    pxeWriteAccess.ClosePXE();
                    _logger.LogInfo("Calibration: Read " + fileName + " successfully.");
                }
                else
                {
                    _logger.LogError("Calibration: reading of " + fileName + " failed");
                    _allCalibrationDataFound = false;
                }

                energy = LINAC_ENERGY_TYPE_VALUE.High;
                pulseWidth = PulseWidth.PulseWidth2;

                fileName = Path.Combine(_calibrationStorageLocation, "Calibration_" + energy.ToString() + "_" + pulseWidth.ToString() + ".pxe");

                if (File.Exists(fileName) && pxeWriteAccess.OpenPXEFile(fileName) == true)
                {
                    ReadStoredEnergyData(pxeWriteAccess, energy, pulseWidth);
                    pxeWriteAccess.ClosePXE();
                    _logger.LogInfo("Calibration: Read " + fileName + " successfully.");
                }
                else
                {
                    _logger.LogError("Calibration: reading of " + fileName + " failed");
                    _allCalibrationDataFound = false;
                }

                energy = LINAC_ENERGY_TYPE_VALUE.Low;
                pulseWidth = PulseWidth.PulseWidth3;

                fileName = Path.Combine(_calibrationStorageLocation, "Calibration_" + energy.ToString() + "_" + pulseWidth.ToString() + ".pxe");

                if (File.Exists(fileName) && pxeWriteAccess.OpenPXEFile(fileName) == true)
                {
                    ReadStoredEnergyData(pxeWriteAccess, energy, pulseWidth);
                    pxeWriteAccess.ClosePXE();
                    _logger.LogInfo("Calibration: Read " + fileName + " successfully.");
                }
                else
                {
                    _logger.LogError("Calibration: reading of " + fileName + " failed");
                    _allCalibrationDataFound = false;
                }

#if LinacLowDose
                energy = LINAC_ENERGY_TYPE_VALUE.LowDose;
                pulseWidth = PulseWidth.PulseWidth3;

                fileName = Path.Combine(_calibrationStorageLocation, "Calibration_" + energy.ToString() + "_" + pulseWidth.ToString() + ".pxe");

                if (File.Exists(fileName) && pxeWriteAccess.OpenPXEFile(fileName) == true)
                {
                    ReadStoredEnergyData(pxeWriteAccess, energy, pulseWidth);
                    pxeWriteAccess.ClosePXE();
                    _logger.LogInfo("Calibration: Read " + fileName + " successfully.");
                }
                else
                {
                    _logger.LogError("Calibration: reading of " + fileName + " failed");
                    _allCalibrationDataFound = false;
                }
#endif

                _allCalibrationDataFound = true;
            }
            catch (Exception e)
            {
                _logger.LogError("Exception during reading Calibration files: " + e.Message);
                _allCalibrationDataFound = false;
            }


            /*
            //foreach (PulseWidth pulseWidth in Enum.GetValues(typeof(PulseWidth)))
            //{
            int index = 1;

            foreach (LINAC_ENERGY_TYPE_VALUE energy in Enum.GetValues(typeof(LINAC_ENERGY_TYPE_VALUE)))
            {
                PulseWidth pulseWidth = (PulseWidth)(index - 1);

                string fileName = Path.Combine(_calibrationStorageLocation, "Calibration_" + energy.ToString() + "_" + pulseWidth.ToString() + ".pxe");

                if (File.Exists(fileName) && pxeWriteAccess.OpenPXEFile(fileName) == true)
                {
                    ReadStoredEnergyData(pxeWriteAccess, energy, pulseWidth);
                    pxeWriteAccess.ClosePXE();
                }
                else
                {
                    _logger.LogError("Calibration: reading of " + fileName + " failed");
                    _allCalibrationDataFound = false;
                }

                index++;
            }
            }
            */

            LoadBadDetectorsInfo();
            _calibrationLoaded = true;
        }

        private void ReadStoredEnergyData(PxeWriteAccess pxeWriteAccess, LINAC_ENERGY_TYPE_VALUE energy, PulseWidth pulseWidth)
        {
            long bufferSize = _dataAccess.Detectors.PixelsPerColumn;
            CalibrationDataCollection collection = GetCalibrationCollection(energy, pulseWidth);
            if (energy == LINAC_ENERGY_TYPE_VALUE.High)
            {
                float[] airHighDataBuffer = new float[bufferSize];
                float[] darkHighDataBuffer = new float[bufferSize];
                float[] normScaleHighDataBuffer = new float[bufferSize];
                if (pxeWriteAccess.ReadHighEngDarkSample(darkHighDataBuffer))
                {
                    collection.AddDarkData(XRayEnergyEnum.HighEnergy, PixelConverter.Convert(darkHighDataBuffer));
                }

                if (pxeWriteAccess.ReadHighEngAirSample(airHighDataBuffer))
                {
                    collection.AddAirData(XRayEnergyEnum.HighEnergy, PixelConverter.Convert(airHighDataBuffer));
                }
            }
            if (energy == LINAC_ENERGY_TYPE_VALUE.Low)
            {
                float[] airLowDataBuffer = new float[bufferSize];
                float[] normScaleLowDataBuffer = new float[bufferSize];
                float[] darkLowDataBuffer = new float[bufferSize];
                if (pxeWriteAccess.ReadLowEngDarkSample(darkLowDataBuffer))
                {
                    collection.AddDarkData(XRayEnergyEnum.LowEnergy, PixelConverter.Convert(darkLowDataBuffer));
                }

                if (pxeWriteAccess.ReadLowEngAirSample(airLowDataBuffer))
                {
                    collection.AddAirData(XRayEnergyEnum.LowEnergy, PixelConverter.Convert(airLowDataBuffer));
                }
            }
            if (energy == LINAC_ENERGY_TYPE_VALUE.Dual)
            {
                float[] airHighDataBuffer = new float[bufferSize];
                float[] darkHighDataBuffer = new float[bufferSize];
                float[] normScaleHighDataBuffer = new float[bufferSize];
                float[] airLowDataBuffer = new float[bufferSize];
                float[] normScaleLowDataBuffer = new float[bufferSize];
                float[] darkLowDataBuffer = new float[bufferSize];
                if (pxeWriteAccess.ReadHighEngDarkSample(darkHighDataBuffer))
                {
                    collection.AddDarkData(XRayEnergyEnum.HighEnergy, PixelConverter.Convert(darkHighDataBuffer));
                }

                if (pxeWriteAccess.ReadHighEngAirSample(airHighDataBuffer))
                {
                    collection.AddAirData(XRayEnergyEnum.HighEnergy, PixelConverter.Convert(airHighDataBuffer));
                }

                if (pxeWriteAccess.ReadLowEngDarkSample(darkLowDataBuffer))
                {
                    collection.AddDarkData(XRayEnergyEnum.LowEnergy, PixelConverter.Convert(darkLowDataBuffer));
                }

                if (pxeWriteAccess.ReadLowEngAirSample(airLowDataBuffer))
                {
                    collection.AddAirData(XRayEnergyEnum.LowEnergy, PixelConverter.Convert(airLowDataBuffer));
                }
            }
        }

        private void DetectBadDetectors()
        {
            _logger.LogInfo("Calibration calculating bad detectors...");

            for (int detectorNum = 0; detectorNum < _dataAccess.Detectors.PixelsPerColumn; detectorNum++)
            {
                //select high evergy data at the highest pulse width for bad detector detection
                if (_HighDataCollection[PulseWidth.PulseWidth2].GetScaleFactor(XRayEnergyEnum.HighEnergy)[detectorNum] <= 0.5 ||
                    _HighDataCollection[PulseWidth.PulseWidth2].GetScaleFactor(XRayEnergyEnum.HighEnergy)[detectorNum] > 13.0)
                {
                    //mark this detector as bad
                    _badDetectorsList.Add(detectorNum);

                    //if this is the very first detector and it is out of bounds then set its value to one
                    //other bad detectors value will get pervious detectors value.
                    if (detectorNum > 0)
                    {
                        //change all buffers values
                        //ScaleFactor
                        _DualDataCollection[PulseWidth.PulseWidth1].GetScaleFactor(XRayEnergyEnum.HighEnergy)[detectorNum] =
                            _DualDataCollection[PulseWidth.PulseWidth1].GetScaleFactor(XRayEnergyEnum.HighEnergy)[detectorNum - 1];

                        _DualDataCollection[PulseWidth.PulseWidth1].GetScaleFactor(XRayEnergyEnum.LowEnergy)[detectorNum] =
                            _DualDataCollection[PulseWidth.PulseWidth1].GetScaleFactor(XRayEnergyEnum.LowEnergy)[detectorNum - 1];

                        _HighDataCollection[PulseWidth.PulseWidth2].GetScaleFactor(XRayEnergyEnum.HighEnergy)[detectorNum] =
                            _HighDataCollection[PulseWidth.PulseWidth2].GetScaleFactor(XRayEnergyEnum.HighEnergy)[detectorNum - 1];

                        _LowDataCollection[PulseWidth.PulseWidth3].GetScaleFactor(XRayEnergyEnum.LowEnergy)[detectorNum] =
                            _LowDataCollection[PulseWidth.PulseWidth3].GetScaleFactor(XRayEnergyEnum.LowEnergy)[detectorNum - 1];
#if LinacLowDose
                        _LowDoseDataCollection[PulseWidth.PulseWidth3].GetScaleFactor(XRayEnergyEnum.LowEnergy)[detectorNum] =
                            _LowDoseDataCollection[PulseWidth.PulseWidth3].GetScaleFactor(XRayEnergyEnum.LowEnergy)[detectorNum - 1];
#endif

                        //AirData
                        _DualDataCollection[PulseWidth.PulseWidth1].GetAirData(XRayEnergyEnum.HighEnergy)[detectorNum] =
                            _DualDataCollection[PulseWidth.PulseWidth1].GetAirData(XRayEnergyEnum.HighEnergy)[detectorNum - 1];

                        _DualDataCollection[PulseWidth.PulseWidth1].GetAirData(XRayEnergyEnum.LowEnergy)[detectorNum] =
                            _DualDataCollection[PulseWidth.PulseWidth1].GetAirData(XRayEnergyEnum.LowEnergy)[detectorNum - 1];

                        _HighDataCollection[PulseWidth.PulseWidth2].GetAirData(XRayEnergyEnum.HighEnergy)[detectorNum] =
                            _HighDataCollection[PulseWidth.PulseWidth2].GetAirData(XRayEnergyEnum.HighEnergy)[detectorNum - 1];

                        _LowDataCollection[PulseWidth.PulseWidth3].GetAirData(XRayEnergyEnum.LowEnergy)[detectorNum] =
                            _LowDataCollection[PulseWidth.PulseWidth3].GetAirData(XRayEnergyEnum.LowEnergy)[detectorNum - 1];

#if LinacLowDose
                        _LowDoseDataCollection[PulseWidth.PulseWidth3].GetAirData(XRayEnergyEnum.LowEnergy)[detectorNum] =
                            _LowDoseDataCollection[PulseWidth.PulseWidth3].GetAirData(XRayEnergyEnum.LowEnergy)[detectorNum - 1];
#endif
                    }
                    else
                    {
                        _DualDataCollection[PulseWidth.PulseWidth1].GetScaleFactor(XRayEnergyEnum.HighEnergy)[detectorNum] = 1.0F;
                        _DualDataCollection[PulseWidth.PulseWidth1].GetScaleFactor(XRayEnergyEnum.LowEnergy)[detectorNum] = 1.0F;
                        _HighDataCollection[PulseWidth.PulseWidth2].GetScaleFactor(XRayEnergyEnum.HighEnergy)[detectorNum] = 1.0F;
#if LinacLowDose
                        _LowDataCollection[PulseWidth.PulseWidth3].GetScaleFactor(XRayEnergyEnum.LowEnergy)[detectorNum] = 1.0F;
                        _LowDoseDataCollection[PulseWidth.PulseWidth3].GetScaleFactor(XRayEnergyEnum.LowEnergy)[detectorNum] = 1.0F;
#endif
                    }
                }
            }

            UpdateDetectorsStatus();
        }

        private void LoadBadDetectorsInfo()
        {
            try
            {
                StreamReader stream = new StreamReader("BadDetectors.txt");
                try
                {
                    while (!stream.EndOfStream)
                        _badDetectorsList.Add(int.Parse(stream.ReadLine()));
                }
                catch { }
                finally { try { stream.Close(); } catch { } }
            }
            catch { }
            UpdateDetectorsStatus();
        }

        private void UpdateDetectorsStatus()
        {
            if (_badDetectorsList.Count > 0)
            {
                int contiguousNumBadDetectors = 0;
                int previousBadDetector = _badDetectorsList[0];
                foreach (int badDetector in _badDetectorsList)
                {
                    if (previousBadDetector == (badDetector - 1))
                        contiguousNumBadDetectors++;
                    else
                        contiguousNumBadDetectors = 0;
                    previousBadDetector = badDetector;
                }
                if (contiguousNumBadDetectors > AppConfiguration.MaxNumContiguousBadDetectors)
                    _contiguousNumBadDetectorsFound = true;
            }

            if (_contiguousNumBadDetectorsFound)
            {
                _statusMgr.DetectorsUpdateStatus(DetectorsStatus.Error);
            }
            else if (_badDetectorsList.Count >= _warningMaxNumBadDetectors &&
                _badDetectorsList.Count < _errorMaxNumBadDetectors)
            {
                _statusMgr.DetectorsUpdateStatus(DetectorsStatus.Warning);
            }
            else if (_badDetectorsList.Count >= _errorMaxNumBadDetectors)
            {
                _statusMgr.DetectorsUpdateStatus(DetectorsStatus.Error);
            }
            else if (!_allCalibrationDataFound)
            {
                _statusMgr.DetectorsUpdateStatus(DetectorsStatus.CalibDataNotFoundWarning);
            }
            else
            {
                _statusMgr.DetectorsUpdateStatus(DetectorsStatus.Clear);
            }

            if (_badDetectorsList.Count > 0)
            {
                foreach (int detector in _badDetectorsList)
                {
                    _statusMgr.BadDetectorsInfo(detector);
                }
            }

            _numBadDetectors = _badDetectorsList.Count;
        }

        private CalibrationDataCollection GetCalibrationCollection(LINAC_ENERGY_TYPE_VALUE energyConfig, PulseWidth pulseWidth)
        {
            switch (energyConfig)
            {
                case LINAC_ENERGY_TYPE_VALUE.Dual:
                    return _DualDataCollection[PulseWidth.PulseWidth1];
                case LINAC_ENERGY_TYPE_VALUE.High:
                    return _HighDataCollection[PulseWidth.PulseWidth2];
                case LINAC_ENERGY_TYPE_VALUE.Low:
                    return _LowDataCollection[PulseWidth.PulseWidth3];
#if LinacLowDose
                case LINAC_ENERGY_TYPE_VALUE.LowDose:
                    return _LowDoseDataCollection[PulseWidth.PulseWidth3];
#endif
                default:
                    return _DualDataCollection[PulseWidth.PulseWidth1];
            }
            //return (energyConfig == LINAC_ENERGY_TYPE_VALUE.Dual) ? _DualDataCollection[PulseWidth.PulseWidth1] :
            //       (energyConfig == LINAC_ENERGY_TYPE_VALUE.High) ? _HighDataCollection[PulseWidth.PulseWidth2] : _LowDataCollection[PulseWidth.PulseWidth3];
        }
    }
}
