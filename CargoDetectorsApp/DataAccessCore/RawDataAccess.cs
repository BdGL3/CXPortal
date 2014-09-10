using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.DetectorPlot.Common;
using L3.Cargo.Communications.DetectorPlot.Host;
using L3.Cargo.Communications.Detectors.Client;
using L3.Cargo.Communications.Detectors.Common;
using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Detectors.Common;

namespace L3.Cargo.Detectors.DataAccessCore
{
    public class RawDataAccess : DetectorPlotHost, IDisposable
    {
        public void AddDataLine(ref DataInfo dataInfo) { IncomingData.TryAdd(dataInfo, AppConfiguration.DataLineProcessTimeout); }

        /// <summary>Class Name specifies the name of this class.</summary>
        public static string ClassName { get { return MethodBase.GetCurrentMethod().DeclaringType.Name; } }

        [DefaultValue(null)]
        public DetectorsAccess DetectorsAccessInstance { get; private set; }

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
                _sendRawDataCancel.Cancel();
                try
                {
                    if (/*exists (avoid first try exceptions)?*/ _sendRawDataThread != null)
                        _sendRawDataThread = Threads.Dispose(_sendRawDataThread, ref _sendRawDataEnd, Utilities.Time10SECONDS);
                }
                catch { }
                finally { _sendRawDataThread = null; }
                Logger.LogInfo("---" + ClassName + "---");
                Logger = null;
                Disposed = true;
            }
        }

        /// <summary>
        /// Disposed declares whether or not the class instance has been disposed.</summary>
        [DefaultValue(false)]
        public bool Disposed { get; private set; }

        [DefaultValue(XRayEnergyEnum.HighEnergy)]
        public XRayEnergyEnum EnergyType { get; private set; }

        public override DetectorConfig GetConfig()
        {
            return new DetectorConfig((int)DetectorsAccessInstance.GetNumberOfChannels(),   AppConfiguration.BytesPerPixel, AppConfiguration.DetectorsPerBoard);
        }

        public override void GetHighEnergyData() { EnergyType = XRayEnergyEnum.HighEnergy; }
        public override void GetLowEnergyData() { EnergyType = XRayEnergyEnum.LowEnergy; }

        [DefaultValue(null)]
        public BlockingCollection<DataInfo> IncomingData { get; private set; }

        public override bool IsDataSourceConnected() { return DetectorsAccessInstance.CommandIsReady; }

        [DefaultValue(null)]
        public EventLoggerAccess Logger { get; private set; }

        /// <summary>
        /// This is a safety net to ensure that resources are disposed even if
        /// <see cref="Dispose()"/> is not called.</summary>
        ~RawDataAccess() { Dispose(!Disposed); }

        public RawDataAccess(EventLoggerAccess eventLogger, DetectorsAccess detectorAccess) :
            base(AppConfiguration.DiplotConnectionUri, AppConfiguration.DiplotMulticastIPAddress, AppConfiguration.DiplotDataPort)
        {
            if (/*invalid?*/ eventLogger == null)
                throw new ArgumentNullException(ClassName + " logger reference (eventLogger) must not be null");
            Logger = eventLogger;
            if (/*invalid?*/ detectorAccess == null)
                throw new ArgumentNullException(ClassName + " detector access (detectorAccess) must not be null");
            DetectorsAccessInstance = detectorAccess;
            GetHighEnergyData();
            IncomingData = new BlockingCollection<DataInfo>();
            _sendRawDataThread = Threads.Create(SendRawDataAgent, ref _sendRawDataEnd, "Raw Data Access Send thread");
            Debug.Assert(!_sendRawDataEnd.WaitOne(0));
            _sendRawDataThread.Start();
        }

        private void SendRawDataAgent()
        {
            while (!_sendRawDataEnd.WaitOne(0))
            {
                try
                {
                    DataInfo dataInfo = IncomingData.Take(_sendRawDataCancel.Token);
                    if (dataInfo.XRayInfo.Energy == EnergyType)
                    {
                        int /*pixel count*/ count = dataInfo.LineData.Length;
                        if (AppConfiguration.DiplotRemoveReferenceData)
                            count -= AppConfiguration.DetectorsPerBoard * (AppConfiguration.ReferenceRangeUpperDetectorNum - AppConfiguration.ReferenceRangeLowerDetectorNum + 1); /*ignore reference pixels*/

                        // Detector data is bytesPerPixels size, not 4 bytes that uint contains.
                        byte[] buffer = new byte[dataInfo.NumberOfBytesPerPixel * count];
                        int bufferIndex = 0;
                        for (int ix = 0; ix < count; ix++)
                            PixelConverter.PixelToBytes(dataInfo.LineData, ix, ref buffer, ref bufferIndex, AppConfiguration.BytesPerPixel);
#if false
                        Parallel.For(0, numPixels, index =>
                        {
                            PixelConverter.PixelToBytes(dataInfo.LineData, index, ref buffer, ref bufferIndex, AppConfiguration.BytesPerPixel);
                        });
#endif
                        base.SendLineData(buffer);
                    }
                }
                catch (Exception ex)
                {
                    if (!_sendRawDataEnd.WaitOne(0))
                        Logger.LogError(ex);
                }
            }
        }
        private CancellationTokenSource _sendRawDataCancel = new CancellationTokenSource();
        private ManualResetEvent _sendRawDataEnd = new ManualResetEvent(false);
        private Thread _sendRawDataThread;
    }
}
