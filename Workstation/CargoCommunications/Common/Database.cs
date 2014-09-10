using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Data.Common;
using L3.Cargo.Communications.Common;
using System.Data;

namespace L3.Cargo.Communications.Common
{
    public class Database
    {
        #region Protected members

        protected DbProviderFactory factory;
        protected DbConnection connection;
        protected DbDataAdapter adapter;
        protected DbCommandBuilder builder;

        #endregion

        public Logger logger;

        #region Constructors

        public Database()
        {
        }

        #endregion

        #region Public members

        virtual public bool CaseExist(string caseid)
        {
            return true;
        }

        virtual public void insert(string iString, DbParameter[] parameters)
        {
            try
            {
                DbCommand insertCommand = factory.CreateCommand();
                insertCommand.Connection = connection;
                insertCommand.Parameters.AddRange(parameters);

                connection.Close();
                connection.Open();
                insertCommand.CommandText = iString;
                int numRowsAffected = insertCommand.ExecuteNonQuery();
                connection.Close();
            }
            catch
            {
                throw;
            }
        }

        virtual public void update(CaseListDataSet.CaseListTableDataTable table)
        {
        }

        virtual public void delete(string caseid)
        {
        }

        virtual public CaseListDataSet query(string caseid)
        {
            return null;
        }

        virtual public int UpdateDatabasePartialEntry(string ustring)
        {
            try
            {
                int rows;

                // Create the DbCommand.
                DbCommand UpdateTableEntryCommand = factory.CreateCommand();
                UpdateTableEntryCommand.CommandText = ustring;
                UpdateTableEntryCommand.Connection = connection;

                connection.Close();
                connection.Open();
                rows = UpdateTableEntryCommand.ExecuteNonQuery();
                connection.Close();

                return rows;
            }
            catch (Exception exp)
            {
                throw;
            }
        }

        virtual public void insertIntoDatabase(string iString)
        {
            try
            {
                int rows;

                // Create the DbCommand.
                DbCommand InsertIntoCommand = factory.CreateCommand();
                InsertIntoCommand.CommandText = iString;
                InsertIntoCommand.Connection = connection;
                connection.Close();
                connection.Open();
                rows = InsertIntoCommand.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception exp)
            {
                throw;
            }
        }

        virtual public void UpdateDatabase(string uString)
        {
            try
            {
                int rows;

                // Create the DbCommand.
                DbCommand updateCommand = factory.CreateCommand();
                updateCommand.CommandText = uString;
                updateCommand.Connection = connection;
                connection.Open();
                rows = updateCommand.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception exp)
            {
                throw;
            }
        }

        virtual public DataSet queryDatabase(string qstring)
        {
            try
            {
                int rows;
                DataSet ds = new DataSet();

                // Create the DbCommand.

                DbCommand SelectTableCommand = factory.CreateCommand();
                SelectTableCommand.CommandText = qstring;
                SelectTableCommand.Connection = connection;

                adapter = factory.CreateDataAdapter();
                adapter.SelectCommand = SelectTableCommand;

                rows = adapter.Fill(ds);

                return ds;
            }
            catch (Exception exp)
            {
                throw;
            }
        }

        // Retrieve a connection string by specifying the providerName.
        // Assumes one connection string per provider in the config file.
        public string GetConnectionStringByProvider(string providerName)
        {
            // Return null on failure.
            string returnValue = null;

            var map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, ConfigurationManager.AppSettings["ConnectionsString"]);

            var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

            ConnectionStringsSection ConnectionStringsSection = config.ConnectionStrings;

            ConnectionStringSettingsCollection settings = ConnectionStringsSection.ConnectionStrings;

            // Walk through the collection and return the first 
            // connection string matching the providerName.
            if (settings != null)
            {
                foreach (ConnectionStringSettings cs in settings)
                {
                    if (cs.ProviderName == providerName)
                    {
                        returnValue = cs.ConnectionString;
                        break;
                    }
                }
            }
            return returnValue;
        }

        // Retrieve a connection string by specifying the providerName.
        // Assumes one connection string per provider in the config file.
        public string GetConnectionStringByDBName(string DBName)
        {
            // Return null on failure.
            string returnValue = null;

            var map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, ConfigurationManager.AppSettings["ConnectionsString"]);

            var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

            ConnectionStringsSection ConnectionStringsSection = config.ConnectionStrings;

            ConnectionStringSettingsCollection settings = ConnectionStringsSection.ConnectionStrings;

            // Walk through the collection and return the first 
            // connection string matching the providerName.
            if (settings != null)
            {
                foreach (ConnectionStringSettings cs in settings)
                {
                    if (cs.Name == DBName)
                    {
                        returnValue = cs.ConnectionString;
                        break;
                    }
                }
            }
            return returnValue;
        }

        // Retrieve a connection string by specifying the providerName.
        // Assumes one connection string per provider in the config file.
        public string GetProviderNameByDBName(string DBName)
        {
            // Return null on failure.
            string returnValue = null;

            var map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, ConfigurationManager.AppSettings["ConnectionsString"]);

            var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

            ConnectionStringsSection ConnectionStringsSection = config.ConnectionStrings;

            ConnectionStringSettingsCollection settings = ConnectionStringsSection.ConnectionStrings;

            // Walk through the collection and return the first 
            // connection string matching the providerName.
            if (settings != null)
            {
                foreach (ConnectionStringSettings cs in settings)
                {
                    if (cs.Name == DBName)
                    {
                        returnValue = cs.ProviderName;
                        break;
                    }
                }
            }
            return returnValue;
        }

        #endregion
    }
}
