using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using L3.Cargo.Communications.Database;
using L3.Cargo.Communications.EventsLogger.Common;

namespace EventAndStatsLogger
{
    public class LogDatabase : DatabaseAccess
    {
        #region private members

        private const string _QuerySelectRowString = "SELECT datetime, type, computer, application, description, username FROM dbo.LogTable";

        private const string _dateTimeFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss.fff";

        #endregion


        #region Constructors

        public LogDatabase() :
            base()
        {
            string LogDBName = (string)ConfigurationManager.AppSettings["LogDBName"];
            Initialize(LogDBName, _QuerySelectRowString);
        }

        #endregion Constructors


        #region Private Methods

        private void UpdateInsertString (ref string queryString, ref string valueString, string column, string value)
        {
            if (!String.IsNullOrWhiteSpace(column) && !String.IsNullOrWhiteSpace(value))
            {
                queryString += (queryString.Length > 0) ? "," : string.Empty;
                queryString += column;

                valueString += (valueString.Length > 0) ? "," : string.Empty;
                valueString += "'" + value + "'";
            }
        }

        private void UpdateSelectString (ref string valueString, string column, string value)
        {
            if (!String.IsNullOrWhiteSpace(column) && !String.IsNullOrWhiteSpace(value))
            {
                valueString += (valueString.Length > 0) ? " AND " : string.Empty;
                valueString += column + " LIKE '%" + value + "%'";
            }
        }

        private void UpdateSelectString (ref string queryString, string column, DateTime? value, string compareString)
        {
            if (!String.IsNullOrWhiteSpace(column) && value != null)
            {
                queryString += (queryString.Length > 0) ? " AND " : string.Empty;
                queryString += column + compareString + "'" + value + "'";
            }
        }

        #endregion Private Methods


        #region Public Methods

        public void InsertEvent(Event eventToAdd)
        {
            string insertString = string.Empty;
            string valueString = string.Empty;

            UpdateInsertString(ref insertString, ref valueString, "datetime", ((DateTime)eventToAdd.DateAndTime).ToString(_dateTimeFormat));
            UpdateInsertString(ref insertString, ref valueString, "type", eventToAdd.Type);
            UpdateInsertString(ref insertString, ref valueString, "computer", eventToAdd.ComputerName);
            UpdateInsertString(ref insertString, ref valueString, "application", eventToAdd.Application);
            UpdateInsertString(ref insertString, ref valueString, "description", eventToAdd.Description.Replace("'", "''"));
            UpdateInsertString(ref insertString, ref valueString, "username", eventToAdd.UserName);
            UpdateInsertString(ref insertString, ref valueString, "object", eventToAdd.Object);
            UpdateInsertString(ref insertString, ref valueString, "lineNum", eventToAdd.Line.ToString());
            UpdateInsertString(ref insertString, ref valueString, "stackTrace", eventToAdd.StackTrace);

            base.InsertIntoDatabase("INSERT INTO LogTable (" + insertString + ") VALUES (" + valueString + ")");
        }

        public DataSet GetReport(ReportFilter filter)
        {
            string queryString = string.Empty;

            UpdateSelectString(ref queryString, "datetime", filter.DateAndTime, " >=  ");
            UpdateSelectString(ref queryString, "datetime", filter.ToDateTime, " <=  ");
            UpdateSelectString(ref queryString, "type", filter.Type);
            UpdateSelectString(ref queryString, "computer", filter.ComputerName);
            UpdateSelectString(ref queryString, "application", filter.Application);
            UpdateSelectString(ref queryString, "description", filter.Description);
            UpdateSelectString(ref queryString, "username", filter.UserName);

            if (queryString.Length > 0)
            {
                queryString = " WHERE (" + queryString;
                queryString += ")";
            }

            return base.QueryDatabase(_QuerySelectRowString + queryString);
        }

        #endregion Public Methods
    }
}