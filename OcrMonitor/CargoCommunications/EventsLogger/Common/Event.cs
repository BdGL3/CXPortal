using System;
using System.Runtime.Serialization;

namespace L3.Cargo.Communications.EventsLogger.Common
{
    [DataContract]
    public class Event
    {
        #region Private Members

        string _type;

        DateTime? _dateAndTime;

        string _application;

        string _computerName;

        string _userName;

        string _description;

        string _objectId;

        Int32? _line;

        string _stackTrace;

        #endregion Private Members


        #region Public Members

        [DataMember]
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        [DataMember]
        public DateTime? DateAndTime
        {
            get { return _dateAndTime; }
            set { _dateAndTime = value; }
        }

        [DataMember]
        public string Application
        {
            get { return _application; }
            set { _application = value; }
        }

        [DataMember]
        public string ComputerName
        {
            get { return _computerName; }
            set { _computerName = value; }
        }

        [DataMember]
        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        [DataMember]
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        [DataMember]
        public string Object
        {
            get { return _objectId; }
            set { _objectId = value; }
        }

        [DataMember]
        public Int32? Line
        {
            get { return _line; }
            set { _line = value; }
        }

        [DataMember]
        public string StackTrace
        {
            get { return _stackTrace; }
            set { _stackTrace = value; }
        }

        #endregion


        #region Constructors

        public Event (string type, DateTime? time, string computerName, string applicationName, string description)
        {
            _type = type;
            _dateAndTime = time;
            _application = applicationName;
            _computerName = computerName;
            _userName = null;
            _description = description;
            _objectId = null;
            _line = null;
            _stackTrace = null;
        }

        public Event (string type, DateTime? time, string computerName, string applicationName, string description, string user)
        {
            _type = type;
            _dateAndTime = time;
            _application = applicationName;
            _computerName = computerName;
            _userName = user;
            _description = description;
            _objectId = null;
            _line = null;
            _stackTrace = null;
        }

        public Event (string type, DateTime? time, string computerName, string applicationName, string description, string user, string objectId, int line)
        {
            _type = type;
            _dateAndTime = time;
            _application = applicationName;
            _computerName = computerName;
            _userName = user;
            _description = description;
            _objectId = objectId;
            _line = line;
            _stackTrace = null;
        }

        public Event (string type, DateTime? time, string computerName, string applicationName, string description, string user, string objectId, int line, string stackTrace)
        {
            _type = type;
            _dateAndTime = time;
            _application = applicationName;
            _computerName = computerName;
            _userName = user;
            _description = description;
            _objectId = objectId;
            _line = line;
            _stackTrace = stackTrace;
        }

        #endregion
    }
}
