using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel;
using L3.Cargo.Common.Configurations;
using L3.Cargo.Communications.Dashboard.Display.Client;
using L3.Cargo.Communications.EventsLogger.Client;
using L3.Cargo.Communications.OPC;
using L3.Cargo.Communications.Common;
using L3.Cargo.Subsystem.DataAccessCore;

namespace L3.Cargo.Subsystem.DataAccessCore
{
    public class DataAccess : DataAccessBase, IDisposable
    {
        #region Private Members

        private OpcClient _OpcClient;

        EventLoggerAccess _logger;

        #endregion Private Members


        #region Public Members

        public event PLCTagUpdateHandler TagUpdate;

        public OpcSection OpcSection
        {
            get
            {
                return _OpcClient.OPCSection;
            }
        }

        #endregion Public Members


        #region Constructors

        public DataAccess (EventLoggerAccess logger) : 
            base(logger)
        {
            _logger = logger;
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            _OpcClient = new OpcClient(configuration.GetSection("opcSection") as OpcSection, ConfigurationManager.AppSettings["TagGroup"], _logger);
            _OpcClient.OpcTagUpdate += new OpcTagUpdateHandler(_OpcClient_OpcTagUpdate);
            _OpcClient.Open();

        }

        #endregion Constructors


        #region Private Methods

        private void _OpcClient_OpcTagUpdate (string name, int value)
        {
            if (TagUpdate != null)
            {
                TagUpdate(name, value);
            }
        }

        #endregion Private Methods


        #region Public Methods

        public int GetTagValue (string name)
        {
            int value = -1;
            try
            {
                value = _OpcClient.ReadValue(name);
            }
            catch (Exception exp)
            {
                _logger.LogError(exp);
            }

            return value;
        }

        public void UpdatePLCTagValue (string name, int value)
        {
            try
            {
                _OpcClient.WriteShort(name, Convert.ToInt16(value));
            }
            catch (Exception exp)
            {
                _logger.LogError(exp);
            }
        }

        public virtual void Dispose()
        {
            _OpcClient.Close();
			_OpcClient.Dispose();
        }

        #endregion Public Methods
    }
}
