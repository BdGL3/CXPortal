using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Net;
using GHIElectronics.NETMF.Net.NetworkInformation;
using GHIElectronics.NETMF.Net.Sockets;

using L3.Cargo.Communications.APCS.Common;

namespace L3.Cargo.APCS
{
    public class NetServer: IDisposable
    {
        public bool IsConnected { get { return _sessionSocket != null; } }

        public void Dispose()
        {
            Dispose(!IsDisposed);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool unused)
        {
            if (IsDisposed)
            {
                ProcessCommandDispose( /*ProcessCommandEvent == null*/ );
                SignOfLifeStop( /*_signOfLifeTimer == null*/ );
                ServerStop( /*_server* == false or null*/ );
                IsDisposed = true;
            }
        }

        public bool IsDisposed { get; private set; }

        ~NetServer() { Dispose(true); }
        public NetServer() { }

        private Socket _listenerSocket;
        private void ListenerClose()
        {
            try
            {
                if (/*avoid debug first try exceptions; exists?*/ _listenerSocket != null)
                    _listenerSocket.Close();
            }
            catch { }
            finally { _listenerSocket = null; }
        }

        private void ProcessCommand(CommandDefinition.CommandFormat command)
        {
            if (ProcessCommandEvent != null)
                try { ProcessCommandEvent(command); }
                catch { /*nobody to report to*/ }
            if (command.CommandAck.AckRequired == BooleanValue.True)
                SendAck(command);
        }
        public event ProcessCommandEventHandler ProcessCommandEvent;
        public delegate void ProcessCommandEventHandler(CommandDefinition.CommandFormat command);
        private void ProcessCommandDispose() { ProcessCommandEvent = null; }

        private CommandDefinition.CommandFormat ReplyCreate(CommandEnum command, BooleanValue isAck, ActionEnum action, byte subAction)
        {
            CommandDefinition.CommandAckStruct /*can't name it command*/ cmd = new CommandDefinition.CommandAckStruct(command, BooleanValue.False, isAck);
            CommandDefinition.ActionStruct /*can't name it action*/ act = new CommandDefinition.ActionStruct(ActionEnum.Response, subAction);
            CommandDefinition.CommandFormat packet = new CommandDefinition.CommandFormat(cmd, act);
            return packet;
        }
        private CommandDefinition.CommandFormat ReplyCreate(CommandEnum command, BooleanValue isAck, ActionEnum action, byte subAction, byte[] data)
        {
            CommandDefinition.CommandAckStruct /*can't name it command*/ cmd = new CommandDefinition.CommandAckStruct(command, BooleanValue.False, isAck);
            CommandDefinition.ActionStruct /*can't name it action*/ act = new CommandDefinition.ActionStruct(ActionEnum.Response, subAction);
            CommandDefinition.CommandFormat packet = new CommandDefinition.CommandFormat(cmd, act, data);
            return packet;
        }
        private bool ReplySend(CommandDefinition.CommandFormat reply)
        {
            try { ReplySendBase(reply); }
            catch { return false; }
            return true;
        }
        private void ReplySendBase(CommandDefinition.CommandFormat reply)
        {
            try
            {
                lock (_sessionSocket)
                {
                    byte[] buffer = reply.Serialize();
                    if (_sessionSocket.Send(buffer) < 1)
                        throw new Exception();
                }
            }
            catch
            {
                SessionClose( /*_sessionSocket == null*/ ); /*make ServerAgent reconnect*/
                throw;
            }
        }

        private void SendAck(CommandDefinition.CommandFormat command)
        {
            CommandDefinition.CommandFormat /*can't name it command*/ cmd = new CommandDefinition.CommandFormat();
            cmd.CommandAck.Command = command.CommandAck.Command;
            cmd.CommandAck.IsAck = BooleanValue.True;
            cmd.Action.Action = ActionEnum.Response;
            cmd.Action.SubAction = command.Action.SubAction;
            cmd.Payload = command.Payload;
            ReplySend(cmd);
        }        

        public void SendAdaptiveModeSpeed(float speed, OperatingMode mode)
        {
            byte[] data = BitConverter.GetBytes(speed);
            CommandDefinition.CommandFormat packet = ReplyCreate(CommandEnum.AdaptiveModeSpeed, BooleanValue.False, ActionEnum.UnsolicitedMsg, (byte)mode, data);
            ReplySend(packet);
        }

        public void SendAdaptiveModeToTrigRatioResponse(OperatingMode mode, float ratio)
        {
            byte[] data = BitConverter.GetBytes(ratio);
            CommandDefinition.CommandFormat packet = ReplyCreate(CommandEnum.AdaptiveModeToTrigRatio, BooleanValue.True, ActionEnum.Response, (byte)mode, data);
            ReplySend(packet);
        }

        public void SendAdaptiveSpeedFeedbackConfigResponse(AdaptiveSpeedFeedbackConfig mode, float frequency)
        {
            //everton byte[] f = Reflection.Serialize(freq, typeof(float));
            byte[] data = BitConverter.GetBytes(frequency);
            CommandDefinition.CommandFormat packet = ReplyCreate(CommandEnum.AdaptiveSpeedFeedbackConfig, BooleanValue.True, ActionEnum.Response, (byte)mode, data);
            ReplySend(packet);
        }

        public void SendCANMessage(byte[] message)
        {
            CommandDefinition.CommandFormat packet = ReplyCreate(CommandEnum.CANMessage, BooleanValue.False, ActionEnum.UnsolicitedMsg, 0, message);
            ReplySend(packet);
        }

        public void SendOperatingModeResponse(OperatingMode mode, short minimum, short maximum)
        {
            byte[] frqMax = BitConverter.GetBytes(maximum);
            byte[] frqMin = BitConverter.GetBytes(minimum);

            byte[] data = new byte[frqMin.Length + frqMax.Length];
            frqMax.CopyTo(data, frqMin.Length);
            frqMin.CopyTo(data, 0);
            CommandDefinition.CommandFormat packet = ReplyCreate(CommandEnum.OperatingMode, BooleanValue.True, ActionEnum.Response, (byte)mode, data);
            ReplySend(packet);
        }

        public void SendPulseWidthConfigResponse(PulseWidth width, float time)
        {
            byte[] data = BitConverter.GetBytes(time);
            CommandDefinition.CommandFormat packet = ReplyCreate(CommandEnum.ConfigPulseWidth, BooleanValue.True, ActionEnum.Response, (byte)width, data);
            ReplySend(packet);
        }

        public void SendPulseWidthResponse(PulseWidth width)
        {
            CommandDefinition.CommandFormat packet = ReplyCreate(CommandEnum.PulseWidth, BooleanValue.True, ActionEnum.Response, (byte)width);
            ReplySend(packet);
        }

        public void SendPWMOutputStatus(PWMOutputConfig status)
        {
            CommandDefinition.CommandFormat packet = ReplyCreate(CommandEnum.PWMOutput, BooleanValue.True, ActionEnum.Response, (byte)status);
            ReplySend(packet);
        }

        public void SendScanModeResponse(ScanEnergyMode mode)
        {
            CommandDefinition.CommandFormat packet = ReplyCreate(CommandEnum.ScanMode, BooleanValue.True, ActionEnum.Response, (byte)mode);
            ReplySend(packet);
        }

        public void SendSignOfLife(uint sequence)
        {
            try
            {
                byte[] data = BitConverter.GetBytes(sequence);
                CommandDefinition.CommandFormat packet = ReplyCreate(CommandEnum.SignOfLife, BooleanValue.False, ActionEnum.UnsolicitedMsg, 0, data);
                ReplySend(packet);
            }
            catch { /*suppress*/ }
        }

        public void SendStaticPulseFreqResponse(OperatingMode mode, int frequency)
        {
            byte[] data = BitConverter.GetBytes(frequency);
            CommandDefinition.CommandFormat packet = ReplyCreate(CommandEnum.StaticPulseFreq, BooleanValue.True, ActionEnum.Response, (byte)mode, data);
            ReplySend(packet);
        }

        private bool _serverTerminate;
        private Thread _serverThread;
        private byte[] ServerAddress = { 192, 168, 0, 90 };
        private byte[] ServerAddressLocal = { 127, 0, 0, 1 };
        private void ServerAgent()
        {
            while (!_serverTerminate)
                try
                {
                    WIZnet_W5100.Enable(SPI.SPI_module.SPI1, (Cpu.Pin)FEZ_Pin.Digital.Di10, (Cpu.Pin)FEZ_Pin.Digital.Di7, true);
                    NetworkInterface.EnableStaticIP(ServerAddress, ServerSubnet, ServerGateway, ServerMac);
                    NetworkInterface.EnableStaticDns(ServerGateway);
                    try
                    {
                        ListenerClose( /*_listenerSocket == null*/ ); /*just to ensure tidiness*/
                        _listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        try
                        {
                            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, ServerPort);
                            _listenerSocket.Bind(endPoint);
                            _listenerSocket.Listen(1); /*blocks...*/
                            SessionClose( /*_sessionSocket == null*/ ); /*just to ensure tidiness*/
                            using (_sessionSocket = _listenerSocket.Accept()) /*using overkill, but what th' hey...*/
                                try
                                {
                                    SignOfLifeStart( /*_signOfLifeTimer != null*/ );
                                    try
                                    {
                                        byte[] buffer = new byte[/*extra for safety*/ 2 * CommandDefinition.PacketSize];
                                        while (/*read OK?*/_sessionSocket.Receive(buffer, buffer.Length, SocketFlags.None) >= 1 /*blocks...*/)
                                        {
                                            CommandDefinition.CommandFormat command = new CommandDefinition.CommandFormat();
                                            command = command.Deserialize(buffer);
                                            ProcessCommand(command);
                                        }
                                    }
                                    catch { SendSignOfLife(uint.MaxValue - 4); }
                                    finally { SignOfLifeStop( /*_signOfLifeTimer == null*/ ); }
                                }
                                catch { SendSignOfLife(uint.MaxValue -3); }
                                finally { SessionClose( /*_sessionSocket == null*/ ); }
                        }
                        catch { SendSignOfLife(uint.MaxValue - 2); }
                        finally { ListenerClose( /*_listenerSocket == null*/ ); }
                    }
                    catch { SendSignOfLife(uint.MaxValue - 1); }
                    finally { WIZnet_W5100.ReintializeNetworking(); }
                }
                catch { SendSignOfLife(uint.MaxValue); }
                finally
                {
                    Thread.Sleep(/*1s*/ 1000 /*ms*/ );
                    Program.ResetBoard();
                }
        }
        private byte[] ServerGateway = { 192, 168, 0, 1 };
        private byte[] ServerMac = { 0x00, 0x26, 0x1C, 0x7B, 0x29, 0xE8 };
        public const int ServerPort = 2730;
        private void ServerStart()
        {
            if ((_serverThread == null) || !_serverThread.IsAlive)
            {
                ServerStop();
                _serverThread = new Thread(new ThreadStart(ServerAgent));
                _serverTerminate = false;
                _serverThread.Start();
            }
        }
        private byte[] ServerSubnet = { 255, 255, 255, 0 };
        private void ServerStop()
        {
            // Tell the server agent to terminate.
            _serverTerminate = true;

            // In case the server is stuck inside of a Receive, dispose of the communications
            // socket, aborting the reception and allowing the server to recognize the termination
            // request.
            SessionClose( /*_sessionSocket == null*/ );

            // In case the server is stuck inside an Accept or Listen, dispose of the listener
            // socket, aborting the acceptance or listening and allowing the server to recognize
            // the termination request.
            ListenerClose( /*_listenerSocket == null*/ );

            // Witness (Join) the server's termination.
            try
            {
                if (/*avoid debug first try exceptions; exists?*/ _serverThread != null)
                    if (_serverThread.IsAlive)
                        _serverThread.Join(/*3s*/ 3000 /*ms*/);
            }
            catch { /*nobody to report to*/ }
            finally { _serverThread = null; }
        }

        private Socket _sessionSocket;
        private void SessionClose()
        {
            try
            {
                if (/*avoid debug first try exceptions; exists?*/ _sessionSocket != null)
                    _sessionSocket.Close();
            }
            catch { /*nobody to report to*/ }
            finally { _sessionSocket = null; }
        }

        private uint _signOfLifeSequence;
        private Timer _signOfLifeTimer;
        private void SignOfLifeAgent(object unused)
        {
            SendSignOfLife(SignOfLifeValue);
            _signOfLifeSequence++;
        }
        public const int SignOfLifePeriod = /*1s*/ 1000 /*ms*/;
        private void SignOfLifeStart()
        {
            SignOfLifeStop( /*_signOfLifeTimer == null*/ );
            _signOfLifeTimer = new Timer(new TimerCallback(SignOfLifeAgent), null, SignOfLifePeriod, SignOfLifePeriod);
        }
        private void SignOfLifeStop()
        {
            try
            {
                if (/*avoid debug first try exceptions; exists?*/ _signOfLifeTimer != null)
                    _signOfLifeTimer.Dispose();
            }
            catch { /*nobody to report to*/ }
            finally { _signOfLifeTimer = null; }
        }
        private uint SignOfLifeValue
        {
            get
            {
                // Encode the build identity (major, minor, build and revision) as four (4) high
                // (even if not the highest) order, decimal digits to precede a trimmed sign of
                // life sequence: M m B R 0 ss
                // [Major] [minor] [Build] [Revision] [0] [low order digits of Sequence].
                Version revision = new Version(0, 0);
                revision = Assembly.GetAssembly(revision.GetType()).GetName().Version;
                uint value = (uint)(1000000 * (revision.Major % 10));
                value += (uint)(100000 * (revision.Minor % 10));
                value += (uint)(10000 * (revision.Build % 10));
                value += (uint)(1000 * (revision.Revision % 10));
                value += _signOfLifeSequence % 100;
                return value;
            }
        }

        public void Start()
        {
            IsDisposed = false;
            ServerStart();
        }
    }
}
