using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using L3.Cargo.Communications.DetectorPlot.Client;
using L3.Cargo.Communications.DetectorPlot.Interface;
using System.ServiceModel.Description;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using L3.Cargo.Communications.DetectorPlot.Common;

namespace L3.Cargo.DetectorPlot
{
    [CallbackBehavior(UseSynchronizationContext = false)]
    public class WCFComm : IDetectorPlotCallback, IDisposable
    {
        DetectorPlotEndpoint dpe;
        public int PixelsPerColumn = 0;//80;
        public int BytesPerPixel = 0;//3;

        private DetectorPlotDataAccess _diplotDataAcess;

        private Thread _processDataThread = null;
        DataLines lineArray;
        AutoResetEvent resolutionReceivedEvent = new AutoResetEvent(false);

        private bool _shutdown = false;

        private bool _isDataSourceConnected;

        private string _multicastAddr = "230.0.0.1";
        private int    _dataPort = 3333;

        private InstanceContext _sourceCallback = null;
        private ServiceEndpoint _hostEndPoint = null;


        public void DataSourceConnected(bool isConnected)
        {
            _isDataSourceConnected = isConnected;

            if (isConnected)
            {
                DASConfig config = dpe.GetConfig();

                PixelsPerColumn = config.PixelsPerColumn;
                BytesPerPixel = config.BytesPerPixel;

                //start thread
                if (_processDataThread == null)
                {
                    _processDataThread = new Thread(new ThreadStart(ProcessDataThreadMethod));
                    _processDataThread.Start();
                }

            }
            else
            {
                //stop data acq thread
                if (_processDataThread != null)
                {
                    _processDataThread.Abort();
                    _diplotDataAcess.Close();
                    _processDataThread.Join();
                    _processDataThread = null;
                }
            }
        }      

        public WCFComm(ref DataLines lineData)
        {
            lineArray = lineData;
            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Mode = SecurityMode.None; 
            EndpointAddress address = new EndpointAddress("net.tcp://localhost:47999/DiPlotComm/");

            _isDataSourceConnected = false;

            _sourceCallback = new InstanceContext(this);

            _hostEndPoint = new ServiceEndpoint(ContractDescription.GetContract(typeof(IDiPlot)),
                                                        binding,
                                                        address);
        }

        public void StartAcq()
        {
            try
            {

                dpe = new DiPlotEndpoint(_sourceCallback, _hostEndPoint);
            }
            catch { }

            try
            {
                _diplotDataAcess = new DiPlotDataAccess(_multicastAddr, _dataPort);
            }
            catch { }

            if (dpe.IsDataSourceConnected())
            {
                DASConfig cfg = dpe.GetConfig();

                BytesPerPixel = cfg.BytesPerPixel;
                PixelsPerColumn = cfg.PixelsPerColumn;

                if (_processDataThread == null)
                {
                    _processDataThread = new Thread(new ThreadStart(ProcessDataThreadMethod));
                    _processDataThread.Start();
                }

                dpe.SendData(true);
            }
            else
            {
                throw new Exception("Source not connected");
            }
        }

        public void StopAcq()
        {
            dpe.SendData(false);
        }

        public void ProcessDataThreadMethod()
        {
            try
            {
                // Enter the listening loop.
                while (!_shutdown)
                {
                    byte[] receivedData;
                    int lineSize = PixelsPerColumn * BytesPerPixel;
                    receivedData = _diplotDataAcess.ReceiveDataLines(lineSize);

                    if (receivedData == null)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    LineData ld = new LineData();
                    ld.data = receivedData;
                    lineArray.Add(ld);                    
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                
            }


            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }

        public void Dispose()
        {
            try
            {
                StopAcq();

                _shutdown = true;

                _diplotDataAcess.Dispose();

                if (_processDataThread != null)
                {
                    _processDataThread.Abort();
                    _processDataThread.Join();
                    _processDataThread = null;
                }

                dpe.Dispose();
                dpe = null;
            }
            catch
            {
            }
        }
    }
}
