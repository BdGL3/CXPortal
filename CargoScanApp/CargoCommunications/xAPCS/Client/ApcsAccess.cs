#if !MF_FRAMEWORK_VERSION_V4_1

using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using L3.Cargo.Communications.APCS.Common;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.EventsLogger.Client;

namespace L3.Cargo.Communications.APCS.Client
{
    public class ApcsAccess : INotifyPropertyChanged, IDisposable
    {
        public ApcsAccess(EventLoggerAccess log)
        {
            Lgr = log;
            Debug.Assert(Lgr != null);

            _serverAddress = ConfigurationManager.AppSettings["ApcsAddress"];
            _serverPort = int.Parse(ConfigurationManager.AppSettings["ApcsPort"]);
            Connected = false;

            _responseEvent = new AutoResetEvent(false);
            SpeedMsgEvent = new AutoResetEvent(false);

            _signOfLifeTimer = new Timer(new TimerCallback(SignOfLifeCheck), null, Timeout.Infinite, Timeout.Infinite);
            _signOfLifeTimerDueTime = 20000;

            _connectionThread = new Thread(new ThreadStart(ConnectionThreadMethod));
            _connectionThread.Name = "APCS Command Connection Thread";
            _connectionThread.IsBackground = true;

            _responseThread = new Thread(new ThreadStart(ResponseThreadMethod));
            _responseThread.Name = "APCS Command Response Thread";
            _responseThread.IsBackground = true;

            _sendConfigDataThread = new Thread(new ThreadStart(SetConfigurationThreadMethod));
            _sendConfigDataThread.Name = "APCS Send Configuration Data Thread";
            _sendConfigDataThread.IsBackground = true;
        }

        #region Public Members
        public event EventHandler ApcsConfigured;
        public const int ApcsDelay = /*1/2 s*/ 500 /*ms*/;
        public event ApcsUpdateHandler ApcsUpdate;
        public bool Configured = false;
        public event ConnectionStateChangeHandler ConnectionStateUpdate;
        public bool Connected { get; private set; }
        public PulseWidth CurrentPulseWidth { get; set; }

        public void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public uint SignOfLifeSequence
        {
            get { return _signOfLifeSequence; }
            set
            {
                _signOfLifeSequence = value;
                NotifyPropertyChanged("SignOfLifeSequence");
            }
        }

        public AutoResetEvent SpeedMsgEvent;
        #endregion

        #region Public Methods
        public bool SetScanEnergyMode(ScanEnergyMode mode, bool confirm)
        {
            if (/*terminated?*/ _cancelEvent.WaitOne(0))
                return false;
            CommandDefinition.CommandFormat command = ConstructCommand(CommandEnum.ScanMode, BooleanValue.True, ActionEnum.Set, (byte)mode);
            CommandDefinition.CommandFormat? response = SendCommand(command, confirm);
            if (confirm && !ConfirmSetActionResponse(response, CommandEnum.ScanMode, (byte)mode))
                return false;
            return true;
        }

        public bool GetScanEnergyMode(out ScanEnergyMode? mode, bool confirm)
        {
            mode = null;
            if (/*terminated?*/ _cancelEvent.WaitOne(0))
                return false;
            CommandDefinition.CommandFormat command = ConstructCommand(CommandEnum.ScanMode, BooleanValue.False, ActionEnum.Get, 0);
            CommandDefinition.CommandFormat? response = SendCommand(command, confirm);
            if (response != null && response.Value.CommandAck.Command == CommandEnum.ScanMode && response.Value.Action.Action == ActionEnum.Response)
            {
                mode = (ScanEnergyMode)ScanEnergyMode.ToObject(typeof(ScanEnergyMode), response.Value.Action.SubAction);
                return true;
            }
            else
                return false;
        }

        public bool SetOperatingMode(OperatingMode mode, ushort minFreq, ushort maxFreq, bool confirm)
        {
            if (/*terminated?*/ _cancelEvent.WaitOne(0))
                return false;
            CommandDefinition.CommandFormat command = ConstructCommand(CommandEnum.OperatingMode, BooleanValue.True, ActionEnum.Set, (byte)mode);
            BitConverter.GetBytes(minFreq).CopyTo(command.Payload, 0);
            BitConverter.GetBytes(maxFreq).CopyTo(command.Payload, 2);
            CommandDefinition.CommandFormat? response = SendCommand(command, confirm);
            if (confirm && (!ConfirmSetActionResponse(response, CommandEnum.OperatingMode, (byte)mode) ||
                minFreq != BitConverter.ToInt16(response.Value.Payload, 0) || maxFreq != BitConverter.ToInt16(response.Value.Payload, 2)))
                return false;
            return true;
        }

        public bool GetOperatingMode(out OperatingMode? mode, out short? minFreq, out short? maxFreq, bool confirm)
        {
            mode = null; minFreq = null; maxFreq = null;
            if (/*terminated?*/ _cancelEvent.WaitOne(0))
                return false;
            CommandDefinition.CommandFormat command = ConstructCommand(CommandEnum.OperatingMode, BooleanValue.False, ActionEnum.Get, 0);
            CommandDefinition.CommandFormat? response = SendCommand(command, confirm);
            if (response != null && response.Value.CommandAck.Command == CommandEnum.OperatingMode && response.Value.Action.Action == ActionEnum.Response)
            {
                mode = (OperatingMode)OperatingMode.ToObject(typeof(OperatingMode), response.Value.Action.SubAction);
                minFreq = BitConverter.ToInt16(response.Value.Payload, 0);
                maxFreq = BitConverter.ToInt16(response.Value.Payload, 2);
                return true;
            }
            else
                return false;
        }

        public bool SetStaticPulseFrequency(OperatingMode mode, uint freq, bool confirm)
        {
            if (/*terminated?*/ _cancelEvent.WaitOne(0))
                return false;
            CommandDefinition.CommandFormat command = ConstructCommand(CommandEnum.StaticPulseFreq, BooleanValue.True, ActionEnum.Set, (byte)mode);
            BitConverter.GetBytes(freq).CopyTo(command.Payload, 0);
            CommandDefinition.CommandFormat? response = SendCommand(command, confirm);
            if (confirm && (!ConfirmSetActionResponse(response, CommandEnum.StaticPulseFreq, (byte)mode) ||
                freq != BitConverter.ToInt32(response.Value.Payload, 0)))
                return false;
            return true;
        }

        public bool GetStaticPulseFrequency(OperatingMode? selectedMode, out OperatingMode? mode, out int? freq, bool confirm)
        {
            mode = null; freq = null;
            if (/*terminated?*/ _cancelEvent.WaitOne(0))
                return false;
            CommandDefinition.CommandFormat command = ConstructCommand(CommandEnum.StaticPulseFreq, BooleanValue.False, ActionEnum.Get, (byte)selectedMode);
            CommandDefinition.CommandFormat? response = SendCommand(command, confirm);
            if (response != null && response.Value.CommandAck.Command == CommandEnum.StaticPulseFreq && response.Value.Action.Action == ActionEnum.Response)
            {
                mode = (OperatingMode)OperatingMode.ToObject(typeof(OperatingMode), response.Value.Action.SubAction);
                freq = BitConverter.ToInt32(response.Value.Payload, 0);
                return true;
            }
            else
                return false;
        }

        public bool DetectorsTimingEnable(bool /*enabled?*/ Enb)
        {   // Just clean it up and pass it on...
            return SetPWMOutputEnable(
                    Enb ? PWMOutputConfig.OutputEnabled : PWMOutputConfig.OutputDisabled,
                    true);
        }

        public bool GetPWMOutputEnable(
                out PWMOutputConfig? /*enabled?*/ Enb,
                bool /*confirm*/ Cnf)
        {
            CommandDefinition.CommandFormat /*command*/ cmd =
                    ConstructCommand(CommandEnum.PWMOutput, BooleanValue.False, ActionEnum.Get, 0);
            CommandDefinition.CommandFormat? /*response; might be null*/ rsp =
                    SendCommand(cmd, Cnf);
            Enb = null;
            if ((rsp != null) &&
                    (rsp.Value.CommandAck.Command == CommandEnum.PWMOutput) &&
                    (rsp.Value.Action.Action == ActionEnum.Response))
            {
                Enb = (PWMOutputConfig)rsp.Value.Action.SubAction;
                return true;
            }
            else
                return false;
        }

        public bool SetPWMOutputEnable(PWMOutputConfig? /*enabled?*/ Enb, bool /*confirm*/ Cnf)
        {
            string /*trace text*/ trc = MethodBase.GetCurrentMethod().Name + "(" +
                    ((Enb != null) ? Enb.ToString() : "null") + ", " + Cnf.ToString() + ")";
            bool /*status (returned)*/ sts = false;
            try
            {
                CommandDefinition.CommandFormat /*command */ cmd = ConstructCommand(
                        CommandEnum.PWMOutput,
                        BooleanValue.True,
                        ActionEnum.Set,
                        (byte)Enb);
                CommandDefinition.CommandFormat? /*response*/ rsp = SendCommand(cmd, Cnf);
                sts = !Cnf || ConfirmSetActionResponse(rsp, CommandEnum.PWMOutput, (byte)Enb);
            }
            catch (Exception ex) { Lgr.LogError(ex); }
            return sts;
        }

        public bool SetAdaptiveModeTriggerRatio(OperatingMode mode, float ratio, bool confirm)
        {
            if (/*terminated?*/ _cancelEvent.WaitOne(0))
                return false;
            CommandDefinition.CommandFormat command = ConstructCommand(CommandEnum.AdaptiveModeToTrigRatio, BooleanValue.True, ActionEnum.Set, (byte)mode);
            BitConverter.GetBytes(ratio).CopyTo(command.Payload, 0);
            CommandDefinition.CommandFormat? response = SendCommand(command, confirm);
            if (confirm && (!ConfirmSetActionResponse(response, CommandEnum.AdaptiveModeToTrigRatio, (byte)mode) ||
                ratio != BitConverter.ToSingle(response.Value.Payload, 0)))
                return false;
            return true;
        }

        public bool GetAdaptiveModeTriggerRatio(OperatingMode? selectedMode, out OperatingMode? mode, out float? ratio, bool confirm)
        {
            mode = null; ratio = null;
            if (/*terminated?*/ _cancelEvent.WaitOne(0))
                return false;
            CommandDefinition.CommandFormat command = ConstructCommand(CommandEnum.AdaptiveModeToTrigRatio, BooleanValue.False, ActionEnum.Get, (byte)selectedMode);
            CommandDefinition.CommandFormat? response = SendCommand(command, confirm);
            if (response != null && response.Value.CommandAck.Command == CommandEnum.AdaptiveModeToTrigRatio && response.Value.Action.Action == ActionEnum.Response)
            {
                mode = (OperatingMode)OperatingMode.ToObject(typeof(OperatingMode), response.Value.Action.SubAction);
                ratio = BitConverter.ToSingle(response.Value.Payload, 0);
                return true;
            }
            else
                return false;
        }

        public bool SetAdaptiveSpeedFeedbackConfiguration(AdaptiveSpeedFeedbackConfig config, float freq, bool confirm)
        {
            if (/*terminated?*/ _cancelEvent.WaitOne(0))
                return false;
            CommandDefinition.CommandFormat command = ConstructCommand(CommandEnum.AdaptiveSpeedFeedbackConfig, BooleanValue.True, ActionEnum.Set, (byte)config);
            BitConverter.GetBytes(freq).CopyTo(command.Payload, 0);
            CommandDefinition.CommandFormat? response = SendCommand(command, confirm);
            if (confirm && (!ConfirmSetActionResponse(response, CommandEnum.AdaptiveSpeedFeedbackConfig, (byte)config) ||
                freq != BitConverter.ToSingle(response.Value.Payload, 0)))
                return false;
            return true;
        }

        public bool GetAdaptiveSpeedFeedbackConfiguration(out AdaptiveSpeedFeedbackConfig? config, out float? freq, bool confirm)
        {
            config = null; freq = null;
            if (/*terminated?*/ _cancelEvent.WaitOne(0))
                return false;
            CommandDefinition.CommandFormat command = ConstructCommand(CommandEnum.AdaptiveSpeedFeedbackConfig, BooleanValue.False, ActionEnum.Get, 0);
            CommandDefinition.CommandFormat? response = SendCommand(command, confirm);
            if (response != null && response.Value.CommandAck.Command == CommandEnum.AdaptiveSpeedFeedbackConfig && response.Value.Action.Action == ActionEnum.Response)
            {
                config = (AdaptiveSpeedFeedbackConfig)OperatingMode.ToObject(typeof(AdaptiveSpeedFeedbackConfig), response.Value.Action.SubAction);
                freq = BitConverter.ToSingle(response.Value.Payload, 0);
                return true;
            }
            else
                return false;
        }

        public bool SetConfigPulseWidth(PulseWidth? pulseWidth, float microseconds, bool confirm)
        {
            if (/*terminated?*/ _cancelEvent.WaitOne(0))
                return false;
            CommandDefinition.CommandFormat command = ConstructCommand(CommandEnum.ConfigPulseWidth, BooleanValue.True, ActionEnum.Set, (byte)pulseWidth);
            BitConverter.GetBytes(microseconds).CopyTo(command.Payload, 0);
            CommandDefinition.CommandFormat? response = SendCommand(command, confirm);
            if (confirm && (!ConfirmSetActionResponse(response, CommandEnum.ConfigPulseWidth, (byte)pulseWidth) ||
                microseconds != BitConverter.ToSingle(response.Value.Payload, 0)))
                return false;
            return true;
        }

        public bool GetConfigPulseWidth(PulseWidth? pulseWidth, out float? time, bool confirm)
        {
            time = null;
            if (/*terminated?*/ _cancelEvent.WaitOne(0))
                return false;
            CommandDefinition.CommandFormat command = ConstructCommand(CommandEnum.ConfigPulseWidth, BooleanValue.True, ActionEnum.Get, (byte)pulseWidth);
            CommandDefinition.CommandFormat? response = SendCommand(command, confirm);
            if (response != null)
            {
                time = BitConverter.ToSingle(response.Value.Payload, 0);
                return true;
            }
            else
                return false;
        }

        public bool SetCurrentPulseWidth(PulseWidth pulseWidth, bool confirm)
        {
            if (/*terminated?*/ _cancelEvent.WaitOne(0))
                return false;
            CommandDefinition.CommandFormat command = ConstructCommand(CommandEnum.PulseWidth, BooleanValue.True, ActionEnum.Set, (byte)pulseWidth);
            CommandDefinition.CommandFormat? response = SendCommand(command, confirm);
            if (confirm && !ConfirmSetActionResponse(response, CommandEnum.PulseWidth, (byte)pulseWidth))
                return false;
            CurrentPulseWidth = pulseWidth;
            return true;
        }

        public bool GetCurrentPulseWidth(out PulseWidth? width, bool confirm)
        {
            width = null;
            if (/*terminated?*/ _cancelEvent.WaitOne(0))
                return false;
            CommandDefinition.CommandFormat command = ConstructCommand(CommandEnum.PulseWidth, BooleanValue.False, ActionEnum.Get, 0);
            CommandDefinition.CommandFormat? response = SendCommand(command, confirm);
            if (response != null && response.Value.CommandAck.Command == CommandEnum.PulseWidth && response.Value.Action.Action == ActionEnum.Response)
            {
                CurrentPulseWidth = (PulseWidth)PulseWidth.ToObject(typeof(PulseWidth), response.Value.Action.SubAction);
                width = CurrentPulseWidth;
                return true;
            }
            else
                return false;
        }

        public bool SetPWMOutput(PWMOutputConfig? config, bool confirm)
        {
            if (/*terminated?*/ _cancelEvent.WaitOne(0))
                return false;
            CommandDefinition.CommandFormat command = ConstructCommand(CommandEnum.PWMOutput, BooleanValue.True, ActionEnum.Set, (byte)config);
            CommandDefinition.CommandFormat? response = SendCommand(command, confirm);
            if (confirm && (!ConfirmSetActionResponse(response, CommandEnum.PWMOutput, (byte)config)))
                return false;
            return true;
        }

        public bool GetPWMOutput(out PWMOutputConfig? config, bool confirm)
        {
            config = null;
            if (/*terminated?*/ _cancelEvent.WaitOne(0))
                return false;
            CommandDefinition.CommandFormat command = ConstructCommand(CommandEnum.PWMOutput, BooleanValue.False, ActionEnum.Get, 0);
            CommandDefinition.CommandFormat? response = SendCommand(command, confirm);
            if (response != null && response.Value.CommandAck.Command == CommandEnum.PWMOutput && response.Value.Action.Action == ActionEnum.Response)
            {
                config = (PWMOutputConfig)response.Value.Action.SubAction;
                return true;
            }
            else
                return false;
        }

        public void ResetBoard()
        {
            if (/*terminated?*/ _cancelEvent.WaitOne(0))
                return;
            SendCommand(ConstructCommand(CommandEnum.ResetBoard, BooleanValue.False, ActionEnum.Set, 0), false);
        }

        public void Connect()
        {
            if (!_connectionThread.IsAlive)
                _connectionThread.Start();
            if (!_responseThread.IsAlive)
                _responseThread.Start();
            if (!_sendConfigDataThread.IsAlive)
                _sendConfigDataThread.Start();
        }

        public void Disconnect()
        {
            CloseConnection();
        }

        public void Dispose()
        {
            _cancel.Cancel();
            _cancelEvent.Set();

            if (_signOfLifeTimer != null)
                _signOfLifeTimer.Dispose();
            _signOfLifeTimer = null;

            if (_sendConfigDataThread != null)
                if (_sendConfigDataThread.IsAlive)
#if true
                    _sendConfigDataThread.Join(/*1s*/ 1000 /*ms*/);
#else
                    if (!_sendConfigDataThread.Join(ApcsDelay))
                        ;_sendConfigDataThread.Abort();
#endif
            _sendConfigDataThread = null;

            if (_responseThread != null)
                if (_responseThread.IsAlive)
#if true
                    _responseThread.Join(/*1s*/ 1000 /*ms*/);
#else
                    if (!_responseThread.Join(ApcsDelay))
                        _responseThread.Abort();
#endif
            _responseThread = null;

            if (_connectionThread != null)
                if (_connectionThread.IsAlive)
#if true
                    _connectionThread.Join(/*1s*/ 1000 /*ms*/);
#else
                    if (!_connectionThread.Join(ApcsDelay))
                        _connectionThread.Abort();
#endif
            _connectionThread = null;

            Disconnect();

            if (_responseEvent != null)
                _responseEvent.Dispose();
            _responseEvent = null;

            Debug.Assert(Lgr != null);
            Lgr.LogInfo("ApcsAccess.Dispose");
            Lgr = null;
        }

        public OperatingMode GetAdaptiveSpeedMode() { return _speedMsgOpMode; }

        public float GetAdaptiveSpeed() { return _adaptiveSpeed; }
        #endregion

        #region Private Members
        private float _adaptiveSpeed = 0.0f;
        private int _apcsCommandTimeout = 3000;
        private CancellationTokenSource _cancel = new CancellationTokenSource();
        private ManualResetEvent _cancelEvent = new ManualResetEvent(false);    //MSVS2010 doesn't support _cancel.WaitHandle.WaitOne
        private CommandDefinition.CommandFormat _commandResponse;
        private Thread _connectionThread;
        public EventLoggerAccess Lgr { get; set; }
        private NetworkStream _networkStream;
        private AutoResetEvent _responseEvent;
        private Thread _responseThread;
        private Thread _sendConfigDataThread;
        private string _serverAddress;
        private int _serverPort;
        private uint _signOfLifeSequence = 0;
        private Timer _signOfLifeTimer;
        private int _signOfLifeTimerDueTime;
        private OperatingMode _speedMsgOpMode;
        private TcpClient _tcpClient;
        private const int _sendCommandRetrys = 3;
        #endregion Private Members

        #region Private Methods
        private void CloseConnection()
        {
            if (_networkStream != null)
            {
                _networkStream.Flush();
                _networkStream.Dispose();
            }

            if (_tcpClient != null)
            {
                _tcpClient.Close();
                _tcpClient = null;
            }

            if (Connected)
            {
                Debug.Assert(Lgr != null);
                Lgr.LogInfo("APCS command link disconnected");
            }

            if (_signOfLifeTimer != null)
            {
                _signOfLifeTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }

            Connected = false;

            BeginConnectionUpdate();
        }

        private void ConnectionThreadMethod()
        {
            Debug.Assert((_connectionThread != null) &&
                    !string.IsNullOrWhiteSpace(_connectionThread.Name));
            string /*thread name*/ nam = _connectionThread.Name;
            Debug.Assert(Lgr != null);
            try
            {
                Lgr.LogInfo(nam + " started");
                int /*delay*/ dly = ApcsDelay;
                do
                {
                    try
                    {
                        if (_tcpClient == null)
                        {
                            _tcpClient = new TcpClient();
                            Lgr.LogInfo(nam + ": client created");
                        }
                        if (!_tcpClient.Connected)
                        {
                            _tcpClient.Connect(_serverAddress, _serverPort);
                            _networkStream = _tcpClient.GetStream();
                            _networkStream.ReadTimeout = _signOfLifeTimerDueTime;
                            Connected = true;
                            BeginConnectionUpdate();
                            dly = ApcsDelay;    //return to full speed
                        }
                    }
                    catch (Exception ex)
                    {
                        Connected = false;
                        CloseConnection();
                        if (/*not terminated?*/ !_cancelEvent.WaitOne(0))
                        {
                            dly = Math.Min(/*30s*/ 30000 /*ms*/, dly + dly + 1);
                            Lgr.LogError(nam + ": unable to connect to APCS at " + _serverAddress +
                                    ":" + _serverPort.ToString() + "; reattempting connection in " +
                                    ((dly + 999) / 1000).ToString() + "s...");
                            Lgr.LogError(ex);
                        }
                    }
                }
                while (!_cancelEvent.WaitOne(dly));
            }
            catch (Exception /*exception*/ ex)
            {
                if (/*not terminated?*/ !_cancelEvent.WaitOne(0))
                {
                    Lgr.LogError("catastrophic error (details follow) for " + nam);
                    Lgr.LogError(ex);
                }
            }
            finally
            {
                try { Lgr.LogInfo(nam + " ended"); }
                catch { /*suppress*/ }
            }
        }

        private void BeginConnectionUpdate()
        {
            Thread connectUpdate = new Thread(new ThreadStart(ConnectionUpdateThreadMethod));
            connectUpdate.IsBackground = true;
            connectUpdate.Start();
        }

        private void ConnectionUpdateThreadMethod()
        {
            try
            {
                if (ConnectionStateUpdate != null)
                    ConnectionStateUpdate(Connected);
                Debug.Assert(Lgr != null);
                Lgr.LogInfo("APCS command link " + (Connected? string.Empty : "dis") + "connected");
            }
            catch (Exception ex)
            {
                if (/*not terminated?*/ !_cancelEvent.WaitOne(0))
                {
                    Debug.Assert(Lgr != null);
                    Lgr.LogError(ex);
                }
            }
        }

        private void ResponseThreadMethod()
        {
            int length = Marshal.SizeOf(typeof(CommandDefinition.CommandFormat));
            do
            {
                while (Connected)
                {
                    try
                    {
                        byte[] response = new byte[length];
                        int dataRead = _networkStream.Read(response, 0, response.Length);
                        if (dataRead > 0)
                        {
                            CommandDefinition.CommandFormat commandResponse = _commandResponse.Deserialize(response);
                            if (commandResponse.CommandAck.Command == CommandEnum.SignOfLife)
                            {
                                SignOfLifeSequence = BitConverter.ToUInt32(commandResponse.Payload, 0);
                                if (_signOfLifeTimer != null)
                                    _signOfLifeTimer.Change(_signOfLifeTimerDueTime, Timeout.Infinite);
                            }
                            else if (commandResponse.CommandAck.Command == CommandEnum.AdaptiveModeSpeed)
                            {
                                _adaptiveSpeed = BitConverter.ToSingle(commandResponse.Payload, 0);
                                _speedMsgOpMode = (OperatingMode)commandResponse.Action.SubAction;
                                SpeedMsgEvent.Set();
                            }
                            else
                            {
                                _commandResponse = commandResponse;
                                _responseEvent.Set();
                                if (ApcsUpdate != null)
                                    ApcsUpdate(commandResponse.CommandAck.Command, commandResponse.Action.Action, commandResponse.Action.SubAction, commandResponse.Payload);
                            }
                        }
                        else
                            throw new Exception("Data read from APCS has size 0.  Restarting Connection.");
                    }
                    catch (Exception ex)
                    {
                        if (/*not terminated?*/ !_cancelEvent.WaitOne(0))
                        {
                            Debug.Assert(Lgr != null);
                            Lgr.LogError(ex);
                        }
                        if (_tcpClient != null && _tcpClient.Connected)
                        {
                            //CloseConnection();
                            _tcpClient = null;
                        }
                    }
                }
            }
            while (!_cancelEvent.WaitOne(ApcsDelay));
        }

        private void SignOfLifeCheck(Object stubObject)
        {
            Debug.Assert(Lgr != null);
            Lgr.LogError("APCS SignOfLifeCheck() timer expired.");
            //Disconnect();
        }

        private void SetConfigurationThreadMethod()
        {
            float pulseWidth1 = float.Parse(ConfigurationManager.AppSettings["PulseWidth1Microseconds"], CultureInfo.InvariantCulture);
            float pulseWidth2 = float.Parse(ConfigurationManager.AppSettings["PulseWidth2Microseconds"], CultureInfo.InvariantCulture);
            float pulseWidth3 = float.Parse(ConfigurationManager.AppSettings["PulseWidth3Microseconds"], CultureInfo.InvariantCulture);
            float pulseWidth4 = float.Parse(ConfigurationManager.AppSettings["PulseWidth4Microseconds"], CultureInfo.InvariantCulture);
            uint dualPulseFrequency = uint.Parse(ConfigurationManager.AppSettings["DualPulseFrequency"]);

            do
            {
                try
                {
                    if (Connected)
                    {
                        SetConfigPulseWidth(PulseWidth.PulseWidth1, pulseWidth1, true);
                        SetConfigPulseWidth(PulseWidth.PulseWidth2, pulseWidth2, true);
                        SetConfigPulseWidth(PulseWidth.PulseWidth3, pulseWidth3, true);
                        SetConfigPulseWidth(PulseWidth.PulseWidth4, pulseWidth4, true);
                        SetStaticPulseFrequency(OperatingMode.NonAdaptiveMobile, dualPulseFrequency, true);
                        SetCurrentPulseWidth(PulseWidth.PulseWidth1, true);
                        Configured = true;
                        if (ApcsConfigured != null)
                        {
                            ApcsConfigured(this, new EventArgs());
                        }
                        break;
                    }
                }
                catch (Exception ex)
                {
                    if (/*not terminated?*/ !_cancelEvent.WaitOne(0))
                    {
                        Debug.Assert(Lgr != null);
                        Lgr.LogError(ex);
                    }
                }
            }
            while (!_cancelEvent.WaitOne(ApcsDelay));
        }

        private CommandDefinition.CommandFormat? SendCommand(CommandDefinition.CommandFormat command, bool expectingReply)
        {
            CommandDefinition.CommandFormat? response = null;
            try
            {
                if (Connected)
                {
                    byte[] cmd = command.Serialize();
                    _responseEvent.Reset();
                    _networkStream.Write(cmd, 0, cmd.Length);
                    for (int i = 0; i < _sendCommandRetrys; i++)
                    {
                        if (_responseEvent.WaitOne(_apcsCommandTimeout))
                        {
                            if (_commandResponse.CommandAck.Command == command.CommandAck.Command &&
                                    _commandResponse.Action.Action == ActionEnum.Response)
                            {
                                response = _commandResponse;
                                _responseEvent.Reset();
                                break;
                            }
                        }
                    }
                }
#if false
                else
                    throw new Exception("SendCommand called while not connected to APCS.");
#endif
            }
            catch (Exception ex)
            {
                if (/*not terminated?*/ !_cancelEvent.WaitOne(0))
                {
                    Debug.Assert(Lgr != null);
                    Lgr.LogError(ex);
                }
            }
            return response;
        }

        private CommandDefinition.CommandFormat ConstructCommand(CommandEnum command, BooleanValue ackReq, ActionEnum action, byte subAction)
        {
            CommandDefinition.CommandAckStruct cmdAck = new CommandDefinition.CommandAckStruct(command, ackReq, BooleanValue.False);
            CommandDefinition.ActionStruct a = new CommandDefinition.ActionStruct(action, subAction);
            CommandDefinition.CommandFormat cmd = new CommandDefinition.CommandFormat(cmdAck.CommandWithAck, a.ActionAndSubAction);
            return cmd;
        }

        private bool ConfirmSetActionResponse(CommandDefinition.CommandFormat? response, CommandEnum command, byte subAction)
        {
            return (response != null &&
                    response.Value.CommandAck.Command == command &&
                    response.Value.Action.Action == ActionEnum.Response &&
                    response.Value.CommandAck.IsAck == BooleanValue.True &&
                    response.Value.Action.SubAction == subAction);
        }
        #endregion Private Methods
    }
}
#endif