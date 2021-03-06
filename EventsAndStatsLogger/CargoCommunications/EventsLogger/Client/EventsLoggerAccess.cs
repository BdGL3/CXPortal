﻿using System;
using System.Diagnostics;
using System.Transactions;
using L3.Cargo.Communications.EventsLogger.Common;

namespace L3.Cargo.Communications.EventsLogger.Client
{
    public delegate void LogMessageUpdateHandler(DateTime dateTime, string message);

    public class EventLoggerAccess
    {
        #region Public Members

        public event LogMessageUpdateHandler LogMessageUpdate;

        #endregion Public Members


        #region Constructors

        public EventLoggerAccess()
        {
        }

        #endregion


        #region Private Methods

        private void Log (Event e)
        {
            try
            {
                if (LogMessageUpdate != null)
                {
                    string /*description*/ dsc = "no description";
                    if (!string.IsNullOrWhiteSpace(e.Description))
                        dsc = e.Description.Trim();
                    string /*stack trace*/ trc = string.Empty;
                    if (!string.IsNullOrWhiteSpace(e.StackTrace))
                        trc = " " + e.StackTrace.Trim();
                    string /*type*/ typ = "unknown type";
                    if (!string.IsNullOrWhiteSpace(e.Type))
                        typ = e.Type.Trim();
                    LogMessageUpdate((DateTime)e.DateAndTime, ": " + typ + "; " + dsc + trc);
                }

                using (EventsLoggerEndpoint endpoint = new EventsLoggerEndpoint())
                {
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required))
                    {
                        endpoint.LogEvent(e);
                        scope.Complete();
                    }
                }
            }
            catch { /*suppress exceptions*/ }
        }

        private void Log (string type, string message)
        {
            Event tmpEvent = new Event(type, DateTime.Now, System.Environment.MachineName,
                                       Process.GetCurrentProcess().ProcessName, message);
            Log(tmpEvent);
        }

        private void Log (string type, string message, string objectId, Int32 line)
        {
            try
            {
                Event tmpEvent = new Event(type, DateTime.Now, System.Environment.MachineName,
                                           Process.GetCurrentProcess().ProcessName, message, null, objectId, line);
                Log(tmpEvent);
            }
            catch { }
        }
        #endregion Private Methods

        #region Public Methods

        public void LogWarning(string message)
        {
            StackTrace trace = new StackTrace(true);
            Log("Warning", message, trace.GetFrame(1).GetMethod().ReflectedType.Name
                + "." + trace.GetFrame(1).GetMethod().Name, trace.GetFrame(1).GetFileLineNumber());
        }

        public void LogInfo (string message)
        {
            StackTrace trace = new StackTrace(true);
            Log("Info", message, trace.GetFrame(1).GetMethod().ReflectedType.Name
                + "." + trace.GetFrame(1).GetMethod().Name, trace.GetFrame(1).GetFileLineNumber());
        }

        public void LogError (string message)
        {
            StackTrace trace = new StackTrace(true);
            Log("Error", message, trace.GetFrame(1).GetMethod().ReflectedType.Name
                + "." + trace.GetFrame(1).GetMethod().Name, trace.GetFrame(1).GetFileLineNumber());
        }

        public void LogError(Exception exp)
        {
            LogError(exp.Message, new StackTrace(exp, true));
        }

        public void LogError (string message, StackTrace stackTrace)
        {
            Event tmpEvent = new Event("Error",
                                       DateTime.Now,
                                       System.Environment.MachineName,
                                       Process.GetCurrentProcess().ProcessName,
                                       message,
                                       null,
                                       stackTrace.GetFrame(0).GetMethod().ReflectedType.Name + "." + stackTrace.GetFrame(0).GetMethod().Name,
                                       stackTrace.GetFrame(0).GetFileLineNumber(),
                                       stackTrace.ToString());
            Log(tmpEvent);
        }

        #endregion Public Methods
    }
}
