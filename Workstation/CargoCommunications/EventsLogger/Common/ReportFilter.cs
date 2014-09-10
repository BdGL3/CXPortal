using System;
using System.Runtime.Serialization;

namespace L3.Cargo.Communications.EventsLogger.Common
{
    [DataContract]
    public class ReportFilter : Event
    {
        #region Private Members

        private DateTime? _ToDateTime;

        #endregion Private Members


        #region Public Members

        public DateTime? ToDateTime
        {
            get
            {
                return _ToDateTime;
            }
            set
            {
                _ToDateTime = value;
            }
        }

        #endregion Public Members


        #region Constructors

        public ReportFilter (string type, DateTime? FromDateTime, DateTime? ToDateTime, string computerName,
                             string applicationName, string description, string user) :
            base(type, FromDateTime, computerName, applicationName, description, user)
        {
            _ToDateTime = ToDateTime;
        }

        #endregion Constructors
    }
}
