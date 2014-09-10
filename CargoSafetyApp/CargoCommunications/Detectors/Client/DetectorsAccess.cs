using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.APCS.Client;
using L3.Cargo.Communications.Detectors.Common;
using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Detectors.Common;

namespace L3.Cargo.Communications.Detectors.Client
{
    public class DetectorsAccess : IDisposable, INotifyPropertyChanged
    {   
        private struct AllLineData
        {
            public UInt32 LineID;
            public long LineTimeStamp;
            public Int32[] BytesRecieved;
            public bool[] DataComplete;
            public DataInfo[] NCBData;
        }

        public int BytesPerPixel
        {
            get
            {
                if (/*prepare?*/ !BytesPerPixelOK(_bytesPerPixel))
                    try { BytesPerPixel = int.Parse(ConfigurationManager.AppSettings[BytesPerPixelKey], CultureInfo.InvariantCulture); }
                    catch { BytesPerPixel = BytesPerPixelDftValue; }
                return _bytesPerPixel;
            }
            private set
            {
                if (/*invalid?*/ !BytesPerPixelOK(value))
                    throw new ArgumentOutOfRangeException(ClassName + ": " + "BytesPerPixel value (" + value.ToString() + ") must in in the domain " + BytesPerPixelDomain);
                if (value != _bytesPerPixel)
                {
                    _bytesPerPixel = value;
                    try { Logger.LogInfo("Image Bytes per Pixel/" + BytesPerPixelKey + " set to " + value.ToString()); }
                    catch { }
                }
            }
        }
        private int _bytesPerPixel = /*invalid, causes preparation*/ int.MinValue;
        public const int BytesPerPixelDftValue = 3;
        public string BytesPerPixelDomain { get { return "[" + BytesPerPixelMinValue.ToString() + ", " + BytesPerPixelMaxValue.ToString() + "]"; } }        public const string BytesPerPixelKey = "DetectorBytesPerPixel";
        public const int BytesPerPixelMaxValue = 4;
        public const int BytesPerPixelMinValue = 1;
        private static bool BytesPerPixelOK(int value) { return (value >= BytesPerPixelMinValue) && (value <= BytesPerPixelMaxValue); }

        private static string ClassName { get { return MethodBase.GetCurrentMethod().DeclaringType.Name; } }

        private void CommandConnectionConfigure(int ncbIndex)
        {
            Debug.Assert((ncbIndex >= 0) && (ncbIndex < NCBCount));
            try
            {
                // Put the NCB into configuration mode, set the parameters and release the NCB.
                if (/*run?*/ !_commandMonitorEnd.WaitOne(0))
                {
                    // Acquire the configuration values.
                    ushort pulseDelay = (ushort)PulseDelay;
                    Debug.Assert((pulseDelay >= 1) && (pulseDelay <= ushort.MaxValue));
                    ushort signOfLife = (ushort)SignOfLife;
                    Debug.Assert((signOfLife >= 1) && (signOfLife <= ushort.MaxValue));
                    SetConfigurationMode(configurationMode.Begin, ncbIndex);
                    if (/*run?*/ !_commandMonitorEnd.WaitOne(0))
                        SetConfigurationParameters(pulseDelay, signOfLife, ncbIndex);
                }
            }
            catch { throw; }
            finally
            {
                try { SetConfigurationMode(configurationMode.End, ncbIndex); }
                catch { }
            }
        }
        private void CommandConnectionCreate()
        {
            if (/*create clients?*/ _commandConnection == null)
            {
                CommandIsReady = false;
                if (/*run?*/ !_commandMonitorEnd.WaitOne(0))
                    _commandConnection = new TcpClient[NCBCount];
            }
            if (/*(re)create?*/ _commandConnection != null)
                for (int ix = 0; ix < _commandConnection.Length; ix++)
                    if (/*(re)create?*/ _commandConnection[ix] == null)
                        if (/*run?*/ !_commandMonitorEnd.WaitOne(0))
                        {
                            CommandIsReady = false;
                            if (/*run?*/ !_commandMonitorEnd.WaitOne(0))
                                _commandConnection[ix] = new TcpClient();
                        }
            if (/*(re)connect?*/ _commandConnection != null)
            {
                bool doReady = false;
                for (int ix = 0; ix < _commandConnection.Length; ix++)
                    if (/*exists?*/ _commandConnection[ix] != null)
                        if (/*not connected?*/ !_commandConnection[ix].Connected)
                        {
                            _commandConnection[ix].Connect(NCBIndexToIPAddress(ix), CommandIpPort);
                            CommandConnectionConfigure(ix);
                            doReady = true;
                        }
                if (/*perform ready?*/ doReady)
                    if (/*run?*/ !_commandMonitorEnd.WaitOne(0))
                        CommandIsReady = true;
            }
        }
        private void CommandConnectionDispose()
        {
            if (/*exists?*/ _commandConnection != null)
                for (int ix = 0; ix < _commandConnection.Length; ix++)
                {
                    try { _commandConnection[ix].Client.Disconnect(false); }
                    catch { }
                    try { _commandConnection[ix].Close(); }
                    catch { }
                    try { CommandStream(ix).Dispose(); }
                    catch { }
                }
            _commandConnection = null;
        }
        private TcpClient[] _commandConnection = null;

        public string CommandIpAddress
        {
            get
            {
                if (/*prepare?*/ string.IsNullOrWhiteSpace(_commandIpAddress))
                    try { CommandIpAddress = ConfigurationManager.AppSettings[CommandIpAddressKey]; }
                    catch { CommandIpAddress = /*default*/ "192.168.0.33"; }
                return _commandIpAddress;
            }
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException(ClassName + ": " + "CommandIPAddress must not be null or empty");
                IPAddress /*address*/ adr;
                if (!IPAddress.TryParse(value, out adr))
                    throw new ArgumentOutOfRangeException(ClassName + ": " + "CommandIPAddress (\"" + value + "\") must be a valid IP address");
                if (adr == IPAddress.None)
                    throw new ArgumentOutOfRangeException(ClassName + ": " + "CommandIPAddress must not be IPAddress.None");
                value = adr.ToString(); //tidy candidate value
                if (value != _commandIpAddress)
                {
                    _commandIpAddress = value;
                    try { Logger.LogInfo("Command IP Address/" + CommandIpAddressKey + " set to " + value); }
                    catch { }
                }
            }
        }
        private string _commandIpAddress = null;
        public const string CommandIpAddressKey = "DataAcqServer";

        public int CommandIpPort
        {
            get
            {
                if (/*prepare?*/ (_commandIpPort < IPEndPoint.MinPort) || (_commandIpPort > IPEndPoint.MaxPort))
                    try { CommandIpPort = int.Parse(ConfigurationManager.AppSettings[CommandIpPortKey], CultureInfo.InvariantCulture); }
                    catch { CommandIpPort = /*default*/ 45296; }
                return _commandIpPort;
            }
            private set
            {
                if (/*invalid?*/ (value < IPEndPoint.MinPort) || (value > IPEndPoint.MaxPort))
                    throw new ArgumentOutOfRangeException(ClassName + ": " + "CommandIPPort value (" + value.ToString() + ") must be in the domain [" + IPEndPoint.MinPort.ToString() + ", " + IPEndPoint.MaxPort.ToString() + "]");
                if (value != _commandIpPort)
                {
                    _commandIpPort = value;
                    try { Logger.LogInfo("Command IP Port/" + CommandIpPortKey + " set to " + value.ToString()); }
                    catch { }
                }
            }
        }
        private int _commandIpPort = /*invalid, causes preparation*/ int.MinValue;
        public const string CommandIpPortKey = "DataAcqCommandPort";

        private bool CommandIsConnected
        {
            get { return _commandIsConnected; }
            set
            {
                if (/*change?*/ _commandIsConnected != value)
                {
                    _commandIsConnected = value;
                    if (/*not connected?*/ !_commandIsConnected)
                        CommandIsReady = false; /*if not connected, it's not ready, either*/
                }
            }
        }
        private bool _commandIsConnected = false;

        /// <summary>CommandIsReady specifies whether or not the NCBs are ready to perform
        /// actions.</summary>
        /// <returns>NCB readiness</returns>
        [DefaultValue(true)]
        public bool CommandIsReady { get; private set; }

        #region Command Margin
        /// <summary>
        /// Command Margin specifies, in milliseconds, the period that must elapse between each
        /// command.</summary>
        /// <value>margin, in milliseconds ... is adjusted to [0, TimeSECOND]</value>
        /// <returns>command margin</returns>
        public int CommandMargin
        {
            get
            {
                if (/*prepare?*/ _commandMargin.Period > CommandMarginMaxValue)
                    try { _commandMargin.Period = Utilities.PropertyConfigureInt(CommandMarginKey, /*minimum*/ 0, /*maximum*/ CommandMarginMaxValue, /*default*/ 0); }
                    catch
                    {
                        CommandMargin = 0;
                        throw; /*exception prepared by PropertyConfigureInt*/
                    }
                return _commandMargin.Remaining;
            }
            private set { _commandMargin.Period = value; }
        }
        private Latency _commandMargin = new Latency(Latency.MaxValue);
        /// <summary/>
        public const string CommandMarginKey = "NCBCommandMargin";
        /// <summary/>
        public const int CommandMarginMaxValue = Time10SECONDS;
        private void CommandMarginReset() { _commandMargin.Reset(); }
        #endregion

        private void CommandMonitorAgent()
        {
            _commandMonitorException = null;
            EventLoggerAccess loggerAccess = Logger; /*prevent unwanted, logger disposal*/
            LogPause = 0;
            try { loggerAccess.LogInfo(Threads.Identity() + " started"); }
            catch { }
            try
            {
                for (int cycleTime = 0 /*ms*/; /*run?*/ !_commandMonitorEnd.WaitOne(cycleTime); cycleTime = SignOfLifeMilliseconds / 2)
                    try
                    {
                        CommandConnectionCreate(); /*if OK, does nothing*/
                        if (/*ready?*/ CommandIsReady)
                            if (/*run?*/ !_commandMonitorEnd.WaitOne(0))
                            {
                                for (int ix = 0; ix < NCBCount; ix++)
                                    if (/*run?*/ !_commandMonitorEnd.WaitOne(0))
                                        GetIdentification(ix);
                                    else /*quit*/
                                        break;
                                LogPause = 0;
                            }
                    }
                    catch (Exception ex)
                    {
                        if (/*run?*/ !_commandMonitorEnd.WaitOne(0))
                            LogPauseError(Utilities.TextTidy(ex.ToString()));
                        CommandConnectionDispose();
                        DataConnectionDispose();
                    }
            }
            catch (Exception ex) { _commandMonitorException = ex; }
#if false
            finally
            {
                try { loggerAccess.LogInfo(Threads.Identity() + " ended"); }
                catch { }
            }
#endif
        }
        private void CommandMonitorStart()
        {
            if (/*(re)create?*/ _commandMonitorThread == null)
                _commandMonitorThread = Threads.Create(CommandMonitorAgent, ref _commandMonitorEnd, "Command Monitor thread");
            if (/*(re)start?*/ _commandMonitorThread != null)
                if (/*(re)start?*/ !_commandMonitorThread.IsAlive)
                {
                    _commandMonitorEnd.Reset();
                    _commandMonitorException = null;
                    _commandMonitorThread.Start();
                }
        }
        private void CommandMonitorStop()
        {
            _commandMonitorEnd.Set();
            CommandConnectionDispose();
            try { _commandMonitorThread = Threads.Dispose(_commandMonitorThread, ref _commandMonitorEnd); }
            catch { }
            finally { _commandMonitorThread = null; }
#if DEBUG
            if (/*anomalies?*/ _commandMonitorException != null)
                try { Logger.LogError(Utilities.TextTidy(_commandMonitorException.ToString())); }
                catch { }
#endif
        }
        private ManualResetEvent _commandMonitorEnd = new ManualResetEvent(false);
        private Exception _commandMonitorException;
        private Thread _commandMonitorThread;

        private CommPacketHeader? CommandSend(CommPacketHeader command, messageType expectedResponse, int ncbIndex = 0)
        {
            CommPacketHeader? reply = null;
            try
            {
                if (/*run?*/ !_commandMonitorEnd.WaitOne(_commandMargin.Remaining))
                    lock (_commandSendLock)
                        if (/*run?*/ !_commandMonitorEnd.WaitOne(0))
                        {
                            Debug.Assert((ncbIndex >= 0) && (ncbIndex < NCBCount));
                            if (/*not connected?*/ CommandStream(ncbIndex) == null)
                                throw new Exception("not connected to " + NCBName(ncbIndex));
                            command.SyncID = 0xA55A55AA;
                            byte[] buffer = command.Serialize();
                            CommandStream(ncbIndex).WriteTimeout = TimeTENTH;
                            Debug.Assert(CommandStream(ncbIndex).CanWrite);
                            CommandStream(ncbIndex).Write(buffer, 0, buffer.Length);
                            CommandStream(ncbIndex).Flush();
                            _commandMargin.Reset();

                            buffer = new byte[Marshal.SizeOf(typeof(CommPacketHeader))];
                            for (int retry = 0; /*OK?*/ (retry < CommandSendReads); reply = null, retry++)
                                if (/*run?*/ !_commandMonitorEnd.WaitOne(retry * TimeTENTH))
                                {
                                    // Read blocks/waits up through
                                    // TcpClient.GetStream().ReadTimeout; if the
                                    // client is disposed, the read faults.
                                    CommandStream(ncbIndex).ReadTimeout = TimeTENTH;
                                    Debug.Assert(CommandStream(ncbIndex).CanRead);
                                    if (/*read OK?*/ CommandStream(ncbIndex).Read(buffer, 0, buffer.Length) >= 1)
                                    {
                                        reply = command.Deserialize(buffer);
                                        if (/*OK?*/ reply != null)
                                            if (/*OK?*/ reply.HasValue)
                                                if (reply.Value.MessageID == expectedResponse)
                                                    break;
                                                else if (reply.Value.MessageID == messageType.CX_SIGNOFLIFE_NORMAL)
                                                    retry--; /*don't count sign of life responses as tries*/
                                    }
                                }
                        }
            }
            catch (Exception ex)
            {
                CommandConnectionDispose();
                reply = null;
                throw ex;
            }
            return reply;
        }
        private object _commandSendLock = new object();
        private const int CommandSendReads = 3;

        private NetworkStream CommandStream(int ncbIndex)
        {
            if (/*OK?*/ _commandConnection != null)
                if (/*OK?*/ _commandConnection[ncbIndex] != null)
                    if (/*OK?*/ _commandConnection[ncbIndex].Connected)
                        return _commandConnection[ncbIndex].GetStream();
            return null;
        }

        /// <summary>Start the instance.</summary>
        /// <remarks>
        /// This method is written so that it may be called repeatedly, whether or not the instance
        /// is already started.</remarks>
        /// <exception cref="Exception">
        /// The method makes use of multiple supporting methods, any one of which might throw an
        /// exception if an anomaly is detected.</exception>
        public void Connect() { Start(); }

        public uint CurrentLineId
        {
            get { return _currentLineId; }
            set
            {
                // Don't log any changes: they occur too frequently and will just clog the gears.
                _currentLineId = value;
                PropertyOnChange(new PropertyChangedEventArgs("CurrentLineId"));
            }
        }
        private uint _currentLineId = 0U;

        private void DataConnectionCreate()
        {
            if (/*(re)create?*/ _dataConnection == null)
            {
                _dataConnection = new UdpClient(DataIPPort);
                _dataConnection.Client.ReceiveBufferSize = /*1M*/ 1048576;
                GetNumberOfChannels();
                DataIsReady = true;
                SetDataTransferMode(dataTransferMode.Start);
            }
        }
        private void DataConnectionDispose()
        {
            DataIsReady = false;
            try { SetDataTransferMode(dataTransferMode.Stop); }
            catch { }
            try { _dataConnection.Close(); }
            catch { }
            _dataConnection = null;
        }
        private UdpClient _dataConnection;

        private void DataGatherAgent()
        {
            _dataGatherException = null;
            EventLoggerAccess loggerAccess = Logger; /*prevent unwanted, logger disposal*/
            LogPause = 0;
            try { loggerAccess.LogInfo(Threads.Identity() + " started"); }
            catch { }
            Dictionary<uint, AllLineData> stagedLines = new Dictionary<uint, AllLineData>(); /*used by DataHarvest*/
            try
            {
                int cycleTime = 0 /*ms*/;
                do
                    try
                    {
                        if (/*not ready?*/ !CommandIsReady)
                            cycleTime = TimeSECOND;
                        else /*ready to connect*/
                        {
                            DataConnectionCreate(); /*if OK, does nothing*/
                            if (/*not ready?*/ !DataIsReady)
                                cycleTime = TimeSECOND;
                            else /*ready*/
                            {
                                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                                byte[] dataBytes = _dataConnection.Receive(ref endPoint);
                                if (/*fail?*/ dataBytes.Length < 1)
                                    throw new Exception("empty data packet;");
                                int ncbIndex = NCBIndexFromIPAddress(endPoint.Address);
                                DataHarvest(dataBytes, ref stagedLines, ncbIndex);
                                LogPause = 0;
                                cycleTime = 0; /*full speed*/
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (/*run?*/ !_dataGatherEnd.WaitOne(0))
                            LogPauseError(Utilities.TextTidy(ex.ToString()));
                        DataConnectionDispose();
                    }
                while (/*run?*/ !_dataGatherEnd.WaitOne(cycleTime));
            }
            catch (Exception ex) { _dataGatherException = ex; }
            finally
            {
                stagedLines.Clear();
#if false
                try { loggerAccess.LogInfo(Threads.Identity() + " ended"); }
                catch { }
#endif
            }
        }
        private void DataGatherStart()
        {
            if (/*create?*/ _dataGatherThread == null)
                _dataGatherThread = Threads.Create(DataGatherAgent, ref _dataGatherEnd, "Data Gather thread");
            if (/*(re)start?*/ !_dataGatherThread.IsAlive)
            {
                _dataGatherEnd.Reset();
                _dataGatherException = null;
                _dataGatherThread.Start();
            }
        }
        private void DataGatherStop()
        {
            _dataGatherEnd.Set();
            DataConnectionDispose();
            try { _dataGatherThread = Threads.Dispose(_dataGatherThread, ref _dataGatherEnd); }
            catch { }
            finally { _dataGatherThread = null; }
#if DEBUG
            if (/*anomalies?*/ _dataGatherException != null)
                try { Logger.LogError(Utilities.TextTidy(_dataGatherException.ToString())); }
                catch { }
#endif
        }
        private ManualResetEvent _dataGatherEnd = new ManualResetEvent(false);
        private Exception _dataGatherException;
        private Thread _dataGatherThread;

        public int DataGatherTrace
        {
            get { return _dataGatherTrace; }
            set { _dataGatherTrace = Math.Max(0, value) % 100; }
        }
        private int _dataGatherTrace = 0;

        private void DataHarvest(byte[] rawData, ref Dictionary<uint, AllLineData> stagedLines, int ncbIndex)
        {
#if false
            ushort StartNumOfDetectors = BitConverter.ToUInt16(rawData, 4);
            if (ncbIndex == 0 && StartNumOfDetectors == 0)
            {
                Array.Reverse(rawData, 0, 4);
                CurrentLineId = BitConverter.ToUInt32(rawData, 0);
            }
#endif

            // Decode the data into a header.
            DataPacketHeader packetHeader = new DataPacketHeader();
            packetHeader = packetHeader.Deserialize(rawData);

            //bUInt32 currLineId = packetHeader.LineID;

            if (ncbIndex == 0 && packetHeader.StartNumOfDetectors == 0)
                CurrentLineId = packetHeader.LineID; /*currLineId*/

            //Logger.LogInfo(String.Format("NCB Index = {0}, Start Num = {1}, End Num = {2}", (int)ncbIndex, packetHeader.StartNumOfDetectors, packetHeader.EndNumOfDetectors));

            if (SourcesSynchronized)
            {
                if (!stagedLines.ContainsKey(packetHeader.LineID))
                {
                    DataInfo[] lineArr = new DataInfo[NCBCount];

                    AllLineData allData = new AllLineData();

                    allData.LineID = packetHeader.LineID;
                    allData.LineTimeStamp = DateTime.UtcNow.Ticks;
                    allData.DataComplete = new bool[NCBCount];
                    allData.BytesRecieved = new int[NCBCount];
                    allData.NCBData = lineArr;

                    for (int ix = 0; ix < NCBCount; ix++)
                    {
                        lineArr[ix] = new DataInfo(_ncbChannelCount[ix], packetHeader.EnergyAndPulsewidth, (byte)ImageBytesPerPixel);
                        allData.DataComplete[ix] = false;
                    }

                    stagedLines.Add(packetHeader.LineID, allData);
                }

                DataInfo currentNCBDataLine = stagedLines[packetHeader.LineID].NCBData[ncbIndex];
                int size = (int)((packetHeader.EndNumOfDetectors - packetHeader.StartNumOfDetectors + 1) * packetHeader.NumBytesPerPixel);
                int linePixelIndex = packetHeader.StartNumOfDetectors;

                //if(linePixelIndex == 2336)
                //      Logger.LogInfo(String.Format("NCB Index = {0}, Line Id = {1}, LinePixelIndex = {2}, Start Num = {3}, End Num = {4}", (int)ncbIndex, CurrentLineId, linePixelIndex, packetHeader.StartNumOfDetectors, packetHeader.EndNumOfDetectors));

                for (int index = 0; index < size; index += packetHeader.NumBytesPerPixel)
                {
                    if (currentNCBDataLine.LineData[linePixelIndex] == null)
                    {
                        currentNCBDataLine.TotalBytesReceived += packetHeader.NumBytesPerPixel;
                        currentNCBDataLine.LineData[linePixelIndex] = PixelConverter.BytesToPixel(packetHeader.ChData, index, _bytesPerPixel);
                        int offset = 8 * (_bytesPerPixel - (int)_imageBytesPerPixel);
                        currentNCBDataLine.LineData[linePixelIndex].Value = currentNCBDataLine.LineData[linePixelIndex++].Value >> offset;
                    }
                }

                stagedLines[packetHeader.LineID].BytesRecieved[ncbIndex] = currentNCBDataLine.TotalBytesReceived;

                //if (linePixelIndex == 2336)
                //    Logger.LogInfo(String.Format("Data Complete for Line Id = {0}", CurrentLineId));

                if ((currentNCBDataLine.TotalBytesReceived / packetHeader.NumBytesPerPixel) >= (_ncbChannelCount[ncbIndex]))
                {
                    stagedLines[packetHeader.LineID].DataComplete[ncbIndex] = true;

                    bool IsLineComplete = true;

                    for (int i = 0; i < NCBCount; i++)
                    {
                        IsLineComplete &= stagedLines[packetHeader.LineID].DataComplete[i];
                    }

                    if (IsLineComplete)
                    {
                        DataInfo currentDataLine = new DataInfo(PixelsPerColumn, currentNCBDataLine.XRayInfo, currentNCBDataLine.NumberOfBytesPerPixel);

                        linePixelIndex = 0;

                        for (int i = 0; i < NCBCount; i++)
                        {
                            for (int index = 0; index < stagedLines[packetHeader.LineID].NCBData[i].LineData.Length; index++)
                            {
                                if (i == 0)
                                {
                                    if (index == ReferenceSensorCount)
                                        index += ReferenceSensorCount;
                                }
                                else
                                {
                                    if (index == 0)
                                        index += ReferenceSensorCount;
                                }

                                currentDataLine.LineData[linePixelIndex] = stagedLines[packetHeader.LineID].NCBData[i].LineData[index];
                                linePixelIndex++;
                            }

                            stagedLines[packetHeader.LineID].NCBData[i].Dispose();
                        }

                        //add this line to the data collection
                        _rawDataCollection.TryAdd(currentDataLine, 500);

                        stagedLines.Remove(packetHeader.LineID);
                        DataGatherTrace--;
                    }

                    List<uint> keysToRemove = new List<uint>();

                    foreach (KeyValuePair<uint, AllLineData> aLine in stagedLines)
                    {
                        //Check if the line is not complete in 200,000 (20 ms - unit is 100 ns)
                        if (DateTime.UtcNow.Ticks > aLine.Value.LineTimeStamp + 500000)
                        {
                            keysToRemove.Add(aLine.Key);
                        }
                    }

                    //                    foreach (KeyValuePair<uint, AllLineData> aLine in stagedLines)

                    String removedLineIndices = "Removing old line index: ";

                    foreach (uint key in keysToRemove)
                    {
                        //Logger.LogInfo(String.Format("Removing old line index: {0}, Bytes Received = {1},{2}", identity, 
                        //                               stagedLines[identity].NCBData[0].TotalBytesReceived, 
                        //                               stagedLines[identity].NCBData[1].TotalBytesReceived));
                        removedLineIndices += String.Format("{0}, ", key);

                        for (int i = 0; i < NCBCount; i++)
                        {
                            stagedLines[key].NCBData[i].Dispose();
                        }

                        stagedLines.Remove(key);
                    }

                    if (keysToRemove.Count > 0)
                    {
                        //bLogger.LogInfo(removedLineIndices);
                        keysToRemove.Clear();
                    }
                }
            }
        }

        public int DataIPPort
        {
            get
            {
                if (/*prepare?*/ (_dataIPPort < IPEndPoint.MinPort) || (_dataIPPort > IPEndPoint.MaxPort))
                    try { DataIPPort = int.Parse(ConfigurationManager.AppSettings[DataIPPortKey], CultureInfo.InvariantCulture); }
                    catch { DataIPPort = /*default*/ 45296; }
                return _dataIPPort;
            }
            private set
            {
                if (/*invalid?*/ (value < IPEndPoint.MinPort) || (value > IPEndPoint.MaxPort))
                    throw new ArgumentOutOfRangeException(ClassName + ": " + "DataIPPort value (" + value.ToString() + ") must in in the domain [" + IPEndPoint.MinPort.ToString() + ", " + IPEndPoint.MaxPort.ToString() + "]");
                if (value != _dataIPPort)
                {
                    _dataIPPort = value;
                    try { Logger.LogInfo("Data IP Port/" + DataIPPortKey + " set to " + value.ToString()); }
                    catch { }
                }
            }
        }
        private int _dataIPPort = /*invalid, causes preparation*/ int.MinValue;
        public const string DataIPPortKey = "DataAcqStreamPort";

        public bool DataIsReady
        {
            get { return _dataIsReady; }
            private set
            {
                if (value != _dataIsReady)
                {
                    _dataIsReady = value;
                    if (/*run?*/ !_dataGatherEnd.WaitOne(0))
                    {
                        if (/*subscribers?*/ ReadyEvent != null)
                            try { ReadyEvent(_dataIsReady); }
                            catch { }
                        if (_dataIsReady)
                            try { Logger.LogInfo("Detectors is/are ready"); }
                            catch { }
                    }
                }
            }
        }
        private bool _dataIsReady = true;
        private void DataReadyDispose() { ReadyEvent = null; }
        public event ConnectionStateChangeHandler ReadyEvent;

        /// <summary>
        /// This is a safety net to ensure that resources are tidied even if
        /// <see cref="Dispose()"/> is not called.</summary>
        /// <remarks>Assuming that the <see cref="Dispose()"/> method was called, explicitly and
        /// properly, by consumer code, <see cref="Disposed"/> is expected to yield false. However,
        /// should that not be the case, this finalizing call ensures that unmanaged resources are
        /// tidied, properly.</remarks>
        ~DetectorsAccess() { Dispose(!Disposed); }

        public DetectorsAccess(EventLoggerAccess eventLogger)
        {   // This class is a singleton: there may be but one instance at any one time. Use the
            // static logger reference as a gate.
            if (/*invalid?*/ eventLogger == null)
                throw new ArgumentNullException(ClassName + ": " +
                        "logger access (eventLogger) must not be null");
            Logger = eventLogger;

            // Prepare several properties by reassigning their current/initial values. The property
            // methods automatically fetch, validate and assign the application's configuration
            // values.
            BytesPerPixel = BytesPerPixel;
            ImageBytesPerPixel = ImageBytesPerPixel;
            CommandIpAddress = CommandIpAddress;
            CommandIpPort = CommandIpPort;
            DataIPPort = DataIPPort;

            try { Logger.LogInfo("+++" + ClassName + "+++"); }
            catch { }

            CommandIsReady = DataIsReady = false;
        }

        /// <summary>Stop the instance.</summary>
        /// <remarks>
        /// This method is written so that it may be called repeatedly, whether or not the instance
        /// is already stopped.</remarks>
        /// <exception cref="Exception">
        /// The method is written so that it never throws an exception.</exception>
        public void Disconnect() { Stop(); }

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
                LogPause = 0;

                DataGatherStop();
                CommandMonitorStop();
                DataReadyDispose();
                DataConnectionDispose();
                CommandConnectionDispose();

                try { Logger.LogInfo("---" + ClassName + "---"); }
                catch { }
                Logger = null;
                Disposed = true;
            }
        }

        /// <summary>
        /// Disposed declares whether or not the class instance has been disposed.</summary>
        [DefaultValue(false)]
        public static bool Disposed { get; private set; }

        public void GetConfigParameters(out ushort pulseDelay, out ushort signOfLife)
        {
            pulseDelay = (ushort)PulseDelay;
            Debug.Assert(pulseDelay <= ushort.MaxValue);
            signOfLife = (ushort)SignOfLife;
            Debug.Assert((signOfLife >= 1) && (signOfLife <= ushort.MaxValue));
            string traceText = MethodBase.GetCurrentMethod().Name + "(" + pulseDelay.ToString() + " * 20ns, " + signOfLife.ToString() + "cs)";
            try
            {
                CommPacketHeader command = new CommPacketHeader();
                command.MessageID = messageType.CX_CONFIG_PARAMETERS_QUERY;
                CommPacketHeader? reply = CommandSend(command, messageType.CX_CONFIG_PARAMETERS_ACK);
                if (/*no response?*/ reply == null)
                    throw new Exception(traceText + ": no/null response");
                pulseDelay = reply.Value.PulseDelayPeriod;
                signOfLife = reply.Value.SignOfLifeInterval;
            }
            catch { throw; }
        }

        public deviceState GetDeviceState(out ushort sensorCount, out ushort referenceCount, int ncbIndex = 0)
        {
            referenceCount = sensorCount = 0;
            deviceState returnedDeviceState = deviceState.Configuration;
            CommPacketHeader command = new CommPacketHeader();
            command.MessageID = messageType.CX_DEVICE_STATE_QUERY;
            CommPacketHeader? reply = CommandSend(command, messageType.CX_DEVICE_STATE_ACK, ncbIndex);
            if (/*no response?*/ reply == null)
                throw new Exception("no/null response");
            returnedDeviceState = reply.Value.DeviceState;
            referenceCount = reply.Value.NumOfReferenceDetectors;
            sensorCount = reply.Value.NumOfChannels;
            return returnedDeviceState;
        }

        public int GetIdentification(int ncbIndex = 0)
        {
            int returnedIdentity = 0;
            CommPacketHeader command = new CommPacketHeader();
            command.MessageID = messageType.CX_IDENTFY_QUERY;
            CommPacketHeader? reply = CommandSend(command, messageType.CX_IDENTFY_ACK, ncbIndex);
            if (/*no response?*/ reply == null)
                throw new Exception("no/null response");
            returnedIdentity = reply.Value.NcbId;
            return returnedIdentity;
        }

        public int GetNumberOfChannels()
        {
            _sensorOffset = new int[NCBCount];
            _actualImagingChannelCount = new int[NCBCount];
            _actualReferenceChannelCount = new int[NCBCount];
            _ncbChannelCount = new int[NCBCount];

            int pixelCount = ReferenceSensorCount; /*avoid PixelsPerColumn traffic!*/
            int tempPixelsPerColumn = 0;

            for (int ix = 0; /*run?*/ !_commandMonitorEnd.WaitOne(0) && (ix < NCBCount); ix++)
            {
                ushort referenceCount;
                ushort sensorCount;
                GetDeviceState(out sensorCount, out referenceCount, ix);
                pixelCount += (int)sensorCount;
                _sensorOffset[ix] = pixelCount;

                _actualImagingChannelCount[ix] = sensorCount;
                _actualReferenceChannelCount[ix] = referenceCount;

                _ncbChannelCount[ix] = ((ix == 0) ? sensorCount + ReferenceSensorCount : sensorCount) + ReferenceSensorCount;
                tempPixelsPerColumn += _actualImagingChannelCount[ix];
                try { Logger.LogInfo(NCBName(ix) + ": " + ((int)sensorCount).ToString() + " sensors" + "; offset " + _sensorOffset[ix].ToString()); }
                catch { }
            }
            PixelsPerColumn = tempPixelsPerColumn + ReferenceSensorCount;
            return PixelsPerColumn;
        }
        private int[] _actualImagingChannelCount = null;
        private int[] _actualReferenceChannelCount = null;
        private int[] _sensorOffset = null;

        public int GetNumberReferencePixels() { return ReferenceCount; }

        public xrayDataState GetXRayDataState(int ncbIndex = 0)
        {
            string traceText = MethodBase.GetCurrentMethod().Name + "(" + ncbIndex.ToString() + ")";
            xrayDataState returnedXrayState = xrayDataState.Ready;
            try
            {
                CommPacketHeader command = new CommPacketHeader();
                command.MessageID = messageType.CX_XRAY_DATA_STATE_QUERY;
                CommPacketHeader? reply = CommandSend(command, messageType.CX_XRAY_DATA_STATE_ACK, ncbIndex);
                if (/*no response?*/ reply == null)
                    throw new Exception(traceText + ": no/null response");
                returnedXrayState = reply.Value.XRayDataState;
            }
            catch { throw; }
            traceText += " returns " + returnedXrayState.ToString();    //if tracing ever done
            return returnedXrayState;
        }

        public int GhostSensorCount
        {
            get
            {
                if (/*prepare?*/ _ghostSensorCount < /*minimum*/ 0)
                    try { GhostSensorCount = int.Parse(ConfigurationManager.AppSettings[GhostSensorCountKey], CultureInfo.InvariantCulture); }
                    catch { GhostSensorCount = /*default*/ 0; }
                return _ghostSensorCount;
            }
            private set
            {
                if (/*invalid?*/ (value < /*minimum*/ 0) || (value > /*maximum*/ 1023))
                    throw new ArgumentOutOfRangeException(ClassName + ": " + "GhostSensorCount value (" + value.ToString() + ") must in in the domain [0, 1023]");
                if (value != _ghostSensorCount)
                {
                    _ghostSensorCount = value;
                    try { Logger.LogInfo("Ghost Sensor Count/" + GhostSensorCountKey + " set to " + value.ToString()); }
                    catch { }
                }
            }
        }
        private int _ghostSensorCount = /*invalid, causes preparation*/ int.MinValue;
        public const string GhostSensorCountKey = "GhostSensorCount";

        public int ImageBytesPerPixel
        {
            get
            {
                if (/*prepare?*/ (_imageBytesPerPixel < /*minimum*/ 1) || (_imageBytesPerPixel > /*maximum*/ 4))
                    try { ImageBytesPerPixel = int.Parse(ConfigurationManager.AppSettings[ImageBytesPerPixelKey], CultureInfo.InvariantCulture); }
                    catch { ImageBytesPerPixel = /*default*/ 3; }
                return _imageBytesPerPixel;
            }
            private set
            {
                if (/*invalid?*/ (value < /*minimum*/ 1) || (value > /*maximum*/ 4))
                    throw new ArgumentOutOfRangeException(ClassName + ": " + "ImageBytesPerPixel value (" + value.ToString() + ") must in in the domain [1, 4]");
                if (value != _imageBytesPerPixel)
                {
                    _imageBytesPerPixel = value;
                    try { Logger.LogInfo("Image Bytes per Pixel/" + ImageBytesPerPixelKey + " set to " + value.ToString()); }
                    catch { }
                }
            }
        }
        private int _imageBytesPerPixel = /*invalid, causes preparation*/ int.MinValue;
        public const string ImageBytesPerPixelKey = "BytesPerPixel";

        /// <summary>
        /// Logger references a <see cref="EventLoggerAccess"/> to provide announcement/logging
        /// facilities.</summary>
        [DefaultValue(null)]
        public static EventLoggerAccess Logger { get; private set; }

        /// <summary>
        /// Log Level specifies a <see cref="TraceLevel"/> that, when set to
        /// <see cref="TraceLevel.Verbose"/>, triggers detailed announcements of
        /// activities.</summary>
        /// <remarks>
        /// Due to the nature of <see cref="EventLoggerAccess"/>, log levels other than
        /// <see cref="TraceLevel.Verbose"/> have little effect on logged details.</remarks>
        /// <returns>
        /// <see cref="TraceLevel"/> as used for logging... The default value is
        /// <see cref="LogLevelDEFAULT"/>.</returns>
        /// <value>
        /// The settor's value must be in the <see cref="TraceLevel"/> enumeration domain.
        /// Invalid values and values less than <see cref="TraceLevel.Error"/> are treated as if
        /// they were <see cref="TraceLevel.Error"/>.</value>
        public TraceLevel LogLevel
        {
            get { return _logLevel; }
            set
            {   // If the candidate value is invalid (not a known TraceLevel) or TraceLevel.Off,
                // translate it to TraceLevel.Error. Then acquire it.
                if (/*invalid?*/ !Enum.IsDefined(typeof(TraceLevel), value))
                    value = TraceLevel.Error;
                value = (TraceLevel)Math.Max((int)value, /*no Off*/ (int)TraceLevel.Error);
                if (/*change?*/ value != _logLevel)
                {   // This candidate value represents a change: if appropriate, log it.
                    if (/*Info or more?*/ (value >= TraceLevel.Info) ||
                            (_logLevel >= TraceLevel.Info))
                        try { Logger.LogInfo("LogLevel set to " + value.ToString()); }
                        catch { }
                    _logLevel = value;
                }
            }
        }
        private TraceLevel _logLevel = LogLevelDEFAULT;
        private const TraceLevel LogLevelDEFAULT = TraceLevel.Info;

        /// <summary/>
        public int LogPause
        {
            get { return _logPause.Period; }
            private set { _logPause.Reset(value); }
        }
        private Latency _logPause = new Latency(0);
        private void LogPauseError(string text)
        {
            if (/*OK to log?*/ LogPauseExpired)
            {
                LogPause = (int)Utilities.Within(Utilities.TimeSECOND, 2 * LogPause, 10 * Utilities.TimeMINUTE);
                if (/*empty?*/ string.IsNullOrWhiteSpace(text))
                    text = string.Empty;
                else
                    text += "\r   ";
                text += "logging disabled for " + Utilities.TimeText(LogPause);
                try { Logger.LogError(text); }
                catch { }
            }
        }
        private bool LogPauseExpired { get { return _logPause.Expired; } }

        public int NCBChannelCount(int ncbIndex)
        {
            if ((ncbIndex < 0) || (ncbIndex > _ncbChannelCount.Length))
                throw new IndexOutOfRangeException();
            return _ncbChannelCount[ncbIndex];
        }
        private int[] _ncbChannelCount = new int[] { /*empty*/ };
 
        public int NCBCount
        {
            get
            {
                // On first call, the property's value is invalid. Detect this and, using the
                // configuration, prepare the property. On subsequent calls, the property is
                // already prepared and all that is necessary is to return the value.
                if (/*invalid/unprepared?*/ _ncbCount < 1)
                    try
                    {
                        _ncbCount = int.Parse(ConfigurationManager.AppSettings["NCBCount"], CultureInfo.InvariantCulture);
                    }
                    catch { _ncbCount = 1; }
                    finally
                    {
                        Debug.Assert(Logger != null);
                        Logger.LogInfo("NCB Count set to " + _ncbCount.ToString());
                    }
                return _ncbCount;
            }
        }
        private int _ncbCount = /*invalid; causes preparation*/ int.MinValue;

        private int NCBIndexFromIPAddress(IPAddress address)
        {
            if (/*invalid?*/ address == null)
                throw new ArgumentNullException(ClassName + ": " +
                            "NCBIndexFromIPAddress NCB address argument (address)" +
                            " must not be null");
            byte[] /*byte form address*/ nAB = address.GetAddressBytes();
            IPAddress /*IP address*/ adr;
            IPAddress.TryParse(CommandIpAddress, out adr);
            byte[] /*byte form base NCB address*/ bas = adr.GetAddressBytes();
            int a = (int)nAB[/*last element*/ nAB.Length - 1];
            int b = (int)bas[/*last element*/ bas.Length - 1];
            int /*NCB index (returned)*/ ncb = a - b;
            return ncb;
        }
        private string NCBIndexToIPAddress(int ncbIndex)
        {
            string /*text (returned)*/ txt = IPAddress.None.ToString();
            try
            {
                IPAddress /*IP address*/ adr;
                IPAddress.TryParse(CommandIpAddress, out adr);
                byte[] /*address bytes*/ byt = adr.GetAddressBytes();
                byt[/*last element*/ byt.Length - 1] += (byte)ncbIndex; //overflow not handled
                adr = new IPAddress(byt);
                txt = adr.ToString();
            }
            catch { throw; }
            return txt;
        }

        private string NCBName(int ncbIndex)
        {
            string /*text (returned)*/ txt = string.Empty;
            try
            {
                txt = "NCB[" + ncbIndex.ToString() + "] at " + NCBIndexToIPAddress(ncbIndex) + ":" +
                        CommandIpPort.ToString();
            }
            catch { txt = "unknown NCB[" + ncbIndex.ToString() + "]"; }
            return txt;
        }

        public int PixelsPerColumn
        {
            get
            {
                if (_pixelsPerColumn <= 0)
                    PixelsPerColumn = int.Parse(ConfigurationManager.AppSettings[PixelsPerColumnKey]);
                return _pixelsPerColumn;
            }
            private set
            {
                if (value != _pixelsPerColumn)
                {
                    _pixelsPerColumn = value;
                    Debug.Assert(Logger != null);
                    Logger.LogInfo("Pixels per Column/" + PixelsPerColumnKey + " set to " + value.ToString());
                }
            }
        }
        private int _pixelsPerColumn = /*invalid; forces preparation*/ int.MinValue;
        public const string PixelsPerColumnKey = "PixelsPerColumn";

        public event PropertyChangedEventHandler PropertyChanged;
        private void PropertyOnChange(PropertyChangedEventArgs arguments)
        {
            if (/*run?*/ !_dataGatherEnd.WaitOne(0) &&
                    (/*notify subscribers?*/ PropertyChanged != null))
                try { PropertyChanged(this, arguments); }
                catch { }
        }

        public int PulseDelay
        {
            get
            {
                if (/*prepare?*/ _pulseDelay < 0)
                {
                    try
                    {
                        // Fetch (from the application's configuration), validate and apply the
                        // value. Use the settor so that valiudation and logging occur.
                        PulseDelay = (int)ushort.Parse(ConfigurationManager.AppSettings[PulseDelayKey]);
                    }
                    catch
                    { 
                        // Apply a default value.
                        PulseDelay = /*default*/ 2750 /* 20ns*/;
                    }
                }
                return _pulseDelay;
            }
            private set
            {
                // Validate the value and, if it is different from the current value, apply it and
                // log the change.
                if (/*invalid?*/ (value < 1) || (value > (int)ushort.MaxValue))
                    throw new ArgumentOutOfRangeException(ClassName + ": " +
                            "PulseDelay value (" + value.ToString() +
                            ") must in in the domain [1, " + ushort.MaxValue.ToString() +
                            "] * 20ns");
                if (value != _pulseDelay)
                {
                    // Apply and log the new value.
                    Debug.Assert((value >= 1) && (value <= (int)ushort.MaxValue));
                    _pulseDelay = value;
                    Debug.Assert(Logger != null);
                    Logger.LogInfo("Pulse Delay/" + PulseDelayKey + " set to " +
                            _pulseDelay.ToString() + " * 20ns");
                }
            }
        }
        private int _pulseDelay = /*invalid, causes preparation*/ int.MinValue;
        public const string PulseDelayKey = "NCBDelayXRayPulsePeriod20nsecUnit";

        public BlockingCollection<DataInfo> RawDataCollection
        {
            get { return _rawDataCollection; }
            private set { _rawDataCollection = value; }
        }
        private BlockingCollection<DataInfo> _rawDataCollection = new BlockingCollection<DataInfo>();

        [DefaultValue(ReferenceSensorCount)]
        public int ReferenceCount { get; private set; }
        public const int ReferenceSensorCount = 32;

        public void ResetLineCount()
        {
            string traceText = MethodBase.GetCurrentMethod().Name;
            if (/*multi NCB?*/ NCBCount >= 2)
            {
                Exception /*exception*/ exc = null;
                for (int ix = 0;
                    /*run?*/ !_dataGatherEnd.WaitOne(0) && (ix < NCBCount);
                        ix++)
                {
                    try
                    {
                        CommPacketHeader command = new CommPacketHeader();
                        command.MessageID = messageType.CX_RESET_LINE_COUNT_WITH_ACK;
                        CommPacketHeader? reply =
                                CommandSend(command, messageType.CX_RESET_LINE_COUNT_ACK, ix);
                        if (/*no response?*/ reply == null)
                            throw new Exception(traceText + ": no/null response");
                    }
                    catch (Exception ex)
                    {
                        if (/*first anomaly?*/ exc == null)
                            exc = ex;
                    }
                }
                if (/*anomaly?*/ exc != null)
                    throw exc;
                Logger.LogInfo(traceText + ": all NCBs' line identities reset");
            }
        }

        private void SetConfigurationMode(configurationMode mode, int ncbIndex)
        {
            Debug.Assert(Enum.IsDefined(typeof(configurationMode), mode));
            Debug.Assert((ncbIndex >= 0) && (ncbIndex < NCBCount));
            string traceText = MethodBase.GetCurrentMethod().Name + "(" +
                    mode.ToString() + ", " + ncbIndex.ToString() + ")";
            try
            {
                CommPacketHeader command = new CommPacketHeader();
                command.MessageID = messageType.CX_DEVICE_CONFIG_NORMAL_WITH_ACK;
                command.ConfigurationMode = mode;
                CommPacketHeader? reply =
                        CommandSend(command, messageType.CX_DEVICE_CONFIG_ACK, ncbIndex);
                if (/*no response?*/ reply == null)
                    throw new Exception(traceText + ": no/null response");
                if (reply.Value.ConfigurationMode != mode)
                    throw new Exception(traceText + ": unexpected response (" +
                            reply.Value.ConfigurationMode.ToString() + ")");
            }
            catch { throw; }
        }

        private void SetConfigurationParameters(ushort pulseDelay, ushort signOfLife, int ncbIndex)
        {
            string traceText = MethodBase.GetCurrentMethod().Name + "(" +
                    pulseDelay.ToString() + ", " + signOfLife.ToString() + ", " + ncbIndex.ToString() +
                    ")";
            Debug.Assert(pulseDelay <= ushort.MaxValue);
            Debug.Assert(signOfLife <= ushort.MaxValue);
            Debug.Assert((ncbIndex >= 0) && (ncbIndex < NCBCount));
            try
            {
                CommPacketHeader command = new CommPacketHeader();
                command.PulseDelayPeriod = pulseDelay;
                command.SignOfLifeInterval = signOfLife;
                command.MessageID = messageType.CX_CONFIG_PARAMETERS_NORMAL_WITH_ACK;
                command.ReservedB6toB7 = 0x0000;
                command.ReservedB8toB9 = 0x0000;
                command.ReservedB10toB11 = 0x0000;
                command.ReservedB12toB13 = 0x0000;
                command.ReservedB18toB19 = 0x0000;
                command.ReservedB20toB31 = new byte[12];
                CommPacketHeader? reply =
                        CommandSend(command, messageType.CX_CONFIG_PARAMETERS_ACK, ncbIndex);
                if (/*no response?*/ reply == null)
                    throw new Exception(traceText + ": no/null response");
                if (reply.Value.PulseDelayPeriod != pulseDelay)
                    throw new Exception(traceText + ": unexpected pulse delay response of " +
                            reply.Value.PulseDelayPeriod.ToString());
                if (reply.Value.SignOfLifeInterval != signOfLife)
                    throw new Exception(traceText + ": unexpected sign of life response of " +
                            reply.Value.SignOfLifeInterval.ToString());
            }
            catch { throw; }
        }

#if false
        public void SetConfigParameterToNVM()
        {
            for (int /*NCB index*/ ix = 0; ix < NCBCount; ix++)
            {
                try
                {
                    CommPacketHeader command = new CommPacketHeader();
                    command.MessageID = messageType.CX_CONFIG_PARAMETERS_WRITE_NORMAL_WITH_ACK;
                    CommPacketHeader? reply = CommandSend(command, messageType.CX_CONFIG_PARAMETERS_WRITE_ACK);
                    if (/*no response?*/ reply == null)
                        throw new Exception("no/null response");
                    if ((nvmWriteStatus)reply.Value.NVMWriteStatus != nvmWriteStatus.Success)
                        throw new Exception("failed- response.NVMWriteStatus is " + ((nvmWriteStatus)reply.Value.NVMWriteStatus).ToString());
                }
                catch { throw; }
            }
        }
#endif

        public void SetDataTransferMode(dataTransferMode mode)
        {
            string traceText = MethodBase.GetCurrentMethod().Name + "(" +
                    mode.ToString() + ")";
            for (int /*NCB index*/ ncb = 0; ncb < NCBCount; ncb++)
            {
                try
                {
                    CommPacketHeader command = new CommPacketHeader();
                    command.MessageID = messageType.CX_XRAY_DATA_STATE_NORMAL_WITH_ACK;
                    command.DataTransferMode = mode;
                    CommPacketHeader? reply =
                            CommandSend(command, messageType.CX_XRAY_DATA_STATE_ACK, ncb);
                    if (/*no response?*/ reply == null)
                        throw new Exception(traceText + " " + NCBName(ncb) + ": no/null response");
                    if (reply.Value.DataTransferMode != mode)
                        throw new Exception(traceText + " " + NCBName(ncb) + ": unexpected response (" +
                                reply.Value.DataTransferMode.ToString() + ")");
                }
                catch { throw; }
            }
        }

        public int SignOfLife
        {
            get
            {
                if (/*prepare?*/ _signOfLife == int.MinValue)
                {
                    try
                    {
                        // Fetch (from the application's configuration), validate and apply the
                        // value. Use the settor so that valiudation and logging occur.
                        SignOfLife =
                                (int)ushort.Parse(ConfigurationManager.AppSettings[SignOfLifeKey]);
                    }
                    catch
                    {
                        // Apply a default value.
                        SignOfLife = /*default*/ SignOfLifeDefault;
                    }
                }
                Debug.Assert((_signOfLife >= SignOfLifeMinValue) &&
                        (_signOfLife <= (int)ushort.MaxValue));
                return _signOfLife;
            }
            private set
            {
                // Validate the value and, if it is different from the current value, apply it and
                // log the change.
                if (/*invalid?*/ (value < 1) || (value > (int)ushort.MaxValue))
                    throw new ArgumentOutOfRangeException(ClassName + ": " +
                            "SignOfLife value (" + value.ToString() +
                            ") must in in the domain [" + SignOfLifeMinValue.ToString() +
                            ", " + ushort.MaxValue.ToString() + "]cs}");
                if (value != _signOfLife)
                {
                    // Apply and log the new value.
                    Debug.Assert((value >= SignOfLifeMinValue) && (value <= (int)ushort.MaxValue));
                    _signOfLife = value;
                    Debug.Assert(Logger != null);
                    Logger.LogInfo("Sign of Life timeout/" + SignOfLifeKey + " set to " +
                            _signOfLife.ToString() + "cs (" + SignOfLifeMilliseconds.ToString() +
                            "ms)");
                }
            }
        }
        private int _signOfLife = /*invalid, causes preparation*/ int.MinValue;
        public const int SignOfLifeDefault = /*1s*/ 100 /*cs*/;
        public const string SignOfLifeKey = "NCBSignOfLifeInterval10msecUnit";
        public const int SignOfLifeMinValue = 3 /*cs*/;
        public int /*10 * [1, ushort.MaxValue] ms*/ SignOfLifeMilliseconds
        { get { return /*ms/cs*/ 10 * SignOfLife; } }

        public bool SourcesSynchronized { get; set; }

        /// <summary>
        /// Start the instance.</summary>
        /// <remarks>
        /// This method is written so that it may be called repeatedly, whether or not the instance
        /// is already started.</remarks>
        /// <exception cref="Exception">
        /// The method makes use of multiple supporting methods, any one of which might throw an
        /// exception if an anomaly is detected.</exception>
        /// <example><code>
        /// DetectorsAccess /*instance*/ ins = new DetectorsAccess(</code>loggerReference<code>);
        /// </code>...<code>
        /// ins.Start(); Debug.Assert(ins.CommandIsReady);
        /// </code>...<code>
        /// ins.Stop(); Debug.Assert(!ins.CommandIsReady);
        /// </code>...<code>
        /// ins.Dispose(); ins = null;
        /// </code></example>
        public void Start()
        {
            SourcesSynchronized = false;
            CommandMonitorStart();
            DataGatherStart();
        }

        /// <summary>Stop the instance.</summary>
        /// <remarks>
        /// This method is written so that it may be called repeatedly, whether or not the instance
        /// is already stopped.</remarks>
        /// <exception cref="Exception">
        /// The method is written so that it never throws an exception.</exception>
        public void Stop()
        {
            DataGatherStop();
            CommandMonitorStop();
        }

        private const int Time10SECONDS = /*10 seconds*/ 10 * TimeSECOND;
        private const int TimeMINUTE = /*1 minute*/ 60 * TimeSECOND;
        private const int TimeSECOND = /*1s*/ 10 * TimeTENTH;
        private const int TimeTENTH = /*1/10s*/ 100 /*ms*/;
    }
}