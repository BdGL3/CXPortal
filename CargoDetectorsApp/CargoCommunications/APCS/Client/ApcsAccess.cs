#if !MF_FRAMEWORK_VERSION_V4_1

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

using L3.Cargo.Communications.APCS.Common;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.EventsLogger.Client;

namespace L3.Cargo.Communications.APCS.Client
{
    /// <summary>APCS Access provides a pathway to the APCS.</summary>
    /// <remarks>This class is a singleton: only one instance may exist at a time.</remarks>
    /// <example><code>
    /// ApcsAccess apcsInstance = new <see cref="ApcsAccess.ApcsAccess(EventLoggerAccess)"/>;
    /// </code>......<code>
    /// void ApcsOnChange(bool isReady) { </code>...<code> }
    /// apcsInstance.EventConnection += new ConnectionStateChangeHandler(ApcsOnChange);
    /// </code>......<code>
    /// apcsInstance.<see cref="ApcsAccess.Start()"/></code>;
    /// apcsInstance.<see cref="ApcsAccess.ReadyWait()"/></code>;
    /// </code>...<code>
    /// apcsInstance.<see cref="ApcsAccess.Dispose()"/></code>; apcsInstance = null;
    /// </code></example>
    public class ApcsAccess : IDisposable, INotifyPropertyChanged
    {
        /// <summary/>
        [DefaultValue(0.0f)]
        public float AdaptiveSpeed { get; private set; }

        /// <summary/>
        [DefaultValue(OperatingMode.NonAdpativePortal)]
        public OperatingMode AdaptiveSpeedMode { get; private set; }

        /// <summary>
        /// This is a safety net to ensure that resources are disposed even if
        /// <see cref="Dispose()"/> is not called.</summary>
        ~ApcsAccess() { Dispose(!Disposed); }

        /// <summary>Create and prepare a new class instance.</summary>
        /// <param name="eventLogger">
        /// Logger Reference specifies a <see cref="EventLoggerAccess"/> instance and must not be
        /// null.</param>
        /// <remarks>This class is a singleton: only one instance may exist at a time.</remarks>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="eventLogger"/> specifies null, an exception is thrown.</exception>
        /// <exception cref="Exception">
        /// If an instance of this class exists, already, an exception is thrown.</exception>
        public ApcsAccess(EventLoggerAccess eventLogger)
        {
            if (/*invalid?*/ eventLogger == null)
                throw new ArgumentNullException(ClassName + " logger reference (eventLogger) must not be null");
            if (/*already instantiated?*/ Logger != null)
                throw new Exception(ClassName + " is a singleton; only one instance may exist at any one time");
            Logger = eventLogger;
            try { Logger.LogInfo("+++" + ClassName + "+++"); }
            catch { }
        }

        /// <summary>
        /// Reset gets the APCS reset flag: set true when a reset is detected; reset when an
        /// ordinary message is received from the APCS.</summary>
        [DefaultValue(false)]
        public bool APCSReset { get; private set; }

        /// <summary>
        /// Reset alerts subscribers when an APCS board reset is detected (not always
        /// possible).</summary>
        public event EventHandler APCSResetEvent;
        private void APCSResetEventDispose() { APCSResetEvent = null; }

        /// <summary>
        /// APCS Update is used to announce a command transaction with the APCS so that
        /// special actions may be taken.</summary>
        /// <remarks>
        /// Subscriber invocations are done in such a manner that any exceptions thrown back to the
        /// instance are suppressed.</remarks>
        /// <example><code>
        /// ApcsAccess apcs = new <see cref="ApcsAccess(EventLoggerAccess)"/>
        /// apcs.ApcsUpdate += new ApcsUpdateHandler(ApcsOnUpdate);
        /// </code>...<code>
        /// private void ApcsOnUpdate( /*see L3.Cargo.Communications.APCS.Common*/
        ///         CommandEnum command,
        ///         ActionEnum action,
        ///         byte subAction,
        ///         object data)
        /// { ... }
        /// </code></example>
        public event ApcsUpdateHandler ApcsUpdate;
        private void ApcsUpdateDispose() { ApcsUpdate = null; }

        /// <summary>Class Name specifies the name of this class.</summary>
        public static string ClassName { get { return MethodBase.GetCurrentMethod().DeclaringType.Name; } }

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
                MonitorStop();
                ReadyEventDispose();
                APCSResetEventDispose();
                ApcsUpdateDispose();
                PropertyChangeDispose();
                SpeedMsgEventDispose();
                ConnectionDispose();
                Logger.LogInfo("---" + ClassName + "---");
                Logger = null;
                Disposed = true;
            }
        }

        /// <summary>
        /// Disposed declares whether or not the class instance has been disposed.</summary>
        [DefaultValue(false)]
        public static bool Disposed { get; private set; }

        #region Command IP Address and Port
        /// <summary>
        /// APCS Internet Protocol Address gets/sets the IP address on which the APCS is expected
        /// to be listening for commands.</summary>
        /// <value>APCS <see cref="IPAddress"/></value>
        /// <returns>APCS <see cref="IPAddress"/></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If an invalid value is supplied to the settor, an exception is thrown and the existing
        /// value remains (if that is invalid, it is set to a default and known valid
        /// value).</exception>
        /// <exception cref="ConfigurationErrorsException">
        /// If an invalid value is specified in the application's configuration, an exception is
        /// thrown and a default and valid value is applied.</exception>
        public IPAddress CommandIpAddress
        {
            get
            {
                try
                {
                    if (/*invalid?*/ !Utilities.IPAddressOK(_commandIpAddress))
                        throw new Exception(/*catch translates*/);
                }
                catch
                {
                    // Prepare, using the application's configuration. Just for safety, before
                    // doing anything else, apply a default and known valid value.
                    _commandIpAddress = CommandIpAddressDftValue;
                    string configurationText = null;
                    try
                    {
                        // Prepare the property using configuration information.
                        IPAddress addressValue;
                        configurationText = ConfigurationManager.AppSettings[CommandIpAddressKey].Trim();
                        if (/*invalid?*/ !IPAddress.TryParse(configurationText, out addressValue))
                            throw new ConfigurationErrorsException();  /*catch translates*/
                        _commandIpAddress = addressValue;
                        try { Logger.LogInfo("ConnectionIpAddress/" + CommandIpAddressKey + " set to " + addressValue.ToString()); }
                        catch { }
                    }
                    catch
                    {
                        if (/*invalid?*/ !Utilities.IPAddressOK(_commandIpAddress))
                            _commandIpAddress = CommandIpAddressDftValue;

                        // Translate the exception to show more configuration information.
                        configurationText = string.IsNullOrWhiteSpace(configurationText) ? string.Empty : configurationText.Trim();
                        throw new ConfigurationErrorsException("ConnectionIpAddress/" + CommandIpAddressKey + " of \"" + configurationText + "\" must specify a valid, internet protocol address; property value defaulted to " + _commandIpAddress.ToString());
                    }
                }
                return _commandIpAddress;
            }
            set
            {
                if (/*active?*/ _connection != null)
                    if (/*active?*/ _connection.Connected)
                        throw new Exception(new StackTrace(true).GetFrame(1).GetMethod().ReflectedType.Name + " call to ConnectionIpAddress: " + ClassName + " is active and cannot accept ConnectionIpAddress changes");
                string addressText = "null";
                IPAddress addressValue = IPAddress.None;
                try
                {
                    addressText = value.ToString(); /*avoid doing anything in catch clause*/
                    if (/*invalid?*/ !IPAddress.TryParse(addressText, out addressValue))
                        throw new ArgumentOutOfRangeException(); /*catch translates*/
                    bool isChange = addressValue != _commandIpAddress;
                    _commandIpAddress = addressValue;
                    if (/*change?*/ isChange)
                        try { Logger.LogInfo("ConnectionIpAddress set to " + addressValue.ToString()); }
                        catch { }
                }
                catch
                {
                    try { addressValue = CommandIpAddress; } /*ensure value*/
                    catch { }
                    Debug.Assert(Utilities.IPAddressOK(_commandIpAddress));
                    throw new ArgumentOutOfRangeException(new StackTrace(true).GetFrame(1).GetMethod().ReflectedType.Name + " " + addressText + " is not an acceptable internet protocol address; property value remains " + _commandIpAddress.ToString());
                }
            }
        }
        /// <summary/>
        public IPAddress CommandIpAddressDftValue
        {
            get
            {
                IPAddress addressValue = IPAddress.None;
                IPAddress.TryParse("192.168.0.90", out addressValue);
                return addressValue;
            }
        }
        /// <summary/>
        public const string CommandIpAddressKey = "ApcsAddress";
        private IPAddress _commandIpAddress = /*invalid; causes preparation*/ null;

        /// <summary>
        /// APCS Internet Protocol Port gets/sets the IP port on which the APCS is expected
        /// to be listening for commands.</summary>
        /// <value>int value in the domain [0, 65535]</value>
        /// <returns>int value in the domain [0, 65535]</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If an invalid value is supplied to the settor, an exception is thrown and the existing
        /// value remains (if that is invalid, it is set to a default and known valid
        /// value).</exception>
        /// <exception cref="ConfigurationErrorsException">
        /// If an invalid value is specified in the application's configuration, an exception is
        /// thrown and a default and known valid value is applied.</exception>
        public int CommandIpPort
        {
            get
            {
                try
                {
                    if (/*prepare?*/ !Utilities.IPPortOK(_commandIpPort))
                        CommandIpPort = Utilities.PropertyConfigureInt(CommandIpPortKey, /*minimum*/ IPEndPoint.MinPort, /*maximum*/ IPEndPoint.MaxPort, /*default*/ CommandIpPortDftValue);
                }
                catch
                {
                    try { CommandIpPort = CommandIpPortDftValue; } catch { }
                    throw; /*exception prepared by PropertyConfigureInt*/
                }
                return _commandIpPort;
            }
            set
            {
                if (/*active?*/ Connected)
                    throw new Exception(ClassName + " is active and cannot accept ConnectionIpPort changes");
                try
                {
                    if (/*invalid?*/ !Utilities.IPPortOK(value))
                        throw new ArgumentOutOfRangeException(CommandIpPortKey + " value (" + value.ToString() + ") must be in the domain " + Utilities.IPPortDomain);
                    if (/*change?*/ value != _commandIpPort)
                    {
                        _commandIpPort = value;
                        try { Logger.LogInfo(CommandIpPortKey + " set to " + value.ToString()); }
                        catch { }
                    }
                }
                catch
                {
                    if (/*invalid?*/ !Utilities.IPPortOK(_commandIpPort))
                        _commandIpPort = /*ensured value*/ CommandIpPortDftValue;
                    throw;
                }
            }
        }
        private int _commandIpPort = /*invalid; causes preparation*/ int.MinValue;
        /// <summary/>
        public const int CommandIpPortDftValue = /*MUST BE VALID!*/ 2730;
        /// <summary/>
        public const string CommandIpPortKey = "ApcsPort";
        #endregion

        private CommandDefinition.CommandFormat CommandMake(CommandEnum commandCode, BooleanValue acknowledgeIsRequested, ActionEnum actionCode, byte subAction)
        {
            CommandDefinition.CommandAckStruct acknowledgement = new CommandDefinition.CommandAckStruct(commandCode, acknowledgeIsRequested, BooleanValue.False);
            CommandDefinition.ActionStruct action = new CommandDefinition.ActionStruct(actionCode, subAction);
            CommandDefinition.CommandFormat returnedCommand = new CommandDefinition.CommandFormat(acknowledgement.CommandWithAck, action.ActionAndSubAction);
            return returnedCommand;
        }

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
        public const string CommandMarginKey = "APCSCommandMargin";
        /// <summary/>
        public const int CommandMarginMaxValue = Utilities.Time10SECONDS;
        private void CommandMarginReset() { _commandMargin.Reset(); }
        #endregion

        private string CommandReplyValidate(CommandDefinition.CommandFormat? reply, CommandEnum expectedCommand)
        {
            string result = /*OK*/ string.Empty;
            if (/*fail?*/ reply == null)
                result = "null reply";
            else if (/*fail?*/ !reply.HasValue)
                result = "reply has no value (!.HasValue)";
            else if (/*fail?*/ reply.Value.CommandAck.Command != expectedCommand)
                result = ".Value.CommandAck.Command(" + reply.Value.CommandAck.Command.ToString() + ") != " + expectedCommand.ToString();
            else if (/*fail?*/ reply.Value.Action.Action != ActionEnum.Response)
                result = ".Value.Action.Action(" + reply.Value.Action.Action.ToString() + ") != " + ActionEnum.Response.ToString();
            return result;
        }
        private string CommandReplyValidate(CommandDefinition.CommandFormat? reply, CommandEnum expectedCommand, byte expectedSubAction)
        {
            string result = CommandReplyValidate(reply, expectedCommand);
            if (/*OK?*/ string.IsNullOrWhiteSpace(result))
                if (/*fail?*/ reply.Value.CommandAck.IsAck != BooleanValue.True)
                    result = ".Value.CommandAck.IsAck(" + reply.Value.CommandAck.IsAck.ToString() + ") != BooleanValue.True";
                else if (/*fail?*/ reply.Value.Action.SubAction != expectedSubAction)
                    result = ".Value.Action.SubAction(" + reply.Value.Action.SubAction.ToString() + ") != " + expectedSubAction.ToString();
            return result;
        }

        #region Command Send
        private CommandDefinition.CommandFormat? /*reply, if any*/ CommandSend(CommandDefinition.CommandFormat command, bool replyIsExpected = true)
        {
            CommandDefinition.CommandFormat? reply = null;
            try
            {
                int ReadBufferLength = 10 * Marshal.SizeOf(typeof(CommandDefinition.CommandFormat));
                lock (_commandSendLock)
                    if (/*run?*/ !_monitorEnd.WaitOne(CommandMargin))
                        if (/*OK?*/ (_connection != null) && _connection.Connected && (_connection.GetStream() != null))
                        {
                            byte[] buffer = command.Serialize();
                            _connection.GetStream().Write(buffer, 0, buffer.Length);
                            _connection.GetStream().Flush();
                            CommandMarginReset();

                            if (/*reply expected?*/ replyIsExpected)
                                if (/*run?*/ !_monitorEnd.WaitOne(0))
                                {
                                    // Acquire the reply.
                                    buffer = new byte[ReadBufferLength];
                                    reply = null;
                                    for (int retry = 0; /*OK?*/ retry < CommandSendReads; reply = null, retry++)
                                        if (!_connection.GetStream().DataAvailable)
                                            if (/*run?*/ !_monitorEnd.WaitOne((retry + 1) * Utilities.TimeTENTH))
                                                continue;
                                            else
                                                break;
                                        else
                                        {
                                            // Read blocks/waits up through
                                            // TcpClient.GetStream().ReadTimeout; if the
                                            // client is disposed, the read faults.
                                            if (/*read OK?*/ _connection.GetStream().Read(buffer, 0, buffer.Length) >= 1)
                                            {
                                                reply = CommandSendProcess(command.Deserialize(buffer));
                                                if (/*OK?*/ reply != null)
                                                    if (/*OK?*/ reply.HasValue)
                                                        if (/*OK?*/ reply.Value.CommandAck.Command == command.CommandAck.Command)
                                                            if (/*OK?*/ reply.Value.Action.Action == ActionEnum.Response)
                                                            {
                                                                if (/*run?*/ !_monitorEnd.WaitOne(0))
                                                                    if (/*subscriber(s)?*/ ApcsUpdate != null)
                                                                        try { ApcsUpdate(reply.Value.CommandAck.Command, reply.Value.Action.Action, reply.Value.Action.SubAction, reply.Value.Payload); }
                                                                        catch { }
                                                                break;
                                                            }
                                            }
                                            else
                                                break;
                                        }
                                    try
                                    {
                                        // Empty the "pipe."
                                        buffer = new byte[ReadBufferLength];
                                        Debug.Assert(_connection.GetStream().ReadTimeout == Utilities.TimeTENTH);
                                        Debug.Assert(_connection.GetStream().CanRead);
                                        while (/*run?*/ !_monitorEnd.WaitOne(0) && /*not empty?*/ _connection.GetStream().DataAvailable)
                                            // Read blocks/waits up through
                                            // TcpClient.GetStream().ReadTimeout; if the
                                            // client is disposed, the read faults.
                                            if (/*data?*/ _connection.GetStream().Read(buffer, 0, buffer.Length) >= 1)
                                                CommandSendProcess(command.Deserialize(buffer));
                                    }
                                    catch { }
                                }
                        }
            }
            catch
            {
                ConnectionDispose();
                if (/*run?*/ !_monitorEnd.WaitOne(0))
                    throw;
            }
            return reply;
        }
        private CommandDefinition.CommandFormat? CommandSendProcess(CommandDefinition.CommandFormat? reply)
        {
            if (!reply.HasValue)
                reply = null;
            else if (reply.Value.CommandAck.Command == CommandEnum.AdaptiveModeSpeed)
            {
                AdaptiveSpeed = BitConverter.ToSingle(reply.Value.Payload, 0);
                AdaptiveSpeedMode = (OperatingMode)reply.Value.Action.SubAction;
                SpeedMessageEvent.Set();
                reply = null;
            }
            else if (reply.Value.CommandAck.Command == CommandEnum.SignOfLife)
            {
                uint sequence = BitConverter.ToUInt32(reply.Value.Payload, 0);
                SignOfLifeSequence = sequence;

                // If a board reset is detected, notify any subscribers.
                APCSReset = /*reset?*/ SignOfLifeSequence > (uint)int.MaxValue;
                if (/*reset?*/ APCSReset)
                    if (/*not shutting down?*/ !_monitorEnd.WaitOne(0))
                        if (/*subscribers?*/ APCSResetEvent != null)
                            try { APCSResetEvent(this, EventArgs.Empty); }
                            catch { }
                reply = null;
            }
            return reply;
        }
        /// <summary/>
        public const int CommandSendReads = 10;
        private object _commandSendLock = new object();
        #endregion

        #region Connect, Disconnect, Start and Stop
        /// <summary>Start the instance.</summary>
        /// <remarks>
        /// This method is written so that it may be called repeatedly, whether or not the instance
        /// is already started.</remarks>
        /// <exception cref="Exception">
        /// The method makes use of multiple supporting methods, any one of which might throw an
        /// exception if an anomaly is detected.</exception>
        public void Connect()
        {
            Start();
        }
        /// <summary>Stop the instance.</summary>
        /// <remarks>
        /// This method is written so that it may be called repeatedly, whether or not the instance
        /// is already stopped.</remarks>
        /// <exception cref="Exception">
        /// The method is written so that it never throws an exception.</exception>
        public void Disconnect()
        {
            Stop();
        }
        /// <summary>Start the instance.</summary>
        /// <remarks>
        /// This method is written so that it may be called repeatedly, whether or not the instance
        /// is already started.</remarks>
        /// <exception cref="Exception">
        /// The method makes use of multiple supporting methods, any one of which might throw an
        /// exception if an anomaly is detected.</exception>
        public void Start()
        {
            MonitorStart();
        }
        /// <summary>Stop the instance.</summary>
        /// <remarks>
        /// This method is written so that it may be called repeatedly, whether or not the instance
        /// is already stopped.</remarks>
        /// <exception cref="Exception">
        /// The method is written so that it never throws an exception.</exception>
        public void Stop()
        {
            MonitorStop();
        }
        #endregion

        #region Connection
        private bool _connected = false;
        private TcpClient _connection = null;

        /// <summary>
        /// Connected specifies whether or not the instance is ready to perform actions.</summary>
        /// <returns>APCS readiness</returns>
        public bool Connected
        {
            get { return _connected; }
            private set
            {
                if (/*change?*/ value != _connected)
                {
                    _connected = value;
                    try { Logger.LogInfo(ClassName + " is" + (_connected ? string.Empty : " not") + " connected to " + CommandIpAddress.ToString() + ":" + CommandIpPort.ToString()); }
                    catch { }
                    if (/*not shutting down?*/ !_monitorEnd.WaitOne(0))
                        if (/*subscribers?*/ ReadyEvent != null)
                            try { ReadyEvent(_connected); }
                            catch { }
                }
            }
        }

        private void ConnectionCreate()
        {
            try
            {
                if (/*run?*/ !_monitorEnd.WaitOne(0))
                    if (!Connected || (/*(re)create?*/ _connection == null))
                    {
                        ConnectionDispose( /*ensure tidiness*/ );
                        Debug.Assert(_connection == null);
                        _connection = new TcpClient();
                        _connection.Connect(CommandIpAddress, CommandIpPort);
                        Debug.Assert(_connection.GetStream().CanRead);
                        _connection.GetStream().ReadTimeout = Utilities.TimeTENTH;
                        Debug.Assert(_connection.GetStream().CanWrite);
                        _connection.GetStream().WriteTimeout = Utilities.TimeTENTH;
                        foreach (PulseWidth width in Enum.GetValues(typeof(PulseWidth)))
                            PulseWidthConfigure(width, (float)PulsePeriod(width));
                        SetStaticPulseFrequency(OperatingMode.NonAdaptiveMobile, PulseFrequencyDual);
                        SetCurrentPulseWidth(PulseWidth.PulseWidth1, /*confirm*/ true);
                        Connected = true;
                    }
            }
            catch
            {
                ConnectionDispose( /*tidy*/ ); Debug.Assert(!Connected);
                throw;
            }
        }
        private void ConnectionDispose()
        {
            Connected = false;
#if false
            try { _connection.GetStream().Close(); } catch { }
            try { _connection.Client.Shutdown(SocketShutdown.Both); } catch { }
            try { _connection.Client.Close(); } catch { }
            try { _connection.Client.Dispose(); } catch { }
#endif
            try
            {
                if (/*exists (avoid first try exceptions)?*/ _connection != null)
                    _connection.Close();
            }
            catch { }
            finally { _connection = null; }
        }
        #endregion

        /// <summary>
        /// Current Pulse Width gets the currently in use pulse width type.</summary>
        public PulseWidth CurrentPulseWidth { get; private set; }

        /// <summary/>
        public bool DetectorsTimingEnable(bool enable)
        {
            return SetPWMOutputEnable(enable ? PWMOutputConfig.OutputEnabled : PWMOutputConfig.OutputDisabled, true);
        }

        /// <summary/>
        public bool GetAdaptiveModeTriggerRatio(OperatingMode? selectedMode, out OperatingMode? mode, out float? ratio, bool confirm)
        {
            bool status = false;
            try
            {
                CommandDefinition.CommandFormat command = CommandMake(CommandEnum.AdaptiveModeToTrigRatio, BooleanValue.False, ActionEnum.Get, (byte)selectedMode);
                CommandDefinition.CommandFormat? reply = CommandSend(command, /*reply expected*/ true);
                string text = CommandReplyValidate(reply, CommandEnum.AdaptiveModeToTrigRatio);
                if (/*fail?*/ !string.IsNullOrWhiteSpace(text))
                    throw new Exception(text);
                mode = (OperatingMode)OperatingMode.ToObject(typeof(OperatingMode), reply.Value.Action.SubAction);
                ratio = BitConverter.ToSingle(reply.Value.Payload, 0);
                status = true;
            }
            catch (Exception ex)
            {
                mode = null;
                ratio = null;
                status = false;
                if (/*OK to log?*/ LogPauseExpired)
                    try { Logger.LogError(Utilities.TextTidy(ex.ToString())); }
                    catch { }
            }
            return status;
        }

        /// <summary/>
        public bool GetAdaptiveSpeedFeedbackConfiguration(out AdaptiveSpeedFeedbackConfig? configuration, out float? frequency, bool confirm)
        {
            bool status = false;
            try
            {
                CommandDefinition.CommandFormat command = CommandMake(CommandEnum.AdaptiveSpeedFeedbackConfig, BooleanValue.False, ActionEnum.Get, 0);
                CommandDefinition.CommandFormat? reply = CommandSend(command, /*reply expected*/ true);
                string text = CommandReplyValidate(reply, CommandEnum.AdaptiveSpeedFeedbackConfig);
                if (/*fail?*/ !string.IsNullOrWhiteSpace(text))
                    throw new Exception(text);
                configuration = (AdaptiveSpeedFeedbackConfig)OperatingMode.ToObject(typeof(AdaptiveSpeedFeedbackConfig), reply.Value.Action.SubAction);
                frequency = BitConverter.ToSingle(reply.Value.Payload, 0);
                status = true;
            }
            catch (Exception ex)
            {
                configuration = null;
                frequency = null;
                status = false;
                if (/*OK to log?*/ LogPauseExpired)
                    try { Logger.LogError(Utilities.TextTidy(ex.ToString())); }
                    catch { }
            }
            return status;
        }

        /// <summary/>
        public bool GetConfigPulseWidth(PulseWidth? width, out float? time, bool confirm)
        {
            bool status = false;
            try
            {
                CommandDefinition.CommandFormat command = CommandMake(CommandEnum.ConfigPulseWidth, BooleanValue.True, ActionEnum.Get, (byte)width);
                CommandDefinition.CommandFormat? reply = CommandSend(command, /*reply expected*/ true);
                string text = CommandReplyValidate(reply, CommandEnum.ConfigPulseWidth);
                if (/*fail?*/ !string.IsNullOrWhiteSpace(text))
                    throw new Exception(text);
                time = BitConverter.ToSingle(reply.Value.Payload, 0);
                status = true;
            }
            catch (Exception ex)
            {
                time = null;
                width = null;
                status = false;
                if (/*OK to log?*/ LogPauseExpired)
                    try { Logger.LogError(Utilities.TextTidy(ex.ToString())); }
                    catch { }
            }
            return status;
        }

        /// <summary/>
        public bool GetCurrentPulseWidth(out PulseWidth? width, bool confirm)
        {
            bool status = false;
            try
            {
                CommandDefinition.CommandFormat command = CommandMake(CommandEnum.PulseWidth, BooleanValue.False, ActionEnum.Get, 0);
                CommandDefinition.CommandFormat? reply = CommandSend(command, /*reply expected*/ true);
                string text = CommandReplyValidate(reply, CommandEnum.PulseWidth);
                if (/*fail?*/ !string.IsNullOrWhiteSpace(text))
                    throw new Exception(text);
                CurrentPulseWidth = (PulseWidth)PulseWidth.ToObject(typeof(PulseWidth), reply.Value.Action.SubAction);
                width = CurrentPulseWidth;
                status = true;
            }
            catch (Exception ex)
            {
                width = null;
                status = false;
                if (/*run?*/ !_monitorEnd.WaitOne(0))
                    if (/*OK to log?*/ LogPauseExpired)
                        try { Logger.LogError(Utilities.TextTidy(ex.ToString())); }
                        catch { }
            }
            return status;
        }

        /// <summary/>
        public bool GetOperatingMode(out OperatingMode? mode, out short? frequencyMinimum, out short? frequencyMaximum, bool confirm)
        {
            bool status = false;
            try
            {
                CommandDefinition.CommandFormat command = CommandMake(CommandEnum.OperatingMode, BooleanValue.False, ActionEnum.Get, 0);
                CommandDefinition.CommandFormat? reply = CommandSend(command, /*reply expected*/ true);
                string text = CommandReplyValidate(reply, CommandEnum.OperatingMode);
                if (/*fail?*/ !string.IsNullOrWhiteSpace(text))
                    throw new Exception(text);
                mode = (OperatingMode)OperatingMode.ToObject(typeof(OperatingMode), reply.Value.Action.SubAction);
                frequencyMinimum = BitConverter.ToInt16(reply.Value.Payload, 0);
                frequencyMaximum = BitConverter.ToInt16(reply.Value.Payload, 2);
                status = true;
            }
            catch (Exception ex)
            {
                frequencyMinimum = null;
                frequencyMaximum = null;
                mode = null;
                status = false;
                if (/*OK to log?*/ LogPauseExpired)
                    try { Logger.LogError(Utilities.TextTidy(ex.ToString())); }
                    catch { }
            }
            return status;
        }

        /// <summary/>
        public bool GetPWMOutput(out PWMOutputConfig? configuration, bool confirm)
        {
            bool status = false;
            try
            {
                CommandDefinition.CommandFormat command = CommandMake(CommandEnum.PWMOutput, BooleanValue.False, ActionEnum.Get, 0);
                CommandDefinition.CommandFormat? reply = CommandSend(command, /*reply expected*/ true);
                string text = CommandReplyValidate(reply, CommandEnum.PWMOutput);
                if (/*fail?*/ !string.IsNullOrWhiteSpace(text))
                    throw new Exception(text);
                configuration = (PWMOutputConfig)reply.Value.Action.SubAction;
                status = true;
            }
            catch (Exception ex)
            {
                configuration = null;
                status = false;
                if (/*OK to log?*/ LogPauseExpired)
                    try { Logger.LogError(Utilities.TextTidy(ex.ToString())); }
                    catch { }
            }
            return status;
        }

        /// <summary/>
        public bool GetPWMOutputEnable(out PWMOutputConfig? enable, bool confirm)
        {
            bool status = false;
            try
            {
                CommandDefinition.CommandFormat command = CommandMake(CommandEnum.PWMOutput, BooleanValue.False, ActionEnum.Get, 0);
                CommandDefinition.CommandFormat? reply = CommandSend(command, /*reply expected*/ true);
                string text = CommandReplyValidate(reply, CommandEnum.PWMOutput);
                if (/*fail?*/ !string.IsNullOrWhiteSpace(text))
                    throw new Exception(text);
                enable = (PWMOutputConfig)reply.Value.Action.SubAction;
                status = true;
            }
            catch (Exception ex)
            {
                enable = null;
                status = false;
                if (/*OK to log?*/ LogPauseExpired)
                    try { Logger.LogError(Utilities.TextTidy(ex.ToString())); }
                    catch { }
            }
            return status;
        }

        /// <summary/>
        public bool GetScanEnergyMode(out ScanEnergyMode? mode, bool confirm)
        {
            bool status = false;
            try
            {
                CommandDefinition.CommandFormat command = CommandMake(CommandEnum.ScanMode, BooleanValue.False, ActionEnum.Get, 0);
                CommandDefinition.CommandFormat? reply = CommandSend(command, /*reply expected*/ true);
                string text = CommandReplyValidate(reply, CommandEnum.ScanMode);
                if (/*fail?*/ !string.IsNullOrWhiteSpace(text))
                    throw new Exception(text);
                mode = (ScanEnergyMode)ScanEnergyMode.ToObject(typeof(ScanEnergyMode), reply.Value.Action.SubAction);
                status = true;
            }
            catch (Exception ex)
            {
                mode = null;
                status = false;
                if (/*OK to log?*/ LogPauseExpired)
                    try { Logger.LogError(Utilities.TextTidy(ex.ToString())); }
                    catch { }
            }
            return status;
        }

        /// <summary/>
        public bool GetStaticPulseFrequency(OperatingMode? selectedMode, out OperatingMode? operatingMode, out int? frequency, bool confirm)
        {
            bool status = false;
            try
            {
                CommandDefinition.CommandFormat command = CommandMake(CommandEnum.StaticPulseFreq, BooleanValue.False, ActionEnum.Get, (byte)selectedMode);
                CommandDefinition.CommandFormat? reply = CommandSend(command, /*reply expected*/ true);
                string text = CommandReplyValidate(reply, CommandEnum.StaticPulseFreq);
                if (/*fail?*/ !string.IsNullOrWhiteSpace(text))
                    throw new Exception(text);
                operatingMode = (OperatingMode)OperatingMode.ToObject(typeof(OperatingMode), reply.Value.Action.SubAction);
                frequency = BitConverter.ToInt32(reply.Value.Payload, 0);
                status = true;
            }
            catch (Exception ex)
            {
                frequency = null;
                operatingMode = null;
                status = false;
                if (/*OK to log?*/ LogPauseExpired)
                    try { Logger.LogError(Utilities.TextTidy(ex.ToString())); }
                    catch { }
            }
            return status;
        }

        #region Logging
        /// <summary>
        /// Logger references a <see cref="EventLoggerAccess"/> to provide announcement/logging
        /// facilities.</summary>
        [DefaultValue(null)]
        public static EventLoggerAccess Logger { get; private set; }

        /// <summary/>
        public int LogPause
        {
            get { return _logPause.Period; }
            private set { _logPause.Reset(value); }
        }
        private Latency _logPause = new Latency(0);
        private void LogPauseError(string text)
        {
            if (/*not shutdown?*/ !_monitorEnd.WaitOne(0))
                if (/*OK to log?*/ LogPauseExpired)
                {
                    LogPause = (int)Utilities.Within(Utilities.TimeSECOND, 2 * LogPause, 10 * Utilities.TimeMINUTE);
                    if (/*empty?*/ string.IsNullOrWhiteSpace(text))
                        text = string.Empty;
                    else
                        text += "\r   ";
                    text += "logging disabled for " + Utilities.TimeText(LogPause);
                    try { Logger.LogError(text); } catch { }
                }
        }
        private bool LogPauseExpired { get { return _logPause.Expired; } }
        #endregion

        #region Monitor
        private void MonitorAgent()
        {
            _monitorException = null;
            LogPause = /*OK to log*/ 0;
            try { Logger.LogInfo(Threads.Identity() + " started"); } catch { }
            try
            {
                do
                    try
                    {
                        ConnectionCreate( /*does nothing if all OK*/ );

                        // Issue an innocuous command to "tickle" the APCS and to cleanse the
                        // "pipe" of unsolicited (pain in the ass) sign of life "responses."
                        PulseWidth? /*pulse width (unused)*/ pulseWidth = null;
                        GetCurrentPulseWidth(out pulseWidth, /*confirm*/ true);
                        LogPause = /*OK, again, to log*/ 0;
                    }
                    catch (SocketException ex)
                    {
                        LogPauseError(Utilities.TextTidy(ex.ToString()) + "\r  SocketErrorCode==" + Utilities.TextTidy(ex.SocketErrorCode.ToString()));
                        ConnectionDispose();
                    }
                    catch (Exception ex)
                    {
                        LogPauseError(Utilities.TextTidy(ex.ToString()));
                        ConnectionDispose();
                    }
                while (/*run?*/ !_monitorEnd.WaitOne(CommandMargin));
            }
            catch (Exception ex) { _monitorException = ex; }
            finally
            {
                ConnectionDispose();
                try { Logger.LogInfo(Threads.Identity() + " ended"); } catch { }
            }
        }
        private void MonitorStart()
        {
            if (/*run?*/ !_monitorEnd.WaitOne(0))
            {
                if (/*(re)create?*/ _monitorThread == null)
                    _monitorThread = Threads.Create(MonitorAgent, ref _monitorEnd, "APCS Connection Monitor thread");
                if (/*(re)start?*/ _monitorThread != null)
                    if (/*(re)start?*/ !_monitorThread.IsAlive)
                    {
                        _monitorEnd.Reset();
                        _monitorException = null;
                        _monitorThread.Start();
                    }
            }
        }
        private void MonitorStop()
        {
            _monitorEnd.Set();
            ConnectionDispose();
            try
            {
                if (/*exists (avoid first try exceptions)?*/ _monitorThread != null)
                    _monitorThread = Threads.Dispose(_monitorThread, ref _monitorEnd);
            }
            catch { }
            finally { _monitorThread = null; }
        }
        private ManualResetEvent _monitorEnd = new ManualResetEvent(false);
        private Exception _monitorException;
        private Thread _monitorThread;
        #endregion

        /// <summary/>
        public event PropertyChangedEventHandler PropertyChanged;
        private void PropertyChangeDispose() { PropertyChanged = null; }
        private void PropertyChangeNotify(string propertyName)
        {
            if (/*subscribers?*/ PropertyChanged != null)
                try { PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
                catch { }
        }

        #region Dual Pulse Frequency
        /// <summary>Dual Pulse Frequency gets/sets the dual pulse frequency.</summary>
        /// <value>
        /// The settor's value must be in the domain
        /// [<see cref="PulseFrequencyDualMinValue"/>, <see cref="PulseFrequencyDualMaxValue"/>].
        /// The default value is obtained from the application's configuration under the
        /// <see cref="PulseFrequencyDualKey"/> tag name.
        /// </value>
        /// <returns>dual pulse frequency</returns>
        public uint PulseFrequencyDual
        {
            get
            {
                try
                {
                    if (/*prepare?*/ !PulseFrequencyDualOK(_pulseFrequencyDual))
                        _pulseFrequencyDual = (uint)Utilities.PropertyConfigureInt(PulseFrequencyDualKey, /*minimum*/ (int)PulseFrequencyDualMinValue, /*maximum*/ (int)PulseFrequencyDualMaxValue, /*default*/ (int)PulseFrequencyDualDftValue);
                }
                catch
                {
                    if (/*invalid?*/ !PulseFrequencyDualOK(_pulseFrequencyDual))
                        _pulseFrequencyDual = PulseFrequencyDualDftValue;
                    throw; /*exception prepared by PropertyConfigureInt*/
                }
                return _pulseFrequencyDual;
            }
            set
            {
                if (/*active?*/ Connected)
                    throw new Exception(new StackTrace(true).GetFrame(1).GetMethod().ReflectedType.Name + " " + ClassName + " is active and cannot accept " + MethodBase.GetCurrentMethod().Name + " changes");
                try
                {
                    if (/*invalid?*/ !PulseFrequencyDualOK(value))
                        throw new ArgumentOutOfRangeException();
                    bool isChange = value != _pulseFrequencyDual;
                    _pulseFrequencyDual = value;
                    if (/*change?*/ isChange)
                        try { Logger.LogInfo("ConnectionIpPort set to " + _pulseFrequencyDual.ToString()); }
                        catch { }
                }
                catch
                {
                    try { uint frequencyValue = PulseFrequencyDual; } /*ensure value*/
                    catch { }
                    throw new ArgumentOutOfRangeException(new StackTrace(true).GetFrame(1).GetMethod().ReflectedType.Name + " " + value.ToString() + " is not an acceptable " + PulseFrequencyDualKey + " value; " + PulseFrequencyDualKey + " remains " + _pulseFrequencyDual.ToString());
                }
            }
        }
        /// <summary/>
        public const int PulseFrequencyDualDftValue = 400;
        /// <summary/>
        public const string PulseFrequencyDualKey = "DualPulseFrequency";
        /// <summary/>
        public const uint PulseFrequencyDualMaxValue = (uint)short.MaxValue;
        /// <summary/>
        public const uint PulseFrequencyDualMinValue = 1;
        /// <summary/>
        public static bool PulseFrequencyDualOK(uint value)
        {
            return (value >= PulseFrequencyDualMinValue) && (value <= PulseFrequencyDualMaxValue);
        }
        private uint _pulseFrequencyDual = /*invalid; causes preparation*/ 0;
        #endregion

        #region Pulse Period
        private double[] _pulsePeriod = null;
        private string _pulsePeriodDomain = string.Empty;

        /// <summary>
        /// Pulse Period gets, by <see cref="PulseWidth"/> enumeration, a pulse width
        /// value.</summary>
        /// <param name="width">
        /// Pulse Width specifies, by <see cref="PulseWidth"/> enumerated value, the pulse width to
        /// be obtained...</param>
        /// <returns>pulse width in microseconds</returns>
        public double PulsePeriod(PulseWidth width)
        {
            int index = 0;
            if (/*prepare?*/ _pulsePeriod == null)
            {
                _pulsePeriod = new double[Enum.GetNames(typeof(PulseWidth)).Length];
                _pulsePeriodDomain = "{";
                Debug.Assert(index == 0);
                foreach (PulseWidth widthType in Enum.GetValues(typeof(PulseWidth)))
                {
                    string keyText = "PulseWidth" + ((int)widthType).ToString() + "Microseconds";
                    _pulsePeriod[index] = Utilities.PropertyConfigureReal(keyText, 1.0, double.MaxValue, -double.MaxValue, "μs");
                    if (/*finish previous entry?*/ index >= 1)
                        _pulsePeriodDomain += "μs, ";
                    _pulsePeriodDomain += widthType.ToString() + " = " + _pulsePeriod[index].ToString();
                    index++; /*for next cycle*/
                }
                _pulsePeriodDomain += "μs}";
                try { Logger.LogInfo("pulse widths set to " + _pulsePeriodDomain); } catch { }
            }
            index = 0;
            foreach (PulseWidth widthType in Enum.GetValues(typeof(PulseWidth)))
                if (/*not this one?*/ width != widthType)
                    index++;
                else /*found: done*/
                    return _pulsePeriod[index];
            throw new IndexOutOfRangeException(MethodBase.GetCurrentMethod().Name + ": width (" + width.ToString() + ") must be from " + _pulsePeriodDomain);
        }
        #endregion

        private void PulseWidthConfigure(PulseWidth width, float time)
        {
            string traceText = MethodBase.GetCurrentMethod().Name + "(" + width.ToString() + ", " + time.ToString() + ")";
            try
            {
                CommandDefinition.CommandFormat command = CommandMake(CommandEnum.ConfigPulseWidth, BooleanValue.True, ActionEnum.Set, (byte)width);
                BitConverter.GetBytes(time).CopyTo(command.Payload, 0);
                CommandDefinition.CommandFormat? reply = CommandSend(command, /*reply expected*/ true);
                string text = CommandReplyValidate(reply, CommandEnum.ConfigPulseWidth);
                if (/*fail?*/ text != string.Empty)
                    throw new Exception(traceText + "; " + text);
                float timeValue = BitConverter.ToSingle(reply.Value.Payload, 0);
                if (/*incorrect result*/ time != timeValue)
                    throw new Exception(traceText + "; " + time.ToString() + " != " + timeValue.ToString());
            }
            catch { throw; }
        }

        /// <summary>
        /// Event, Ready is used to announce changes to the instance's state.</summary>
        /// <remarks>
        /// Subscriber invocations are done in such a manner that any exceptions thrown back to
        /// this instance are suppressed.</remarks>
        public event ConnectionStateChangeHandler ReadyEvent;
        private void ReadyEventDispose() { ReadyEvent = null; }

        /// <summary/>
        public bool ResetBoard()
        {
            try { CommandSend(CommandMake(CommandEnum.ResetBoard, BooleanValue.False, ActionEnum.Set, 0), /*do not confirm*/ false); }
            catch { throw; }
            try { Logger.LogInfo(MethodBase.GetCurrentMethod() + " returns true"); } catch { }
            return true;
        }

        /// <summary/>
        public bool SetAdaptiveModeTriggerRatio(OperatingMode mode, float ratio, bool confirm)
        {
            bool status = false;
            try
            {
                CommandDefinition.CommandFormat command = CommandMake(CommandEnum.AdaptiveModeToTrigRatio, BooleanValue.True, ActionEnum.Set, (byte)mode);
                BitConverter.GetBytes(ratio).CopyTo(command.Payload, 0);
                CommandDefinition.CommandFormat? reply = CommandSend(command, /*reply expected*/ true);
                if (/*ignore results?*/ !confirm)
                    status = true;
                else
                {
                    string text = CommandReplyValidate(reply, CommandEnum.AdaptiveModeToTrigRatio, (byte)mode);
                    if (/*fail?*/ text != string.Empty)
                        throw new Exception(text);
                    if (/*fail?*/ ratio != BitConverter.ToSingle(reply.Value.Payload, 0))
                        throw new Exception("ratio != " + ratio.ToString()); //set result to false
                    status = true;
                }
            }
            catch (Exception ex)
            {
                status = false;
                if (/*OK to log?*/ LogPauseExpired)
                    try { Logger.LogError(Utilities.TextTidy(ex.ToString())); }
                    catch { }
            }
#if DEBUG
            try { Logger.LogInfo(MethodBase.GetCurrentMethod() + "(" + mode.ToString() + ", " + ratio.ToString() + ", " + confirm.ToString() + ") returns " + status.ToString()); }
            catch { }
#endif
            return status;
        }

        /// <summary/>
        public bool SetAdaptiveSpeedFeedbackConfiguration(AdaptiveSpeedFeedbackConfig configuration, float frequency, bool confirm)
        {
            bool status = false;
            try
            {
                CommandDefinition.CommandFormat command = CommandMake(CommandEnum.AdaptiveSpeedFeedbackConfig, BooleanValue.True, ActionEnum.Set, (byte)configuration);
                BitConverter.GetBytes(frequency).CopyTo(command.Payload, 0);
                CommandDefinition.CommandFormat? reply = CommandSend(command, /*reply expected*/ true);
                if (/*ignore results?*/ !confirm)
                    status = true;
                else
                {
                    string text = CommandReplyValidate(reply, CommandEnum.AdaptiveSpeedFeedbackConfig, (byte)configuration);
                    if (/*fail?*/ text != string.Empty)
                        throw new Exception(text);
                    if (/*fail?*/ frequency != BitConverter.ToSingle(reply.Value.Payload, 0))
                        throw new Exception("frequency != " + frequency.ToString());
                    status = true;
                }
            }
            catch (Exception ex)
            {
                status = false;
                if (/*OK to log?*/ LogPauseExpired)
                    try { Logger.LogError(Utilities.TextTidy(ex.ToString())); }
                    catch { }
            }
#if DEBUG
            try { Logger.LogInfo(MethodBase.GetCurrentMethod() + "(" + configuration.ToString() + ", " + frequency.ToString() + ", " + confirm.ToString() + ") returns " + status.ToString()); }
            catch { }
#endif
            return status;
        }

        /// <summary/>
        public bool SetConfigPulseWidth(PulseWidth width, float time, bool confirm)
        {
            bool status = false;
            try
            {
                PulseWidthConfigure(width, time);
                status = true;
            }
            catch { Debug.Assert(!status); }
            return status;
        }

        /// <summary/>
        public bool SetCurrentPulseWidth(PulseWidth width, bool confirm)
        {
            bool status = false;
            string traceText = MethodBase.GetCurrentMethod().Name + "(" + width.ToString() + ")";
            try
            {
                CommandDefinition.CommandFormat command = CommandMake(CommandEnum.PulseWidth, BooleanValue.True, ActionEnum.Set, (byte)width);
                CommandDefinition.CommandFormat? reply = CommandSend(command, /*reply expected*/ true);
                if (/*ignore results?*/ !confirm)
                    status = true;
                else
                {
                    string text = CommandReplyValidate(reply, CommandEnum.PulseWidth, (byte)width);
                    if (/*fail?*/ text != string.Empty)
                        throw new Exception(traceText + "; " + text);
                    status = true;
                }
                CurrentPulseWidth = width;
            }
            catch (Exception ex)
            {
                status = false;
                throw ex;
            }
            return status;
        }

        /// <summary/>
        public bool SetOperatingMode(OperatingMode mode, ushort frequencyMinimum, ushort frequencyMaximum, bool confirm)
        {
            bool status = false;
            try
            {
                CommandDefinition.CommandFormat command = CommandMake(CommandEnum.OperatingMode, BooleanValue.True, ActionEnum.Set, (byte)mode);
                BitConverter.GetBytes(frequencyMinimum).CopyTo(command.Payload, 0);
                BitConverter.GetBytes(frequencyMaximum).CopyTo(command.Payload, 2);
                CommandDefinition.CommandFormat? reply = CommandSend(command, /*reply expected*/ true);
                if (/*ignore results?*/ !confirm)
                    status = true;
                else
                {
                    string text = CommandReplyValidate(reply, CommandEnum.OperatingMode, (byte)mode);
                    if (/*fail?*/ !string.IsNullOrWhiteSpace(text))
                        throw new Exception(text);
                    if (/*fail?*/ frequencyMinimum != BitConverter.ToInt16(reply.Value.Payload, 0))
                        throw new Exception("frequencyMinimum != " + frequencyMinimum.ToString());
                    else if (/*fail?*/ frequencyMaximum != BitConverter.ToInt16(reply.Value.Payload, 2))
                        throw new Exception("frequencyMaximum != " + frequencyMaximum.ToString());
                    status = true;
                }
            }
            catch (Exception ex)
            {
                Debug.Assert(!status);
                if (/*OK to log?*/ LogPauseExpired)
                    try { Logger.LogError(Utilities.TextTidy(ex.ToString())); }
                    catch { }
            }
#if DEBUG
            try { Logger.LogInfo(MethodBase.GetCurrentMethod() + "(" + mode.ToString() + ", " + frequencyMinimum.ToString() + ", " + frequencyMaximum.ToString() + ", " + confirm.ToString() + ") returns " + status.ToString()); }
            catch { }
#endif
            return status;
        }

        /// <summary/>
        public bool SetPWMOutput(PWMOutputConfig? configuration, bool confirm)
        {
            bool status = false;
            try
            {
                CommandDefinition.CommandFormat command = CommandMake(CommandEnum.PWMOutput, BooleanValue.True, ActionEnum.Set, (byte)configuration);
                CommandDefinition.CommandFormat? reply = CommandSend(command, /*reply expected*/ true);
                if (/*ignore results?*/ !confirm)
                    status = true;
                else
                {
                    string text = CommandReplyValidate(reply, CommandEnum.PWMOutput, (byte)configuration);
                    if (/*fail?*/ text != string.Empty)
                        throw new Exception(text);
                    status = true;
                }
            }
            catch (Exception ex)
            {
                Debug.Assert(!status);
                if (/*OK to log?*/ LogPauseExpired)
                    try { Logger.LogError(Utilities.TextTidy(ex.ToString())); }
                    catch { }
            }
#if DEBUG
            try { Logger.LogInfo(MethodBase.GetCurrentMethod() + "(" + ((configuration == null) ? "null" : configuration.ToString()) + ", " + confirm.ToString() + ") returns " + status.ToString()); }
            catch { }
#endif
            return status;
        }

        /// <summary/>
        public bool SetPWMOutputEnable(PWMOutputConfig? enable, bool confirm)
        {
            bool status = false;
            try
            {
                CommandDefinition.CommandFormat command = CommandMake(CommandEnum.PWMOutput, BooleanValue.True, ActionEnum.Set, (byte)enable);
                CommandDefinition.CommandFormat? reply = CommandSend(command, /*reply expected*/ true);
                if (/*ignore results?*/ !confirm)
                    status = true;
                else
                {
                    string text = CommandReplyValidate(reply, CommandEnum.PWMOutput, (byte)enable);
                    if (/*fail?*/ text != string.Empty)
                        throw new Exception(text);
                    status = true;
                }
            }
            catch (Exception ex)
            {
                Debug.Assert(!status);
                if (/*OK to log?*/ LogPauseExpired)
                    try { Logger.LogError(Utilities.TextTidy(ex.ToString())); }
                    catch { }
            }
#if DEBUG
            try { Logger.LogInfo(MethodBase.GetCurrentMethod() + "(" + ((enable == null) ? "null" : enable.ToString()) + ", " + confirm.ToString() + ") returns " + status.ToString()); }
            catch { }
#endif
            return status;
        }

        /// <summary/>
        public bool SetScanEnergyMode(ScanEnergyMode mode, bool confirm)
        {
            bool status = false;
            try
            {
                CommandDefinition.CommandFormat command = CommandMake(CommandEnum.ScanMode, BooleanValue.True, ActionEnum.Set, (byte)mode);
                CommandDefinition.CommandFormat? reply = CommandSend(command, /*reply expected*/ true);
                if (/*ignore results?*/ !confirm)
                    status = true;
                else
                {
                    string text = CommandReplyValidate(reply, CommandEnum.ScanMode, (byte)mode);
                    if (/*fail?*/ !string.IsNullOrWhiteSpace(text))
                        throw new Exception(text);
                    status = true;
                }
            }
            catch (Exception ex)
            {
                Debug.Assert(!status);
                if (/*OK to log?*/ LogPauseExpired)
                    try { Logger.LogError(Utilities.TextTidy(ex.ToString())); }
                    catch { }
            }
#if DEBUG
            try { Logger.LogInfo(MethodBase.GetCurrentMethod() + "(" + mode.ToString() + ", " + confirm.ToString() + ") returns " + status.ToString()); }
            catch { }
#endif
            return status;
        }

        /// <summary/>
        public bool SetStaticPulseFrequency(OperatingMode mode, uint frequency)
        {
            bool status = false;
            string traceText = MethodBase.GetCurrentMethod().Name + "(" + mode.ToString() + ", " + frequency.ToString() + ")";
            try
            {
                CommandDefinition.CommandFormat command = CommandMake(CommandEnum.StaticPulseFreq, BooleanValue.True, ActionEnum.Set, (byte)mode);
                BitConverter.GetBytes(frequency).CopyTo(command.Payload, 0);
                CommandDefinition.CommandFormat? reply = CommandSend(command, /*reply expected*/ true);
                string text = CommandReplyValidate(reply, CommandEnum.StaticPulseFreq, (byte)mode);
                if (/*fail?*/ text != string.Empty)
                    throw new Exception(traceText + "; " + text);
                if (/*fail?*/ frequency != BitConverter.ToInt32(reply.Value.Payload, 0))
                    throw new Exception(traceText + "; " + "frequency != " + frequency.ToString());
                status = true;
            }
            catch (Exception ex)
            {
                status = false;
                throw ex;
            }
            return status;
        }

        /// <summary/>
        public uint SignOfLifeSequence
        {
            get { return _signOfLifeSequence; }
            set
            {
                _signOfLifeSequence = value;
                PropertyChangeNotify("SignOfLifeSequence");
            }
        }
        private uint _signOfLifeSequence = 0;

        /// <summary/>
        public AutoResetEvent SpeedMessageEvent = new AutoResetEvent(false);
        private void SpeedMsgEventDispose()
        {
            try
            {
                if (/*exists (avoid first try exceptions)?*/ SpeedMessageEvent != null)
                    SpeedMessageEvent.Dispose();
            }
            catch { }
            finally { SpeedMessageEvent = null; }
        }
    }
}
#endif