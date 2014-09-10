using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using L3.Cargo.Communications.Detectors.Common;
using L3.Cargo.Communications.Detectors.Host;
using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Detectors.Common;
using L3.Cargo.Detectors.DataAccessCore;

namespace L3.Cargo.Detectors.BusinessCore
{
    public class RealTimeViewer: IDisposable
    {
        #region Private Members

        private EventLoggerAccess _log;

        private DetectorsDataAccess _dataAccess;

        private Thread _sendDataThread;


        private RealTimeViewerHost _realTimeViewerHost;


        private uint _realTimeViewerBytesPerPixel;

        private uint _realTimeViewerPixelsPerColumn;

        private int _pixelInterval;

        private BlockingCollection<DataInfo> _inComingDataColl;

        private string _masterControlAddress;

        private CancellationTokenSource _cancellationtokenSource;

        #endregion


        #region Constructors

        public RealTimeViewer(EventLoggerAccess log, DetectorsDataAccess dataAccess)
        {
            _log = log;
            _dataAccess = dataAccess;
            _dataAccess.DisplayControlUpdateRequest += new Subsystem.DataAccessCore.DashboardControlUpdateHandler(_dataAccess_DisplayControlUpdateRequest);

            int dataPort = int.Parse(ConfigurationManager.AppSettings["RealTimeViewerDataPort"]);
            string address = ConfigurationManager.AppSettings["RealTimeViewerMulticastIPAddress"];
            _realTimeViewerBytesPerPixel = uint.Parse(ConfigurationManager.AppSettings["RealTimeViewerBytesPerPixel"]);
            _realTimeViewerPixelsPerColumn = uint.Parse(ConfigurationManager.AppSettings["RealTimeViewerPixelsPerColumn"]);

            _masterControlAddress = string.Empty;

            if ((dataAccess.Detectors.PixelPerColumn > _realTimeViewerPixelsPerColumn) &&
                (_realTimeViewerPixelsPerColumn != 0))
            {
                float value = (float)dataAccess.Detectors.PixelPerColumn / (float)_realTimeViewerPixelsPerColumn;
                _pixelInterval = (int)(Math.Round(value, 0, MidpointRounding.ToEven));
            }
            else
                _pixelInterval = 1;

            try
            {
                _realTimeViewerHost = new RealTimeViewerHost(address, dataPort);
            }
            catch { }

            _inComingDataColl = new BlockingCollection<DataInfo>(AppConfiguration.DataCollectionMaxSize);
        }

        #endregion Constructors


        #region Private Methods

        void _dataAccess_DisplayControlUpdateRequest(string name, int value)
        {
            if (name == "SEND_REALTIME_VIEW_DATA")
            {
                //there can only be one master controller commanding to start and stop sending data.
                //get ipAddress
                RemoteEndpointMessageProperty clientEndpoint =
                OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;

                if (value == 1)
                {
                    if (string.IsNullOrWhiteSpace(_masterControlAddress))
                    {
                        _masterControlAddress = clientEndpoint.Address;
                        //StartSendingData();
                    }
                }
                else
                {
                    if (_masterControlAddress == clientEndpoint.Address)
                    {
                        //StopSendingData();
                        _masterControlAddress = string.Empty;
                    }
                }
            }
        }

        private void SendDataThreadMethod(object param)
        {
            //send resoultion information first
            _dataAccess.UpdateWidgets("REALTIME_VIEWER_RESOLUTION", (int)
                ((_realTimeViewerPixelsPerColumn << 16) | _realTimeViewerBytesPerPixel));

            int scaleValueBits = (int) ((AppConfiguration.BytesPerPixel - _realTimeViewerBytesPerPixel) * 8);

            CancellationToken token = (CancellationToken)param;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    DataInfo dataInfo = _inComingDataColl.Take(token);

                    //if (_dataAccess.Detectors.PixelPerColumn != _realTimeViewerPixelsPerColumn)
                    if(AppConfiguration.BytesPerPixel != _realTimeViewerBytesPerPixel)
                    {
                        //downscale data to reduce bandwidth
                        byte[] downscaledData = new byte[_realTimeViewerPixelsPerColumn * _realTimeViewerBytesPerPixel];
                        int downscaledDataIndex = 0;

                        for (int index = 0; index < dataInfo.LineData.Length; index++)
                        {
                            if ((index % _pixelInterval) == 0)
                            {
                                uint[] value = new uint[1];
                                value[0] = dataInfo.LineData[index].Value >> scaleValueBits;
                                Buffer.BlockCopy(value, 0, downscaledData, downscaledDataIndex, (int)_realTimeViewerBytesPerPixel);
                                downscaledDataIndex += (int)_realTimeViewerBytesPerPixel;
                            }
                        }

                        _realTimeViewerHost.SendData(downscaledData);
                    }
                    else
                    {
                        _realTimeViewerHost.SendData(dataInfo.LineData, (uint)AppConfiguration.BytesPerPixel);
                    }
                }
                catch (Exception exp)
                {
                    if (!token.IsCancellationRequested)
                    {
                        _log.LogError(exp);
                    }
                }
            }
        }

        #endregion


        #region Public Methods

        public void AddDataLine(ref DataInfo dataInfo)
        {
            _inComingDataColl.TryAdd(dataInfo, AppConfiguration.DataLineProcessTimeout);
        }

        public void Dispose()
        {
            _cancellationtokenSource.Cancel();

            if (_sendDataThread != null)
            {
                if (_sendDataThread.IsAlive)
                {
                    _sendDataThread.Abort();
                    _sendDataThread.Join();
                }

                _sendDataThread = null;
            }
        }

        #endregion Public Methods
    }
}
