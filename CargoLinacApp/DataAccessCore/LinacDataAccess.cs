using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using L3.Cargo.Subsystem.DataAccessCore;
using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Communications.Linac;
using System.Configuration;
using L3.Cargo.Communications.Common;
using System.Threading;

namespace L3.Cargo.Linac.DataAccessCore
{
    public class LinacDataAccess : DataAccess
    {
        #region Private Members

        private LinacAccess _LinacAccess;

        private Thread _StatesThread;

        #endregion Private Members


        #region Public Members

        public event ConnectionStateChangeHandler LinacConnectionStateChangeEvent;

        public LinacAccess Linac
        {
            get { return _LinacAccess; }
        }

        #endregion Public Members


        #region Constructors

        public LinacDataAccess(EventLoggerAccess logger) : 
            base(logger)
        {
            string ipAddress = ConfigurationManager.AppSettings["LinacIP"];
            int port = Convert.ToInt32(ConfigurationManager.AppSettings["LinacPort"]);
            int time = Convert.ToInt32(ConfigurationManager.AppSettings["LinacPingTime"]);
            _LinacAccess = new LinacAccess(logger, ipAddress, port, time);
            _LinacAccess.ConnectionStateChangeEvent += new ConnectionStateChangeHandler(LinacAccess_ConnectionStateChangeEvent);
        }

        #endregion Constructors


        #region Private Methods

        private void LinacAccess_ConnectionStateChangeEvent(bool isConnected)
        {
            if (LinacConnectionStateChangeEvent != null)
            {
                LinacConnectionStateChangeEvent(isConnected);
            }
        }

        private void ProcessLinacStates()
        {
            Dictionary<LinacPacketFormat.VariableIdEnum, object> parameterList = new Dictionary<LinacPacketFormat.VariableIdEnum, object>();
            _LinacAccess.GetOperatingParameters(ref parameterList);
        }

        #endregion Private Methods


        #region Public Methods

        public void Open()
        {
            _LinacAccess.Open();
            if (_StatesThread == null)
            {
                _StatesThread = new Thread(new ThreadStart(ProcessLinacStates));
                _StatesThread.IsBackground = true;
                _StatesThread.Start();
            }
        }

        public void Close()
        {
            _LinacAccess.Close();
            if (_StatesThread != null)
            {
                _StatesThread.Join();
                _StatesThread = null;
            }
        }

        public override void Dispose()
        {
            _LinacAccess.Dispose();
            this.Close();
            base.Dispose();
        }

        #endregion Public Methods
    }
}
