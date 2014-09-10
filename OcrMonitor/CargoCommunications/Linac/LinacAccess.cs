using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using L3.Cargo.Communications.EventsLogger.Client;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using L3.Cargo.Communications.Common;

namespace L3.Cargo.Communications.Linac
{
    public class LinacAccess : IDisposable
    {
        #region Private Members

        private EventLoggerAccess _logger;

        private TcpClient _tcpClient;

        private NetworkStream _networkStream;

        private Thread _dataAckThread;

        private string _address;

        private int _port;

        private bool _shutdown;

        private bool _commConnected;

        private bool _pingAckReceived;

        private Timer _pingTimer;

        private int _pingDueTimeMsec;

        private bool _flushNetworkStream;

        private AutoResetEvent _FlushingNetworkDoneEvent;

        private AutoResetEvent _commandResponseEvent;

        private List<LinacPacketFormat.DataPacket> _commandResponseList;

        #endregion


        #region Public Members

        public bool IsConnected
        {
            get
            {
                return _commConnected;
            }
            set
            {
                _commConnected = value;
                if (ConnectionStateChangeEvent != null)
                {
                    ConnectionStateChangeEvent(_commConnected);
                }
            }
        }

        public event ConnectionStateChangeHandler ConnectionStateChangeEvent;

        public delegate void ProcessCommandEventHandler(Dictionary<LinacPacketFormat.VariableIdEnum, object> commandList);

        public event ProcessCommandEventHandler ProcessCommandEvent;

        #endregion


        #region Constructor

        public LinacAccess(EventLoggerAccess log, string address, int port, int pingDuetimeMsec)
        {
            _shutdown = false;

            IsConnected = false;

            _flushNetworkStream = false;

            _pingDueTimeMsec = pingDuetimeMsec;

            _pingAckReceived = false;

            _logger = log;

            _address = address;
            _port = port;

            _commandResponseList = new List<LinacPacketFormat.DataPacket>();

            _FlushingNetworkDoneEvent = new AutoResetEvent(false);
            _commandResponseEvent = new AutoResetEvent(false);
        }

        #endregion


        #region Private Methods

        private void CheckConnection(object param)
        {
            //send ping command
        }

        private void DataAckThreadMethod()
        {
            int length = Marshal.SizeOf(typeof(LinacPacketFormat.CommandPacket));
            byte[] command = new byte[length];
            int dataRead = 0;

            while (!_shutdown)
            {
                while (!IsConnected && !_shutdown)
                {
                    try
                    {
                        _tcpClient = new TcpClient(_address, _port);

                        _pingTimer = new Timer(new TimerCallback(CheckConnection), null, _pingDueTimeMsec, _pingDueTimeMsec);

                        _networkStream = _tcpClient.GetStream();
                        _networkStream.ReadTimeout = -1;
                        IsConnected = true;
                        _logger.LogInfo("Connected to Linac.");

                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e);
                        Thread.Sleep(2000);
                    }
                }

                try
                {
                    while (!_shutdown && IsConnected)
                    {
                        try
                        {
                            //read command
                            dataRead = _networkStream.Read(command, 0, command.Length);

                            if (dataRead <= 0)
                            {
                                if (!_shutdown)
                                {
                                    _logger.LogInfo("Disconnected from Linac: Zero bytes read");
                                }
                                IsConnected = false;
                                _networkStream.Close();
                                _logger.LogInfo("Disconnected from Linac");
                                break;
                            }
                            else
                            {
                                ProcessCommand(command, dataRead);
                            }
                        }
                        catch (IOException)
                        {
                            // This even can be used to clear all values on a disconnection
                            if (ProcessCommandEvent != null)
                                ProcessCommandEvent(null);
                            if (_flushNetworkStream)
                            {
                                if (_networkStream.DataAvailable)
                                {
                                    StackTrace stackTrace = new StackTrace(true);
                                    _logger.LogError("Flushing network stream",
                                        stackTrace);
                                }
                                while (_networkStream.DataAvailable)
                                {
                                    byte[] dummyBuffer = new byte[256];
                                    _networkStream.Read(dummyBuffer, 0, dummyBuffer.Length);
                                }
                                _flushNetworkStream = false;
                                _FlushingNetworkDoneEvent.Set();
                            }
                            IsConnected = false;
                            _networkStream.Close();
                            _logger.LogInfo("Disconnected from Linac");
                        }
                        catch (ObjectDisposedException exp)
                        {
                            _logger.LogError(exp);
                            IsConnected = false;
                            _networkStream.Close();
                            _logger.LogInfo("Disconnected from Linac");
                        }
                    }
                }
                catch (Exception exp)
                {
                    //connection error, try to connect again.
                    _logger.LogError(exp);
                    _logger.LogInfo("Exception in LinacAccess: " + exp.StackTrace);
                }
            }
        }

        private void ProcessCommand(byte[] cmd, int size)
        {
            LinacPacketFormat.CommandPacket command = new LinacPacketFormat.CommandPacket();
            command = command.Deserialize(cmd);

            int dataLength = Marshal.SizeOf(typeof(LinacPacketFormat.DataPacket));
            byte[] data = new byte[dataLength];
            int dataRead = 0;

            if (command.Command == LinacPacketFormat.CommandEnum.Ping)
            {
                _pingAckReceived = true;
            }
            else if (command.Command == LinacPacketFormat.CommandEnum.Unsolicited)
            {
                Dictionary<LinacPacketFormat.VariableIdEnum, object> commandList = new Dictionary<LinacPacketFormat.VariableIdEnum, object>();
                object dataValue;

                for (int packetNum = 0; packetNum < command.Size; packetNum++)
                {
                    //number of packets following command packet
                    dataRead = _networkStream.Read(data, 0, data.Length);

                    //deserialize data packet
                    LinacPacketFormat.DataPacket dataPacket = new LinacPacketFormat.DataPacket();
                    dataPacket = dataPacket.Deserialize(data);
                    
                    if (dataPacket.dataType == LinacPacketFormat.DataTypeEnum.Decimal)
                    {
                        dataValue = BitConverter.ToUInt32(dataPacket.data, 0);
                    }
                    else if (dataPacket.dataType == LinacPacketFormat.DataTypeEnum.Float)
                    {
                        dataValue = BitConverter.ToSingle(dataPacket.data, 0);
                    }
                    else //string
                    {
                        dataValue = BitConverter.ToString(dataPacket.data);
                    }

                    commandList.Add(dataPacket.variableId, dataValue);
                }

                if (ProcessCommandEvent != null)
                    ProcessCommandEvent(commandList);
            }
            else
            {

                Dictionary<LinacPacketFormat.VariableIdEnum, object> commandList = new Dictionary<LinacPacketFormat.VariableIdEnum, object>();

                for (int packetNum = 0; packetNum < command.Size; packetNum++)
                {
                    //number of packets following command packet
                    dataRead = _networkStream.Read(data, 0, data.Length);

                    //deserialize data packet
                    LinacPacketFormat.DataPacket dataPacket = new LinacPacketFormat.DataPacket();
                    dataPacket = dataPacket.Deserialize(data);

                    commandList.Add(dataPacket.variableId, dataPacket.data);
                }


                if (ProcessCommandEvent != null)
                    ProcessCommandEvent(commandList);
                

                if (!_commandResponseEvent.Set())
                {
                    _logger.LogError("setting _commandResponseEvent returned false", new StackTrace(true));
                }
            }
        }

        private List<LinacPacketFormat.DataPacket> SendCommand(LinacPacketFormat.CommandPacket command, LinacPacketFormat.DataPacket data, bool expectingReply)
        {
            List<LinacPacketFormat.DataPacket> receiveData;
            byte[] commandbuffer = command.Serialize();

            _networkStream.Write(commandbuffer, 0, commandbuffer.Length);

            byte[] databuffer = data.Serialize();

            _commandResponseList.Clear();

            _networkStream.Write(databuffer, 0, databuffer.Length);

            if (expectingReply)
            {
                //wait for response
                if (_commandResponseEvent.WaitOne(2000))
                {
                    receiveData = _commandResponseList;
                }
                else
                {
                    receiveData = null;
                }
            }
            else
            {
                throw new Exception("Host not connected");
            }

            return receiveData;
        }

        #endregion

        #region Public Methods

        public void Open()
        {
            if (_dataAckThread == null)
            {
                _dataAckThread = new Thread(new ThreadStart(DataAckThreadMethod));
                _dataAckThread.Name = "LINAC Data Ack";
                _dataAckThread.IsBackground = true;
                _dataAckThread.Start();
            }
        }

        public void Close()
        {
            if (_dataAckThread != null)
            {
                if (_networkStream != null)
                {
                    _networkStream.Close();
                    _networkStream.Dispose();
                }
                _dataAckThread.Join();
                _dataAckThread = null;
            }
        }

        public void Dispose()
        {

            _shutdown = true;
            this.Close();
        }

        public void GetOperatingParameters(ref Dictionary<LinacPacketFormat.VariableIdEnum, object> parameterList)
        {
            Array values = Enum.GetValues(typeof(LinacPacketFormat.VariableIdEnum));
                
            LinacPacketFormat.CommandPacket command = new LinacPacketFormat.CommandPacket(LinacPacketFormat.CommandEnum.Read, LinacPacketFormat.TypeEnum.Query);
            command.Size = 1;

            LinacPacketFormat.DataPacket data = new LinacPacketFormat.DataPacket();
            data.variableId = LinacPacketFormat.VariableIdEnum.OperatingParameters;
            data.dataType = LinacPacketFormat.DataTypeEnum.None;

            try
            {
                List<LinacPacketFormat.DataPacket> response = SendCommand(command, data, true);
            }
            catch (Exception e)
            {
            }
            //Console.WriteLine(String.Format("{0}: {1}", data.variableId, data.dataType )); // For Debugging

        }
       
        
        #endregion
    }
}
