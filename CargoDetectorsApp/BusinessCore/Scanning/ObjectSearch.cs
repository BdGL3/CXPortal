using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.Detectors.Common;
using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Detectors.Common;
using L3.Cargo.Detectors.DataAccessCore;

namespace L3.Cargo.Detectors.BusinessCore
{
    public class ObjectSearch: IDisposable
    {
        #region Private Members

        private EventLoggerAccess _log;

        private DetectorsDataAccess _dataAccess;

        private ArchiveData _archiveData;

        private BlockingCollection<DataInfo> _inComingDataColl;

        private List<DataInfo> _ObjectLines;

        private bool _foundObject;

        private string _lastCaseId = null;

        // These are used to fake an object
        bool _useFakeObject;
        bool _fakeObjectFound;

        #endregion Private Members

        public ObjectSearch(EventLoggerAccess log, DetectorsDataAccess dataAccess, Calibration calibration)
        {
            _log = log;
            _dataAccess = dataAccess;
            _archiveData = new ArchiveData(log, calibration);

            _inComingDataColl = new BlockingCollection<DataInfo>();

            _searchThread = Threads.Create(SearchAgent, ref _searchEnd, "Object Search thread");
            _searchThread.Start();

            _foundObject = false;
        }

        #region Private Methods

        private bool IsObjectLine(DataInfo dataInfo)
        {
            if (this._useFakeObject)
            {
                return this._fakeObjectFound;
            }

            bool ret = false;
            int numObjPixels = 0;

            for (int index = AppConfiguration.SearchBeginDetectorNum; index < AppConfiguration.SearchEndDetectorNum; index++)
            {
                if (dataInfo.LineData[index].Value <= AppConfiguration.ObjectThreshold)
                {
                    if (++numObjPixels > AppConfiguration.SmallObjectSizeInPixels)
                    {
                        ret = true;
                        break;
                    }
                }
            }

            return ret;
        }

        private void SearchAgent()
        {
            _ObjectLines = new List<DataInfo>();
            int airLines = 0;
            int emptyScanLines = 0;
            while (!_searchEnd.WaitOne(0))
            {
                try
                {
                    DataInfo dataInfo = _inComingDataColl.Take(_searchCancel.Token);
                    if (_ObjectLines.Count == AppConfiguration.NumberOfLinesForStartOfObject)
                    {
                        _dataAccess.StartCaseManagerScan();
                        _ObjectLines.Add(dataInfo);

                        _log.LogInfo("Found Start of Object");
                        _foundObject = true;
                        emptyScanLines = 0;
                    }
                    else if (IsObjectLine(dataInfo))
                    {
                        _ObjectLines.Add(dataInfo);
                    }
                    else if (_ObjectLines.Count < AppConfiguration.NumberOfLinesForStartOfObject)
                    {
                        _ObjectLines.Clear();

                         if (++emptyScanLines > AppConfiguration.XrayOffLineThreshold && _foundObject)
                        {
                            _log.LogInfo("No object found for " + emptyScanLines + " lines, forcing STOP");
                            emptyScanLines = 0;
                            _dataAccess.StopScan();
                        }
                    }
                    else if (++airLines > AppConfiguration.NumberofLinesForEndOfObject)
                    {
                        DataComplete(true);
                        _log.LogInfo("Found End of Object");
                        airLines = 0;
                    }
                    else
                        _ObjectLines.Add(dataInfo);
                    _dataAccess.RealTimeViewer.AddDataLine(ref dataInfo);
                }
                catch (Exception ex)
                {
                    // this will re initialize the CORBA connection
                    _dataAccess.CargoHostEndPoint.RebindToNameService();
                    if (!_searchEnd.WaitOne(0))
                        _log.LogError(ex);
                }
            }
        }
        private CancellationTokenSource _searchCancel = new CancellationTokenSource();
        private ManualResetEvent _searchEnd = new ManualResetEvent(false);
        private Thread _searchThread;
        #endregion


        #region Public Methods

        public void AddDataLine(ref DataInfo dataInfo)
        {
            _inComingDataColl.TryAdd(dataInfo, AppConfiguration.DataLineProcessTimeout);
        }

        public void DataComplete(bool setFoundObjectFlag = false)
        {
            try
            {
                _foundObject = setFoundObjectFlag;

                if (_ObjectLines.Count >= AppConfiguration.NumberOfLinesForStartOfObject)
                {
                    if (_ObjectLines.Count > 100)
                    {
                        string currentCaseId = _dataAccess.GetCaseManagerScanCaseId();

                        if (currentCaseId != null && _lastCaseId != currentCaseId)
                        {
                            string pxeFile = _archiveData.CreatePXEFile(_dataAccess.OpcTags.LINAC_ENERGY_TYPE_STATE.Value, _dataAccess.Apcs.CurrentPulseWidth, _ObjectLines);
                            _dataAccess.StopCaseManagerScan(pxeFile);
                        }
                        else
                        {
                            _dataAccess.AbortCaseManagerScan();
                            _log.LogInfo("Last CaseId = " + _lastCaseId + ", currentCaseId = " + currentCaseId);
                        }

                        _lastCaseId = currentCaseId;
                    }
                    else
                    {
                        _log.LogInfo("Image too small - ignoring");
                        _ObjectLines.Clear();
                    }
                }
                else
                {
                    _dataAccess.AbortCaseManagerScan();
                }
            }
            catch (Exception ex)
            {
                if (!_searchEnd.WaitOne(0))
                    _log.LogError(ex);
            }
            finally { _ObjectLines.Clear(); }
        }

        public void Dispose()
        {
            _searchCancel.Cancel();
            try
            {
                if (/*exists (avoid first try exceptions)?*/ _searchThread != null)
                    _searchThread = Threads.Dispose(_searchThread, ref _searchEnd);
            }
            catch { }
            finally { _searchThread = null; }
        }

        public void FakeEndOfObject(bool useFakeObject, bool objectFound)
        {
            if (useFakeObject)
                _log.LogInfo("Fake object in beam? " + objectFound.ToString());
            _useFakeObject = useFakeObject;
            _fakeObjectFound = objectFound;
        }

        #endregion Public Methods
    }
}
