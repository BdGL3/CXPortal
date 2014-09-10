using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Windows.Forms;

using L3.Cargo.OCR.Messages;

namespace L3.Cargo.OCR.Interfaces
{
    public class OCRMonitor
    {

        #region Public Properties

        public bool IsRunning
        {
            get { return _isRunning; }
        }

        private bool _isRunning = false;

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

                if (isStateChange && UpdateConnectionStatus != null)
                    UpdateConnectionStatus(value);
            }
        }

        private bool _isConnected = false;

        #endregion

        #region Private Members

        private IPAddress _ipAddress = null;
        private int _ocrPort = 0;

        private bool _connectAsListener = false;

        private CancellationTokenSource _shutDownTokenSrc;
        private CancellationToken _shutDownToken;

        private TcpListener _listener = null;
        private Thread _listenerThread = null;
        private AutoResetEvent _listenerReady { get; set; }

        private TcpClient _ocrClient = null;
        private Thread _ocrRxThread = null;
        private AutoResetEvent _ocrRxReady { get; set; }

        private NetworkStream _ocrMsgStream = null;

        private OCRMessageUtils _msgUtils = null;

        #endregion


        #region Delegates

        public delegate void OCRConnectionStatusNotifier(bool isConnected);

        public delegate void OCRMessageHandler(object objMessage,
                                               string msgName);
        #endregion


        #region Events

        public event OCRConnectionStatusNotifier UpdateConnectionStatus;

        public event OCRMessageHandler NotifyRxACK;
        public event OCRMessageHandler NotifyRxNACK;
        public event OCRMessageHandler NotifyRxOCR_MASTER;
        public event OCRMessageHandler NotifyRxOCR_NEW_EVENT;
        public event OCRMessageHandler NotifyRxOCR_ULD;
        public event OCRMessageHandler NotifyRxPING;
        public event OCRMessageHandler NotifyRxREGISTER;
        public event OCRMessageHandler NotifyRxUNREGISTER;

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
            return (limit >= WaitLimitMINIMUM) && (limit <= WaitLimitMAXIMUM);
        }

        #endregion


        #region Constructors

        public OCRMonitor(IPAddress ipAddress,
                          int port,
                          bool connectAsListener,
                          string ocrMessagePrefix,
                          string cargoSenderName)
        {
            // Set the IPAddress and Port values
            _ipAddress = ipAddress;
            _ocrPort = port;
            _connectAsListener = connectAsListener;

            _shutDownTokenSrc = new CancellationTokenSource();
            _shutDownToken = _shutDownTokenSrc.Token;

            _listenerReady = new AutoResetEvent(false);
            _ocrRxReady = new AutoResetEvent(false);

            _msgUtils = new OCRMessageUtils(ocrMessagePrefix,
                                            cargoSenderName
                                           );
            _isRunning = false;
            _isConnected = false;
        }

        #endregion


        #region Methods

        public void Start()
        {
            try
            {
                if (_connectAsListener)
                {
                    // Start the listener thread.
                    _StartOcrListener();
                }
                else
                {
                    _OcrServerConnect();
                }
            }
            catch
            {
                // bury any exceptions
            }
        }

        private void _StartOcrListener()
        {
            if (!_shutDownToken.IsCancellationRequested)
            {
                try
                {
                    _listenerReady.Reset();

                    if (null == _listenerThread)
                        _listenerThread = new Thread(new ParameterizedThreadStart(_ListenThreadMethod));

                    _listenerThread.Name = "OCR Connect Listener Thread";
                    _listenerThread.IsBackground = true;
                    _listenerThread.Start(_shutDownToken);

                    if (!_listenerReady.WaitOne(WaitLimit))
                        throw new Exception("OCR listener thread failed to start within "
                                            + WaitLimit.ToString()
                                            + " ms"
                                           );
                    _isRunning = true;
                }
                catch
                {
                    // bury any exceptions
                }
            }
        }

        private void _OcrServerConnect ()
        {
            if (!_shutDownToken.IsCancellationRequested)
            {
                try
                {
                    if (null == _ocrClient)
                        _ocrClient = new TcpClient();

                    if (!_ocrClient.Connected)
                    {
                        _ocrClient.Connect(_ipAddress, _ocrPort);
                        _ocrMsgStream = _ocrClient.GetStream();
                        _ocrMsgStream.ReadTimeout = -1;
                    }

                    // With the client in place, start the OCR message
                    // receiving thread.
                    _StartOcrRxThread();

                    _isRunning = true;
                }
                catch
                {
                    // bury any exceptions
                }
            }
        }

        public void Shutdown()
        {
            // Notify threads that it's time to shut down.
            _shutDownTokenSrc.Cancel();

            // Clean up threads.
            // We have been told to shut down, but we can't shut down
            // until the current message is processed.
            lock (this)
            {
                if (_ocrRxThread != null && _ocrRxThread.IsAlive)
                {
                    try
                    {
                        _ocrRxThread.Abort();
                    }
                    catch
                    {
                        // bury any exceptions
                    }

                    try
                    {
                        _ocrRxThread.Join(WaitLimit);
                    }
                    catch
                    {
                        // bury any exceptions
                    }
                }

                _OcrMsgStreamDispose();
                _ocrRxThread = null;
                _OcrClientDispose ();

                if (_listenerThread != null && _listenerThread.IsAlive)
                {
                    try
                    {
                        _listenerThread.Abort();
                    }
                    catch
                    {
                        // bury any exceptions
                    }

                    try
                    {
                        _listenerThread.Join(WaitLimit);
                    }
                    catch
                    {
                        // bury any exceptions
                    }
                }

                if (_listener != null)
                {
                    try
                    {
                        _listener.Stop();
                    }
                    catch
                    {
                        // bury any exceptions
                    }
                }

                _listener = null;
                _listenerThread = null;
            }
        }

        private void _OcrMsgStreamDispose ()
        {
            if (_ocrMsgStream != null)
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
                    _ocrMsgStream.Flush();
                }
                catch
                {
                    // bury exceptions
                }

                try
                {
                    _ocrMsgStream.Dispose();
                }
                catch
                {
                    // bury exceptions
                }

            _ocrMsgStream = null;
            }
        }

        protected void _OcrClientDispose ()
        {
            if (_ocrClient != null)
            {
                try
                {
                    _ocrClient.Close();
                }
                catch
                {
                    // bury any exceptions
                }

                try
                {
                    _ocrClient = null;
                }
                catch
                {
                    // bury any exceptions
                }
            }

            _ocrClient = null;
            IsConnected = false;
        }

        private void _ListenThreadMethod (object param)
        {
            CancellationToken canxToken = (CancellationToken)param;

            try
            {
                _listener = new TcpListener(_ipAddress, _ocrPort);

                // Start listening for the OCR system to connect.
                _listener.Start();

                while (!canxToken.IsCancellationRequested)
                {
                    // If there are any active connections kill them.
                    // Accept client connections and thread off a
                    // connection handler thread.
                    TcpClient client = _listener.AcceptTcpClient();

                    lock (this)
                    {
                        if (_ocrClient != null)
                        {
                            // This is a secondary connection and we
                            // only allow one. Something went wrong
                            // with the last one.
                            Shutdown();
                        }

                        _ocrClient = client;
                        _ocrMsgStream = _ocrClient.GetStream();
                        _ocrMsgStream.ReadTimeout = -1;

                        // Now that the connection to the OCR system has
                        // been established, start the OCR messages
                        // receive thread.
                        _StartOcrRxThread();
                    }
                }
            }
            catch
            {
                // bury any exceptions
            }
        }

        private void _StartOcrRxThread ()
        {
            IsConnected = _ocrClient.Connected;

            _ocrRxReady.Reset();

            if (null == _ocrRxThread)
                _ocrRxThread = new Thread(new ParameterizedThreadStart(ReceiveMessagesThreadMethod));

            _ocrRxThread.Name = "Receive OCR Messages Thread";
            _ocrRxThread.IsBackground = true;
            _ocrRxThread.Start(_shutDownToken);

            if (!_ocrRxReady.WaitOne(WaitLimit))
                throw new Exception(  "Receive OCR messages thread failed to start within "
                                    + WaitLimit.ToString()
                                    + " ms"
                                   );
        }

        private void ReceiveMessagesThreadMethod(object param)
        {
            CancellationToken canxToken = (CancellationToken)param;

            // 1 MiB - totally arbitrary buffer size
            byte[] rcvBufr = new byte[1024*1024];
            int numBytesReceived = 0;
            string msgPackage = String.Empty;
            Encoding enc = new UTF8Encoding(true, true);

            _ocrRxReady.Set();

            try
            {
                // Just stay in the loop, instance owner's thread will
                // shut down this thread.
                while (!canxToken.IsCancellationRequested)
                {
                    // Receive a message from client. This call will
                    // block until the stream has the message.
                    numBytesReceived = _ocrMsgStream.Read(rcvBufr,
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
                        // TODO: log it when a logger is added
                    }
                }
            }
            catch (IOException)
            {
                // Handle sources of IO Exceptions
                // More than likely because the connection was closed as
                // part of shutdown, so bury the exception and bail. We
                // can catch more specific things if other errors pop up.
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
                        case OCRMessageType.OCR_MASTER:
                            ocrMessage = new Messages.ocr_master.message();
                            break;

                        case OCRMessageType.OCR_ULD:
                            ocrMessage = new Messages.ocr_uld.message();
                            break;

                        case OCRMessageType.OCR_NEW_EVENT:
                            ocrMessage = new Messages.ocr_new_event.message();
                            break;

                        case OCRMessageType.REGISTER:
                            ocrMessage = new Messages.ocr_register.message();
                            break;

                        case OCRMessageType.UNREGISTER:
                            ocrMessage = new Messages.ocr_unregister.message();
                            break;

                        case OCRMessageType.PING:
                            ocrMessage = new Messages.ocr_ping.message();
                            break;

                        case OCRMessageType.ACK:
                            ocrMessage = new Messages.ocr_ack.message();
                            break;

                        default:
                            // If something is unrecognized or corrupt,
                            // just ignore it per the ICD
                            break;
                    }

                    OCRMessageRespond(DeserializeToTypedMessage(ocrMessage, msgString), messageType);
                }
            }
            catch (Exception) //XML parser and ProcessMasterMessage will throw in invalid message
            {
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
                catch (Exception)
                {   // We received a malformed message.
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
                string msgName = OCRMessageUtils.MessageName(msgType);

                switch (msgType)
                {
                    case OCRMessageType.OCR_MASTER:
                        SendACK(((Messages.ocr_master.message)messageObj).body.guid);
                        // Let event subscribers know we received and ACK'd an OCR_MASTER message.
                        if (NotifyRxOCR_MASTER != null)
                            NotifyRxOCR_MASTER(messageObj, msgName);
                        break;

                    case OCRMessageType.OCR_ULD:
                        SendACK(((Messages.ocr_uld.message)messageObj).body.guid);
                        // Let event subscribers know we received and ACK'd an OCR_ULD message.
                        if (NotifyRxOCR_ULD != null)
                            NotifyRxOCR_ULD(messageObj, msgName);
                        break;

                    case OCRMessageType.OCR_NEW_EVENT:
                        SendACK(((Messages.ocr_new_event.message)messageObj).body.guid);
                        // Let event subscribers know we received and ACK'd an OCR_NEW_EVENT message.
                        if (NotifyRxOCR_NEW_EVENT != null)
                            NotifyRxOCR_NEW_EVENT(messageObj, msgName);
                        break;

                    case OCRMessageType.REGISTER:
                        Send(OCRMessageUtils.RegisteredMessage());
                        // Let event subscribers know we received and ACK'd a REGISTER message.
                        if (NotifyRxREGISTER != null)
                            NotifyRxREGISTER(messageObj, msgName);
                        break;

                    case OCRMessageType.UNREGISTER:
                        Send(OCRMessageUtils.UnregisteredMessage());
                        // Let event subscribers know we received and ACK'd an UNREGISTER message.
                        if (NotifyRxUNREGISTER != null)
                            NotifyRxUNREGISTER(messageObj, msgName);

                        Shutdown();
                        break;

                    case OCRMessageType.PING:
                        SendACK(((Messages.ocr_ping.message)messageObj).body.guid);
                        // Let event subscribers know we received and ACK'd a PING message.
                        if (NotifyRxPING != null)
                            NotifyRxPING(messageObj, msgName);
                        break;

                    case OCRMessageType.ACK:
                        // Received Ack Message, no response necessary
                        // Let event subscribers know we received an ACK message.
                        if (NotifyRxACK != null)
                            NotifyRxACK(messageObj, msgName);
                        break;

                    case OCRMessageType.NACK:
                        // Received Nack Message, no response necessary
                        // Let event subscribers know we received an NACK message.
                        if (NotifyRxNACK != null)
                            NotifyRxNACK(messageObj, msgName);
                        break;

                    default:
                        // If something is unrecognized or corrupt,
                        // just ignore it per the ICD.
                        break;
                }
            }
            catch (Exception) //XML parser and ProcessMasterMessage will throw in invalid message
            {
                // bury any exceptions
            }
        }

        private void SendNACK(string guid)
        {
            Send(OCRMessageUtils.NackMessage(guid));
        }

        private void SendACK(string guid)
        {
            Send(OCRMessageUtils.AckMessage(guid));
        }

        public void SendNewEventResponse(object messageObj, string caseID)
        {
            // Send an OCR_NEW_EVENT_RESPONSE message
            Send(OCRMessageUtils
                   .NewEventResponseMessage(((Messages.ocr_new_event
                                                .message
                                             )messageObj
                                            ).body
                                             .payload
                                             .EventID,
                                            ((Messages.ocr_new_event
                                                .message
                                             )messageObj
                                            ).body
                                             .payload
                                             .LaneID,
                                            caseID
                                           )
                );
        }

        private void Send(object messageObj)
        {
            MemoryStream ms =  null;

            if (messageObj != null)
            {
                ms = OCRMessageUtils.writeMessage(messageObj);
            }

            if (ms != null)
            {
                _ocrMsgStream.Write(ms.ToArray(),
                                    0,
                                    ms.ToArray().Length
                                   );
            }
        }

        #endregion
    }
}
