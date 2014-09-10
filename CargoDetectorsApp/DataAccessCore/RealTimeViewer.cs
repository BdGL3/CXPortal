using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.Detectors.Common;
using L3.Cargo.Communications.Detectors.Host;
using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Detectors.Common;

namespace L3.Cargo.Detectors.DataAccessCore
{
    public class RealTimeViewer: IDisposable
    {
        private EventLoggerAccess _log;
        private RealTimeViewerHost _realTimeViewerHost;
        private BlockingCollection<DataInfo> _inComingDataColl;

        #region Public Members

        public bool IsDualEnergy { get; set; }

        #endregion Public Members

        public RealTimeViewer(EventLoggerAccess log)
        {
            _log = log;
            IsDualEnergy = false;
            _realTimeViewerHost = new RealTimeViewerHost(AppConfiguration.RealTimeViewerMulticastIPAddress,
                                                         AppConfiguration.RealTimeViewerDataPort,
                                                         AppConfiguration.RealTimeViewerUdpClientPort,
                                                         log);

            _inComingDataColl = new BlockingCollection<DataInfo>();
            

            _sendThread = Threads.Create(SendAgent, ref _sendEnd, "Real Time View Data Send thread");
            _sendThread.Start();
        }

        private void SendAgent()
        {
            while (!_sendEnd.WaitOne(0))
            {
                try
                {
                    DataInfo dataInfo = _inComingDataColl.Take(_sendCancel.Token);
                    if ((IsDualEnergy && dataInfo.XRayInfo.Energy == XRayEnergyEnum.HighEnergy) || !IsDualEnergy)
                    {
                        byte[] scaledData = new byte[dataInfo.LineData.Length / AppConfiguration.RealTimeViewerPixelInterval];

                        Parallel.For(0, scaledData.Length, i =>
                        {
                            scaledData[i] = (byte)(dataInfo.LineData[i * AppConfiguration.RealTimeViewerPixelInterval].Value >> 8);
                        });

                        _realTimeViewerHost.SendData(scaledData);
                    }
                }
                catch (Exception exp)
                {
                    if (!_sendEnd.WaitOne(0))
                        _log.LogError(exp);
                }
            }
        }
        private CancellationTokenSource _sendCancel = new CancellationTokenSource();
        private ManualResetEvent _sendEnd = new ManualResetEvent(false);
        private Thread _sendThread;

        public void AddDataLine(ref DataInfo dataInfo)
        {
            _inComingDataColl.TryAdd(dataInfo, AppConfiguration.DataLineProcessTimeout);
        }

        public void Dispose()
        {
            _sendCancel.Cancel();
            try
            { 
                if (/*exists (avoid first try exceptions)?*/ _sendThread != null)
                    _sendThread = Threads.Dispose(_sendThread, ref _sendEnd, Utilities.Time10SECONDS);
            }
            catch { }
            finally { _sendThread = null; }
        }
    }
}
