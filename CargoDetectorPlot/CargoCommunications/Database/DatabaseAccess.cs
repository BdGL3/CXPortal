using System;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace L3.Cargo.Communications.Database
{
    public class DatabaseAccess
    {
        #region Protected Members

        protected DbProviderFactory factory;

        protected DbConnection connection;

        protected DbDataAdapter adapter;

        protected DbCommandBuilder builder;

        #endregion


        #region Constructors

        public DatabaseAccess()
        {
        }

        #endregion Constructors


        #region Protected Methods

        protected void Initialize (string databaseName, string querySelectRowString)
        {
            string providerName = GetProviderNameByDBName(databaseName);
            string connectionString = GetConnectionStringByDBName(databaseName);

            // Create the DbProviderFactory and DbConnection.
            factory = DbProviderFactories.GetFactory(providerName);

            connection = factory.CreateConnection();
            connection.ConnectionString = connectionString;

            // Create the DbCommand.
            DbCommand SelectTableCommand = factory.CreateCommand();
            SelectTableCommand.CommandText = querySelectRowString;
            SelectTableCommand.Connection = connection;

            adapter = factory.CreateDataAdapter();
            adapter.SelectCommand = SelectTableCommand;

            // Create the DbCommandBuilder.
            builder = factory.CreateCommandBuilder();
            builder.DataAdapter = adapter;

            adapter.ContinueUpdateOnError = true;
        }

        virtual protected void Insert(string iString, DbParameter[] parameters)
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

        virtual protected void Delete(string caseid)
        {
        }

        virtual protected void UpdateDatabasePartialEntry(string ustring)
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
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        virtual protected void InsertIntoDatabase(string iString)
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
                throw exp;
            }
        }

        virtual protected void UpdateDatabase(string uString)
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
                throw exp;
            }
        }

        virtual protected DataSet QueryDatabase(string qstring)
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
                throw exp;
            }
        }

        // Retrieve a connection string by specifying the providerName.
        // Assumes one connection string per provider in the config file.
        protected string GetConnectionStringByDBName(string DBName)
        {
            // Return null on failure.
            string ret = null;

            ConnectionStringSettings connectionSetting = ConfigurationManager.ConnectionStrings[DBName];
            if (connectionSetting != null)
            {
                ret = connectionSetting.ConnectionString;
            }

            return ret;
        }

        // Retrieve a connection string by specifying the providerName.
        // Assumes one connection string per provider in the config file.
        protected string GetProviderNameByDBName(string DBName)
        {
            // Return null on failure.
            string ret = null;

            ConnectionStringSettings connectionSetting = ConfigurationManager.ConnectionStrings[DBName];
            if (connectionSetting != null)
            {
                ret = connectionSetting.ProviderName;
            }

            return ret;
        }

        #endregion Protected Methods
    }
}
