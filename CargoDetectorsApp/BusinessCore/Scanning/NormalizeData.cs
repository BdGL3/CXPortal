using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.Detectors.Common;
using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Detectors.Common;
using L3.Cargo.Detectors.DataAccessCore;

namespace L3.Cargo.Detectors.BusinessCore
{
    public class NormalizeData: IDisposable
    {
        private EventLoggerAccess _log;
        private DetectorsDataAccess _dataAccess;
        private Calibration _calibration;
        private ObjectSearch _objectSearch;
        private BlockingCollection<DataInfo> _rawDataColl;

        public NormalizeData(EventLoggerAccess log, DetectorsDataAccess dataAccess, Calibration calibration)
        {
            _log = log;
            _dataAccess = dataAccess;
            _calibration = calibration;
            _objectSearch = new ObjectSearch(log, dataAccess, _calibration);
            _rawDataColl = new BlockingCollection<DataInfo>();

            _normalizeThread = Threads.Create(NormalizeAgent, ref _normalizeEnd, "Normalization thread");
            _normalizeThread.Start();
        }

        private double ReferenceFactor(Pixel[] data, Pixel[] airData, Pixel[]darkData)
        {
            double airValue = 0;
            double darkValue = 0;
            double dataValue = 0;

            for (int detectorNum = AppConfiguration.ReferenceRangeLowerDetectorNum; detectorNum <= AppConfiguration.ReferenceRangeUpperDetectorNum; detectorNum++)
            {
                airValue += airData[detectorNum].Value;
                darkValue += darkData[detectorNum].Value;
                dataValue += data[detectorNum].Value;
            }

            return Math.Min(Math.Max((airValue - darkValue) / (dataValue - darkValue), AppConfiguration.ReferenceScaleFactorLowerLimit), AppConfiguration.ReferenceScaleFactorUpperLimit);
        }

        private void NormalizeAgent()
        {            
            double MaxValue = Math.Pow(Byte.MaxValue + 1, AppConfiguration.BytesPerPixel) - 1.0;
            while (!_normalizeEnd.WaitOne(0))
            {
                try
                {
                    DataInfo dataInfo = _rawDataColl.Take(_normalizeCancel.Token);
                    if (AppConfiguration.NormalizeRawData)
                    {
                        Pixel[] airData = _calibration.GetAirData(dataInfo.XRayInfo);
                        Pixel[] darkData = _calibration.GetDarkData(dataInfo.XRayInfo);
                        float[] scaleFactorData = _calibration.GetScaleFactor(dataInfo.XRayInfo);

                        double refFactorValue = (AppConfiguration.EnableReferenceCorrection) ? ReferenceFactor(dataInfo.LineData, airData, darkData) : 1.0;
                        _calibration.AddReferenceCorrection(dataInfo.XRayInfo, refFactorValue);
                        int numReferenceDetectors = _dataAccess.Detectors.GetNumberReferencePixels();

                        // Normalize all of the data;
                        Parallel.For(0, dataInfo.LineData.Length, i =>
                        {
                            double tempValue = (Math.Max((double)dataInfo.LineData[i].Value - (double)darkData[i].Value, 0) * scaleFactorData[i] * refFactorValue);
                            dataInfo.LineData[i].Value = (uint)Math.Min(tempValue, MaxValue);
                        });

                        for (int i = 0; i < 32; i++)
                        {
                            if ( i < (AppConfiguration.ReferenceRangeLowerDetectorNum - 1) && i > (AppConfiguration.ReferenceRangeUpperDetectorNum + 1))
                                dataInfo.LineData[i].Value = (uint)AppConfiguration.NormConstant;
                        }

                        // Correct for bad detectors
                        if (AppConfiguration.CorrectForBadDetectors)
                        {
                            foreach (int index in _calibration.BadDetectorsList)
                            {
                                int correctedIndex = (index > 0) ? index - 1 : index + 1;
                                dataInfo.LineData[index].Value = dataInfo.LineData[correctedIndex].Value;
                            }
                        }
                    }

                    _objectSearch.AddDataLine(ref dataInfo);
                }
                catch (Exception ex)
                {
                    if (!_normalizeCancel.IsCancellationRequested)
                        _log.LogError(ex);
                }
            }
        }
        private CancellationTokenSource _normalizeCancel = new CancellationTokenSource();
        private ManualResetEvent _normalizeEnd = new ManualResetEvent(false);
        private Thread _normalizeThread;

        public void AddDataLine(DataInfo dataInfo)
        {
            _rawDataColl.TryAdd(dataInfo, AppConfiguration.DataLineProcessTimeout);
        }

        public void DataComplete()
        {
            _objectSearch.DataComplete();
        }

        public void Dispose()
        {
            _normalizeCancel.Cancel();
            try
            {
                if (/*exists (avoid first try exceptions)?*/ _normalizeThread != null)
                    _normalizeThread = Threads.Dispose(_normalizeThread, ref _normalizeEnd);
            }
            catch { }
            finally { _normalizeThread = null; }
            try
            {
                if (/*exists (avoid first try exceptions)?*/ _objectSearch != null)
                    _objectSearch.Dispose();
            }
            catch { }
            finally { _objectSearch = null; }
        }

        public void FakeEndOfObject(bool useFakeObject, bool objectFound)
        {
            _objectSearch.FakeEndOfObject(useFakeObject, objectFound);
        }
    }
}
