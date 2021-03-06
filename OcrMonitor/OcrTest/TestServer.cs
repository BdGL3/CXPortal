﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

using L3.Cargo.OCR.Messages;

namespace L3.Cargo.OCR.Test
{
    public class LogEntry : INotifyPropertyChanged
    {
        public enum LogLevel { ERROR = 0,
                               INFO,
                               DEBUG,
                               TRACE
                             };

        private static string[] _LogLevels = { "ERROR",
                                               "INFO",
                                               "DEBUG",
                                               "TRACE"
                                             };
        private DateTime _timestamp;

        private LogLevel _level;

        private string _message;

        public DateTime Timestamp
        {
            get
            {
                return _timestamp;
            }

            private set
            {
                _timestamp = value;
                _NotifyPropertyChanged("Timestamp");
            }
        }

        public LogLevel Level
        {
            get
            {
                return _level;
            }

            private set
            {
                _level = value;
                _NotifyPropertyChanged("LevelString");
            }
        }

        public string LevelString
        {
            get
            {
                return _LogLevels[(int)_level];
            }
        }

        public string Message
        {
            get
            {
                return _message;
            }

            private set
            {
                _message = value;
                _NotifyPropertyChanged("Message");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void _NotifyPropertyChanged(string propertyName)
        {
            _OnPropertyChange(this, new PropertyChangedEventArgs(propertyName));
        }

        private void _OnPropertyChange(object sender, PropertyChangedEventArgs evtArgs)
        {
            if (PropertyChanged != null)
            {
                Application
                  .Current
                  .Dispatcher
                  .Invoke((Action)(() => PropertyChanged(sender, evtArgs)));
            }
        }

        public LogEntry(string msg, LogLevel lvl)
        {
            Timestamp = DateTime.Now;
            Message = msg;
            Level = lvl;
        }

       public override string ToString()
       {
           return string.Format("{0} - {1}: {2}",
                                Timestamp.ToString(),
                                LevelString,
                                Message
                               );
       }
    }

    public class MessageLog : ObservableCollection<LogEntry>
    {
        public MessageLog(): base()
        {
        }

        public void Error (string msg)
        {
            _Add(msg, LogEntry.LogLevel.ERROR);
        }

        public void Debug (string msg)
        {
            _Add(msg, LogEntry.LogLevel.DEBUG);
        }

        public void Info (string msg)
        {
            _Add(msg, LogEntry.LogLevel.INFO);
        }

        public void Trace (string msg)
        {
            _Add(msg, LogEntry.LogLevel.TRACE);
        }

        private void _Add(string msg, LogEntry.LogLevel lvl)
        {
            Application
              .Current
              .Dispatcher
              .Invoke((Action)(() => Add(new LogEntry(msg, lvl))));
        }
    }

    public class OCRTestServer : INotifyPropertyChanged
    {

        #region Public Properties

        public MessageLog Logger
        {
            get;
            private set;
        }

        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
        }

        private bool _isRunning = false;

        public string ConnectionState
        {
            get
            {
                return (_isConnected ? "Connected" : "Disconnected");
            }
        }

        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }

            private set
            {
                bool isStateChange = (_isConnected != value);
                _isConnected = value;

                if (isStateChange)
                    _PropertyChangeNotify(_ConnectionStateNAME);
            }
        }

        private bool _isConnected = false;

        private const string _ConnectionStateNAME = "ConnectionState";

        public string ReceivedMessage
        {
            get
            {
                return _rcvdMsgText;
            }

            set
            {
                _rcvdMsgText = value;
                _PropertyChangeNotify(_RcvdMsgTxtBlkNAME);
            }
        }

        private string _rcvdMsgText = "";

        private const string _RcvdMsgTxtBlkNAME = "ReceivedMessage";

        #endregion


        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion


        #region Private Members

        private IPAddress _ipAddress = null;
        private int _testPort = 0;

        private CancellationTokenSource _shutDownTokenSrc;
        private CancellationToken _shutDownToken;

        private TcpListener _listener = null;
        private Thread _listenerThread = null;
        private AutoResetEvent _listenerReady { get; set; }

        private TcpClient _testClient = null;
        private Thread _rxThread = null;
        private AutoResetEvent _rxReady { get; set; }

        private NetworkStream _testMsgStream = null;

        private OCRMessageUtils _msgUtils = null;

        private string _testSenderName;

        #endregion


        #region Public Members

        public const int WaitLimitMAXIMUM = /*10s*/ 10000 /*ms*/;

        public const int WaitLimitMINIMUM = WaitLimitMAXIMUM / 30;

        public const int WaitLimitDEFAULT = (  WaitLimitMINIMUM
                                             + WaitLimitMAXIMUM
                                            ) / 2;
        public static int WaitLimit
        {
            get
            {
                return _waitLimit;
            }

            private set
            {
                if (!WaitLimitOK(value))
                    throw new ArgumentOutOfRangeException(  "WaitLimit "
                                                          + value.ToString()
                                                          + " too small; must be in the range "
                                                          + WaitLimitMINIMUM.ToString()
                                                          + ".."
                                                          + WaitLimitMAXIMUM.ToString()
                                                         );
                _waitLimit = value;
            }
        }

        private static int _waitLimit = WaitLimitDEFAULT;

        public static bool WaitLimitOK (int limit)
        {
            return (limit >= WaitLimitMINIMUM && limit <= WaitLimitMAXIMUM);
        }

        #endregion


        #region Constructors

        public OCRTestServer (IPAddress ipAddress,
                              int port,
                              string ocrMessagePrefix,
                              string testSenderName)
        {
            Logger = new MessageLog();

            // Set the IPAddress and Port values
            _ipAddress = ipAddress;
            _testPort = port;

            _shutDownTokenSrc = new CancellationTokenSource();
            _shutDownToken = _shutDownTokenSrc.Token;

            _testSenderName = testSenderName;

            _listenerReady = new AutoResetEvent(false);
            _rxReady = new AutoResetEvent(false);

            _msgUtils = new OCRMessageUtils(ocrMessagePrefix,
                                            testSenderName
                                           );
            _isRunning = false;
            _isConnected = false;
        }

        #endregion


        #region Methods

        public void StartClientMode ()
        {
            try
            {
                Logger.Trace("Starting test client");

                // Start the client thread.
                _StartTestClient();
            }
            catch (Exception ex)
            {
                Logger.Error("_StartTestClient() exception: " + ex.Message);
            }
        }

        public void StartListenerMode ()
        {
            try
            {
                Logger.Trace("Starting test listener");

                // Start the listener thread.
                _StartTestListener();
            }
            catch (Exception ex)
            {
                Logger.Error("_StartTestListener() exception: " + ex.Message);
            }
        }

        private void _StartTestClient ()
        {
            if (!_shutDownToken.IsCancellationRequested)
            {
                if (null == _testClient)
                    _testClient = new TcpClient();

                if (!_testClient.Connected)
                {
                    _testClient.Connect(_ipAddress, _testPort);
                    _testMsgStream = _testClient.GetStream();
                    _testMsgStream.ReadTimeout = -1;
                }

                // With the client in place, start the message
                // receiving thread.
                _StartRxThread();

                _isRunning = true;
            }
        }

        private void _StartTestListener()
        {
            if (!_shutDownToken.IsCancellationRequested)
            {
                _listenerReady.Reset();

                if (null == _listenerThread)
                    _listenerThread = new Thread(new ParameterizedThreadStart(_ListenThreadMethod));

                _listenerThread.Name = "OCR Connect Listener Thread";
                _listenerThread.IsBackground = true;
                _listenerThread.Start(_shutDownToken);

                if (!_listenerReady.WaitOne(WaitLimit))
                    throw new Exception(  "OCR listener thread failed to start within "
                                        + WaitLimit.ToString()
                                        + " ms"
                                       );
                _isRunning = true;
            }
        }

        public void Shutdown()
        {
            SendUnregisterMessage();

            // Notify threads that it's time to shut down.
            _shutDownTokenSrc.Cancel();

            // Clean up threads.
            // We have been told to shut down, but we can't shut down
            // until the current message is processed.
            lock (this)
            {
                if (_rxThread != null && _rxThread.IsAlive)
                {
#                   if false
                    try
                    {
                        _rxThread.Abort();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("_rxThread.Abort() exception: " + ex.Message);
                    }
#                   endif

                    try
                    {
                        _rxThread.Join(WaitLimit);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("_rxThread.Join() exception: " + ex.Message);
                    }
                }

                _TestMsgStreamDispose();
                _rxThread = null;
                _TestClientDispose();

                if (_listenerThread != null && _listenerThread.IsAlive)
                {
#                   if false
                    try
                    {
                        _listenerThread.Abort();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("_listenerThread.Abort() exception: " + ex.Message);
                    }
#                   endif

                    try
                    {
                        _listenerThread.Join(WaitLimit);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("_listenerThread.Join() exception: " + ex.Message);
                    }
                }

                if (_listener != null)
                {
                    try
                    {
                        _listener.Stop();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("_listener.Stop() exception: " + ex.Message);
                    }
                }

                _listener = null;
                _listenerThread = null;
            }
        }

        private void _TestMsgStreamDispose ()
        {
            if (_testMsgStream != null)
            {
                try
                {
                    // Send UNREGISTER if we're acting as a client?
                }
                catch
                {
                    // bury exceptions
                }

                try
                {
                    _testMsgStream.Flush();
                }
                catch (Exception ex)
                {
                    Logger.Error("_testMsgStream.Flush() exception: " + ex.Message);
                }

                try
                {
                    _testMsgStream.Dispose();
                }
                catch (Exception ex)
                {
                    Logger.Error("_testMsgStream.Dispose() exception: " + ex.Message);
                }

            _testMsgStream = null;
            }
        }

        protected void _TestClientDispose ()
        {
            if (_testClient != null)
            {
                try
                {
                    _testClient.Close();
                }
                catch (Exception ex)
                {
                    Logger.Error("_testClient.Dispose() exception: " + ex.Message);
                }

                try
                {
                    _testClient = null;
                }
                catch (Exception ex)
                {
                    Logger.Error("_testClient assign null exception: " + ex.Message);
                }
            }

            _testClient = null;
            IsConnected = false;
        }

        private void _ListenThreadMethod (object param)
        {
            CancellationToken canxToken = (CancellationToken)param;

            try
            {
                _listener = new TcpListener(_ipAddress, _testPort);

                // Start listening for the OCR client system to connect.
                _listener.Start();

                _listenerReady.Set();

                while (!canxToken.IsCancellationRequested)
                {
                    // If there are any active connections kill them.
                    // Accept client connections and thread off a
                    // connection handler thread.
                    TcpClient client = _listener.AcceptTcpClient();

                    lock (this)
                    {
                        if (_testClient != null)
                        {
                            // This is a secondary connection and we
                            // only allow one. Something went wrong
                            // with the last one.
                            Logger.Error("_ListenThreadMethod: _testClient != null");
                            Shutdown();
                        }
                        else
                        {
                            _testClient = client;
                            _testMsgStream = _testClient.GetStream();
                            _testMsgStream.ReadTimeout = -1;

                            // Now that the connection to the OCR
                            // system has been established, start
                            // the OCR messages receive thread.
                            _StartRxThread();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("_ListenThreadMethod() exception: " + ex.Message);
            }
        }

        private void _StartRxThread ()
        {
            IsConnected = _testClient.Connected;

            _rxReady.Reset();

            if (null == _rxThread)
                _rxThread = new Thread(new ParameterizedThreadStart(ReceiveMessagesThreadMethod));

            _rxThread.Name = "Receive Messages Thread";
            _rxThread.IsBackground = true;
            _rxThread.Start(_shutDownToken);

            if (!_rxReady.WaitOne(WaitLimit))
                throw new Exception(  "Receive messages thread failed to start within "
                                    + WaitLimit.ToString()
                                    + " ms"
                                   );
        }

        private void ReceiveMessagesThreadMethod(object param)
        {
            CancellationToken canxToken = (CancellationToken)param;

            // 128 KiB - totally arbitrary buffer size
            byte[] rcvBufr = new byte[128*1024];
            int numBytesReceived = 0;
            string msgPackage = String.Empty;
            Encoding enc = new UTF8Encoding(true, true);

            _rxReady.Set();

            Logger.Trace("OCR client connected to server");

            try
            {
                // Test client connection was just established, so send
                // a REGISTER message.
                if (!canxToken.IsCancellationRequested)
                    Send(RegisterMessage(), OCRMessageUtils.MessageName(OCRMessageType.REGISTER));

                // Just stay in the loop, instance owner's thread will
                // shut down this thread.
                while (!canxToken.IsCancellationRequested && IsConnected)
                {
                    // Receive a message from client. This call will
                    // block until the stream has the message.
                    numBytesReceived = _testMsgStream.Read(rcvBufr,
                                                           0,
                                                           rcvBufr.Length
                                                          );
                    if (numBytesReceived <= 0)
                    {   // The remote client closed, bail.
                        break;
                    }

                    // Encode the received bytes into a UTF8 string.
                    msgPackage = enc.GetString(rcvBufr,
                                               0,
                                               numBytesReceived
                                              );
                    try
                    {
                        ProcessOCRMessage(RemoveEnvelope(msgPackage));
                    }
                    catch (ArgumentException ex)
                    {
                        Logger.Error("ProcessOCRMessage() ArgumentException: " + ex.Message);
                    }

                    IsConnected = (_testClient != null
                                     ? _testClient.Connected
                                     : false
                                  );
                }
            }
            catch (IOException ex)
            {
                // Log IO Exceptions
                Logger.Error("ReceiveMessagesThreadMethod() IOException: " + ex.Message);
            }
        }

        /// <summary>
        /// Strip the start and end chars from the message
        ///   [StartChar]: 0x02
        ///   [EndChar]:   0x03
        /// </summary>
        /// <param name="msgPackage">string message including envelope</param>
        /// <returns>message (string) with the envelope removed</returns>
        private string RemoveEnvelope(string msgPackage)
        {
            if (Convert.ToUInt32(msgPackage[0]) != 2)
                throw new ArgumentException(  "OCR message starts with value 0x"
                                            + Convert.ToUInt32(msgPackage[0]).ToString("X2")
                                            + ", not 0x02 (OCR message start char)"
                                           );
            msgPackage = msgPackage.Remove(0, 1);

            if (Convert.ToUInt32(msgPackage[msgPackage.Length - 1]) != 3)
                throw new ArgumentException(  "OCR message ends with value 0x"
                                            + Convert.ToUInt32(msgPackage[msgPackage.Length - 1]).ToString("X2")
                                            + ", not 0x03 (OCR message end char)"
                                           );
            return msgPackage.Remove(msgPackage.Length - 1, 1);
        }

        public void ProcessOCRMessage(string msgString)
        {
            string guidString = null;

            // Don't allow shutdown while still processing a message
            try
            {
                lock (this)
                {
                    OCRMessageType messageType = OCRMessageUtils
                                                   .GetMessageInfo(msgString,
                                                                   out guidString
                                                                  );
                    object ocrMessage = null;

                    switch (messageType)
                    {
                        case OCRMessageType.REGISTERED:
                            ocrMessage = new Messages.ocr_register.message();
                            break;

                        case OCRMessageType.UNREGISTERED:
                            ocrMessage = new Messages.ocr_unregister.message();
                            break;

                        case OCRMessageType.PING:
                            ocrMessage = new Messages.ocr_ping.message();
                            break;

                        case OCRMessageType.ACK:
                            ocrMessage = new Messages.ocr_ack.message();
                            break;

                        case OCRMessageType.NACK:
                            ocrMessage = new Messages.ocr_nack.message();
                            break;

                        default:
                            // Per the ICD, ignore anything else.
                            break;
                    }

                    OCRMessageRespond(DeserializeToTypedMessage(ocrMessage,
                                                                msgString),
                                      messageType
                                     );
                }
            }
            catch (Exception ex) //XML parser and ProcessMasterMessage will throw in invalid message
            {
                Logger.Error("ProcessOCRMessage() exception: " + ex.Message);

                // Per the ICD, if something is corrupt or unrecognized,
                // just NACK it.
                SendNACK(guidString);
            }
        }

        private object DeserializeToTypedMessage(object typedMsg,
                                                 string xmlString)
        {
            if (typedMsg != null)
            {
                try
                {   // Attempt to deserialize the object.
                    XmlSerializer msgSerializer = new XmlSerializer(typedMsg.GetType());
                    TextReader xmlReader = new StringReader(xmlString);

                    typedMsg = Convert.ChangeType(msgSerializer.Deserialize(xmlReader),
                                                  typedMsg.GetType()
                                                 );
                }
                catch (Exception ex)
                {   // We received a malformed message.
                    Logger.Error("DeserializeToTypedMessage() exception: " + ex.Message);

                    typedMsg = null;
                }
            }

            return typedMsg;
        }

        private void OCRMessageRespond(object messageObj,
                                       OCRMessageType msgType)
        {
            try
            {
                ReceivedMessage = OCRMessageUtils.MessageName(msgType);

                switch (msgType)
                {
                    case OCRMessageType.REGISTERED:
                        break;

                    case OCRMessageType.UNREGISTERED:
                        Shutdown();
                        break;

                    case OCRMessageType.PING:
                        SendACK(((Messages.ocr_ping.message)messageObj).body.guid);
                        break;

                    case OCRMessageType.ACK:
                        // Received Ack Message, no response necessary.
                        break;

                    case OCRMessageType.NACK:
                        // Received Nack Message, no response necessary.
                        break;

                    default:
                        // Ignore anything else.
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("OCRMessageRespond() exception: " + ex.Message);
            }
        }

        private void SendNACK(string guid)
        {
            Send(OCRMessageUtils.NackMessage(guid), OCRMessageUtils.MessageName(OCRMessageType.NACK));
        }

        private void SendACK(string guid)
        {
            Send(OCRMessageUtils.AckMessage(guid), OCRMessageUtils.MessageName(OCRMessageType.ACK));
        }

        private void Send(object messageObj, string msgName)
        {
            MemoryStream ms =  null;

            if (messageObj != null)
            {
                ms = OCRMessageUtils.writeMessage(messageObj);
            }

            if (ms != null)
            {
                lock (_testMsgStream)
                {
                    _testMsgStream.Write(ms.ToArray(),
                                         0,
                                         ms.ToArray().Length
                                        );
                }
            }

            Logger.Trace("Sent: " + msgName);
        }

        /// <summary>
        /// Function that creates a REGISTER message
        /// </summary>
        /// <param></param>
        /// <returns>Register message in XML format</returns>
        private object RegisterMessage()
        {
            try
            {
                // Initialize all of the classes we'll need to construct the XML
                Messages.ocr_register.message ocrMessage = new Messages.ocr_register.message();
                ocrMessage.name = OCRMessageUtils.MessageName(OCRMessageType.REGISTER);
                ocrMessage.sentDateTime = DateTime.Now;

                ocrMessage.addressInfo = new Messages.ocr_register.messageAddressInfo();

                ocrMessage.addressInfo.sender = new Messages.ocr_register.messageAddressInfoSender();
                ocrMessage.addressInfo.sender.name = _testSenderName;

                ocrMessage.body = new Messages.ocr_register.messageBody();
                ocrMessage.body.guid = System.Guid.NewGuid().ToString();
                ocrMessage.body.ackRequired = false;
                ocrMessage.body.messageType = "protocol";

                ocrMessage.body.payload = new Messages.ocr_register.messageBodyPayload();
                ocrMessage.body.payload.base64Encoded = false;

                return ocrMessage;
            }
            catch (Exception ex)
            {
                Logger.Error("RegisterMessage() exception: " + ex.Message);

                return null;
            }
        }

        /// <summary>
        /// Function that creates an UNREGISTER message
        /// </summary>
        /// <returns>
        /// Unregister message in XML format
        /// </returns>
        public void SendUnregisterMessage()
        {
            try
            {
                // Initialize all of the classes we'll need to construct the XML
                Messages.ocr_unregister.message ocrMessage = new Messages.ocr_unregister.message();
                ocrMessage.name = OCRMessageUtils.MessageName(OCRMessageType.UNREGISTER);
                ocrMessage.sentDateTime = DateTime.Now;

                ocrMessage.addressInfo = new Messages.ocr_unregister.messageAddressInfo();

                ocrMessage.addressInfo.sender = new Messages.ocr_unregister.messageAddressInfoSender();
                ocrMessage.addressInfo.sender.name = _testSenderName;

                ocrMessage.body = new Messages.ocr_unregister.messageBody();
                ocrMessage.body.guid = System.Guid.NewGuid().ToString();
                ocrMessage.body.ackRequired = false;
                ocrMessage.body.messageType = "protocol";

                ocrMessage.body.payload = new Messages.ocr_unregister.messageBodyPayload();
                ocrMessage.body.payload.base64Encoded = false;

                Send(ocrMessage, OCRMessageUtils.MessageName(OCRMessageType.UNREGISTER));
            }
            catch (Exception ex)
            {
                Logger.Error("SendUnregisterMessage() exception: " + ex.Message);
            }
        }

        private void _PropertyChangeNotify(string name)
        {
            _OnPropertyChange(this, new PropertyChangedEventArgs(name));
        }

        private void _OnPropertyChange(object sender, PropertyChangedEventArgs evtArgs)
        {
            if (PropertyChanged != null)
                Application
                  .Current
                  .Dispatcher
                  .Invoke((Action)(() => PropertyChanged(sender, evtArgs)));
        }

        #endregion
    }
}
