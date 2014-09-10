using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Serialization;

namespace L3.Cargo.OCR.Messages
{
    public enum OCRMessageType { UNRECOGNIZED = 0,
                                 ACK,
                                 NACK,
                                 OCR_MASTER,
                                 OCR_NEW_EVENT,
                                 OCR_NEW_EVENT_RESPONSE,
                                 OCR_ULD,
                                 PING,
                                 REGISTER,
                                 REGISTERED,
                                 UNREGISTER,
                                 UNREGISTERED
                               };

    public class OCRMessageUtils
    {
        public class TwoWayMap<T1, T2>
        {
            public class Mapping<KT, VT>
            {
                public VT this[KT key]
                {
                    get { return _dictionary[key]; }
                    set { _dictionary[key] = value; }
                }

                private Dictionary<KT, VT> _dictionary;

                public Mapping(Dictionary<KT, VT> dict)
                {
                    _dictionary = dict;
                }

                public bool TryGetValue(KT key, out VT value)
                {
                    return _dictionary.TryGetValue(key, out value);
                }
            }

            // Forward: map key of type T1 to value of type T2
            public Mapping<T1, T2> Forward { get; private set; }

            private Dictionary<T1, T2> _forward = new Dictionary<T1, T2>();

            // Reverse: map key of type T2 to value of type T1
            public Mapping<T2, T1> Reverse { get; private set; }

            private Dictionary<T2, T1> _reverse = new Dictionary<T2, T1>();

            public T2 this[T1 key]
            {
                get { return _forward[key]; }
                set { _forward[key] = value; }
            }


            public T1 this[T2 key]
            {
                get { return _reverse[key]; }
                set { _reverse[key] = value; }
            }

            public TwoWayMap()
            {
                this.Forward = new Mapping<T1, T2>(_forward);
                this.Reverse = new Mapping<T2, T1>(_reverse);
            }

            public void Add(T1 fwdKey, T2 revKey)
            {
                if (!_forward.ContainsKey(fwdKey))
                    _forward.Add(fwdKey, revKey);

                if (!_reverse.ContainsKey(revKey))
                    _reverse.Add(revKey, fwdKey);
            }

            public bool TryGetValue(T1 fwdKey, out T2 fwdValue)
            {
                return _forward.TryGetValue(fwdKey, out fwdValue);
            }

            public bool TryGetValue(T2 revKey, out T1 revValue)
            {
                return _reverse.TryGetValue(revKey, out revValue);
            }
        }


        #region Private Members

        private static TwoWayMap<string,
                                 OCRMessageType
                                > _ocrMsgTypeMap = new TwoWayMap<string,
                                                                 OCRMessageType
                                                                >();
        private static string _senderName;

        #endregion


        #region Constructors

        /// <summary>
        /// Initialize static members of the OCRMessageUtils class
        /// </summary>
        public OCRMessageUtils(string ocrMsgPrefix, string senderName)
        {
            _ocrMsgTypeMap.Add("ACK", OCRMessageType.ACK);
            _ocrMsgTypeMap.Add("NACK", OCRMessageType.NACK);
            _ocrMsgTypeMap.Add(ocrMsgPrefix + "_MASTER",
                               OCRMessageType.OCR_MASTER
                              );
            _ocrMsgTypeMap.Add(ocrMsgPrefix + "_NEW_EVENT",
                               OCRMessageType.OCR_NEW_EVENT
                              );
            _ocrMsgTypeMap.Add(ocrMsgPrefix + "_NEW_EVENT_RESPONSE",
                               OCRMessageType.OCR_NEW_EVENT_RESPONSE
                              );
            _ocrMsgTypeMap.Add(ocrMsgPrefix + "_ULD",
                               OCRMessageType.OCR_ULD
                              );
            _ocrMsgTypeMap.Add("PING", OCRMessageType.PING);
            _ocrMsgTypeMap.Add("REGISTER", OCRMessageType.REGISTER);
            _ocrMsgTypeMap.Add("REGISTERED", OCRMessageType.REGISTERED);
            _ocrMsgTypeMap.Add("UNRECOGNIZED", OCRMessageType.UNRECOGNIZED);
            _ocrMsgTypeMap.Add("UNREGISTER", OCRMessageType.UNREGISTER);
            _ocrMsgTypeMap.Add("UNREGISTERED", OCRMessageType.UNREGISTERED);

            _senderName = senderName;
        }

        #endregion


        #region Public Methods

        public static string MessageName(OCRMessageType msgType)
        {
            string msgName = null;

            _ocrMsgTypeMap.TryGetValue(msgType, out msgName);
            return msgName;
        }

        public static OCRMessageType MessageType(string msgName)
        {
            OCRMessageType msgType = OCRMessageType.UNRECOGNIZED;

            return (_ocrMsgTypeMap.TryGetValue(msgName,
                                               out msgType)
                      ? msgType
                      : OCRMessageType.UNRECOGNIZED
                   );
        }

        /// <summary>
        /// Method for getting a message's type (OCR_MASTER, Ping, ...)
        /// and guid
        /// </summary>
        /// <param name="xmlString">message string</param>
        /// <returns>message type</returns>
        public static OCRMessageType GetMessageInfo(string xmlMessage,
                                                    out string guid)
        {
            guid = null;

            try
            {
                XmlDocument doc = new XmlDocument();

                // Parse the message.
                doc.InnerXml = xmlMessage;

                // Get the elements in the XML message.
                XmlElement root = doc.DocumentElement;

                // Get the message's guid.
                // If it doesn't have a guid, guid is set to null.
                guid = ((XmlAttribute)root.GetElementsByTagName("body")[0]
                                          .Attributes
                                          .GetNamedItem("guid")
                       ).Value;

                // Look up and return the OCR message type.
                return OCRMessageUtils.MessageType(root.GetAttribute("name"));
            }
            catch
            {
                return OCRMessageType.UNRECOGNIZED;
            }
        }

        /// <summary>
        /// Method to serialize the message to XML and output the message
        /// XML to a MemoryStream.
        /// </summary>
        /// <param name="messageObj">the typed message object</param>
        /// <returns>
        /// MemoryStream containing the message XML
        /// </returns>
        public static MemoryStream writeMessage(object messageObj)
        {
            try
            {
                // Create the Memory Stream
                MemoryStream ms = new MemoryStream();

                // Write the message start character
                ms.WriteByte(0x02);

                XmlSerializer xmls = new XmlSerializer(messageObj.GetType());

                // Specify how we want the message XML to be output
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Encoding = Encoding.UTF8;
                settings.Indent = false;
                settings.NewLineChars = Environment.NewLine;
                settings.ConformanceLevel = ConformanceLevel.Document;
                XmlWriter writer = XmlTextWriter.Create(ms, settings);

                // Create a blank namespace so the XmlSerializer doesn't
                // add one for us.
                XmlSerializerNamespaces xns = new XmlSerializerNamespaces();
                xns.Add("", "");

                // Finally, serialize the message to the memory stream
                // using the XmlWriter instance.
                xmls.Serialize(writer, messageObj, xns);

                // Write the message end character
                ms.WriteByte(0x03);

                return ms;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Function that creates an ACK message
        /// </summary>
        /// <param name="sGuid">the recived guid</param>
        /// <returns>Acknowledge message in XML format</returns>
        public static object AckMessage(string guid)
        {
            try
            {
                // Initialize all of the classes we'll need to construct the XML
                ocr_ack.message ocrMessage = new ocr_ack.message();
                ocrMessage.name = _ocrMsgTypeMap[OCRMessageType.ACK];
                ocrMessage.sentDateTime = DateTime.Now;

                ocrMessage.addressInfo = new ocr_ack.messageAddressInfo();

                ocrMessage.addressInfo.sender = new ocr_ack.messageAddressInfoSender();
                ocrMessage.addressInfo.sender.name = _senderName;

                ocrMessage.body = new ocr_ack.messageBody();
                ocrMessage.body.guid = guid;
                ocrMessage.body.ackRequired = false;
                ocrMessage.body.messageType = "protocol";
                ocrMessage.body.payload = "";

                return ocrMessage;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Function that creates a NACK message
        /// </summary>
        /// <param name="sGuid">the received guid</param>
        /// <returns>Acknowledge message in XML format</returns>
        public static object NackMessage(string guid)
        {
            try
            {
                // Initialize all of the classes we'll need to construct the XML
                ocr_nack.message ocrMessage = new ocr_nack.message();
                ocrMessage.name = _ocrMsgTypeMap[OCRMessageType.NACK];
                ocrMessage.sentDateTime = DateTime.Now;

                ocrMessage.addressInfo = new ocr_nack.messageAddressInfo();

                ocrMessage.addressInfo.sender = new ocr_nack.messageAddressInfoSender();
                ocrMessage.addressInfo.sender.name = _senderName;

                ocrMessage.body = new ocr_nack.messageBody();
                ocrMessage.body.guid = guid;
                ocrMessage.body.ackRequired = false;
                ocrMessage.body.messageType = "protocol";
                ocrMessage.body.payload = "";

                return ocrMessage;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Function that creates a REGISTERED message
        /// </summary>
        /// <param></param>
        /// <returns>Registered message in XML format</returns>
        public static object RegisteredMessage()
        {
            try
            {
                // Initialize all of the classes we'll need to construct the XML
                ocr_register.message ocrMessage = new ocr_register.message();
                ocrMessage.name = _ocrMsgTypeMap[OCRMessageType.REGISTERED];
                ocrMessage.sentDateTime = DateTime.Now;

                ocrMessage.addressInfo = new ocr_register.messageAddressInfo();

                ocrMessage.addressInfo.sender = new ocr_register.messageAddressInfoSender();
                ocrMessage.addressInfo.sender.name = _senderName;

                ocrMessage.body = new ocr_register.messageBody();
                ocrMessage.body.guid = System.Guid.NewGuid().ToString();
                ocrMessage.body.ackRequired = false;
                ocrMessage.body.messageType = "protocol";

                ocrMessage.body.payload = new ocr_register.messageBodyPayload();
                ocrMessage.body.payload.base64Encoded = false;

                return ocrMessage;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Function that creates an UNREGISTERED message
        /// </summary>
        /// <returns>Acknowledge message in XML format</returns>
        public static object UnregisteredMessage()
        {
            try
            {
                // Initialize all of the classes we'll need to construct the XML
                ocr_unregister.message ocrMessage = new ocr_unregister.message();
                ocrMessage.name = _ocrMsgTypeMap[OCRMessageType.UNREGISTERED];
                ocrMessage.sentDateTime = DateTime.Now;

                ocrMessage.addressInfo = new ocr_unregister.messageAddressInfo();

                ocrMessage.addressInfo.sender = new ocr_unregister.messageAddressInfoSender();
                ocrMessage.addressInfo.sender.name = _senderName;

                ocrMessage.body = new ocr_unregister.messageBody();
                ocrMessage.body.guid = System.Guid.NewGuid().ToString();
                ocrMessage.body.ackRequired = false;
                ocrMessage.body.messageType = "protocol";

                ocrMessage.body.payload = new ocr_unregister.messageBodyPayload();
                ocrMessage.body.payload.base64Encoded = false;

                return ocrMessage;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Function that creates a NEW_EVENT_RESPONSE message
        /// </summary>
        /// <param name="eventId">event ID from OCR server</param>
        /// <param name="laneId">lane ID from OCR server</param>
        /// <param name="caseNumber">newly created case ID</param>
        /// <returns>OCR_NEW_EVENT_RESPONSE message in XML format</returns>
        public static object NewEventResponseMessage(ushort eventId,
                                                     ushort laneId,
                                                     string caseNumber)
        {
           try
           {
               // Construct the XML message object
               ocr_new_event_response.message ocrMessage = new ocr_new_event_response.message();
               ocrMessage.name = _ocrMsgTypeMap[OCRMessageType.OCR_NEW_EVENT_RESPONSE];
               ocrMessage.sentDateTime = DateTime.Now;

               ocrMessage.addressInfo = new ocr_new_event_response.messageAddressInfo();

               ocrMessage.addressInfo.sender = new ocr_new_event_response.messageAddressInfoSender();
               ocrMessage.addressInfo.sender.name = _senderName;

               ocrMessage.body = new ocr_new_event_response.messageBody();
               ocrMessage.body.guid = System.Guid.NewGuid().ToString();
               ocrMessage.body.ackRequired = false;
               ocrMessage.body.messageType = "protocol";

               ocrMessage.body.payload = new ocr_new_event_response.messageBodyPayload();
               ocrMessage.body.payload.base64Encoded = false;
               ocrMessage.body.payload.EventID = eventId;
               ocrMessage.body.payload.LaneID = laneId;
               ocrMessage.body.payload.CaseNumber = caseNumber;

               return ocrMessage;
            }
           catch
           {
               return null;
           }
        }

        #endregion

    }
}
