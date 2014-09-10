using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Configuration;
using L3.Cargo.Communications.Common;
using L3.Cargo.Communications.Interfaces;
using System.Data;

namespace L3.Cargo.ArchiveCaseServer
{
    public class ContainerDatabase : Database
    {
        #region private members

        private string connectionString = null;

        private string QuerySelectRowString = "SELECT ULDNumber, FlightNumber, StatusMajor, StatusMinor, ImageExists, CheckInSequence, Batch FROM dbo.Container";
        #endregion

        #region Constructors

        public ContainerDatabase()
            : base()
        {
            try
            {
                string ContainerDBName = (String)ConfigurationManager.AppSettings["ContainerDBName"];
                string providerName = base.GetProviderNameByDBName(ContainerDBName);
                connectionString = base.GetConnectionStringByDBName(ContainerDBName);

                // Create the DbProviderFactory and DbConnection.
                base.factory = DbProviderFactories.GetFactory(providerName);

                base.connection = factory.CreateConnection();
                connection.ConnectionString = connectionString;

                // Create the DbCommand.
                DbCommand SelectTableCommand = factory.CreateCommand();
                SelectTableCommand.CommandText = QuerySelectRowString;
                SelectTableCommand.Connection = connection;

                adapter = factory.CreateDataAdapter();
                adapter.SelectCommand = SelectTableCommand;

                // Create the DbCommandBuilder.
                builder = factory.CreateCommandBuilder();
                builder.DataAdapter = adapter;
                // Get the insert, update and delete commands.
                adapter.InsertCommand = builder.GetInsertCommand();
                adapter.UpdateCommand = builder.GetUpdateCommand();
                adapter.DeleteCommand = builder.GetDeleteCommand();
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region override public members

        override public bool CaseExist(string caseid)
        {
            return true;
        }

        override public void update(CaseListDataSet.CaseListTableDataTable table)
        {
        }

        override public void delete(string caseid)
        {
        }

        #endregion

        #region public members

        public new DataRow query(string ULDNum)
        {
            try
            {
                string queryString = QuerySelectRowString + " WHERE ULDNumber = '" + ULDNum + "'";
                DataSet ds = queryDatabase(queryString);
                DataRow row;

                if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                    row = ds.Tables[0].Rows[0];
                else
                    row = null;

                return row;
            }
            catch
            {
                throw;
            }
        }

        public int GetSequenceNumber(DataRow row)
        {
            int number = 0;

            if (row["CheckInSequence"] != DBNull.Value)
                number = (short) row["CheckInSequence"];

            return number;
        }

        public int GetBatchNumber(DataRow row)
        {
            int number = 0;

            if (row["Batch"] != DBNull.Value)
                number = (int)row["Batch"];

            return number;
        }

        public int Update(DataRow row, int statusMajor, bool imageExist, int seqNum, int batchNum)
        {
            try
            {
                string updateString = "UPDATE dbo.Container SET StatusMajor= " + statusMajor + ", StatusMinor= 8, ImageExists= " + Convert.ToInt32(imageExist) +
                    ", CheckInSequence= " + seqNum + ", Batch= " + batchNum + " WHERE ULDNumber='" + row["ULDNumber"] + "' AND FlightNumber= '" + row["FlightNumber"] + "'";

                return base.UpdateDatabasePartialEntry(updateString);
            }
            catch
            {
                throw;
            }
        }

        public int UpdateStatusImageAndSeq(DataRow row, int StatusMajor, bool imageExist, int seqNum)
        {
            try
            {
                string updateString = "UPDATE dbo.Container SET StatusMajor= " + StatusMajor + ", StatusMinor= 8, ImageExists= " + Convert.ToInt32(imageExist) +
                    ", CheckInSequence= " + seqNum + " WHERE ULDNumber='" + row["ULDNumber"] + "' AND FlightNumber= '" + row["FlightNumber"] + "'";

                return base.UpdateDatabasePartialEntry(updateString);
            }
            catch
            {
                throw;
            }
        }

        public int UpdateStatusImageAndBatch(DataRow row, int StatusMajor, bool imageExist, int batchNum)
        {
            try
            {
                string updateString = "UPDATE dbo.Container SET StatusMajor= " + StatusMajor + ", StatusMinor= 8, ImageExists= " + Convert.ToInt32(imageExist) +
                    ", Batch= " + batchNum + " WHERE ULDNumber='" + row["ULDNumber"] + "' AND FlightNumber= '" + row["FlightNumber"] + "'";

                return base.UpdateDatabasePartialEntry(updateString);
            }
            catch
            {
                throw;
            }
        }

        public int Update(DataRow row, int StatusMajor, bool imageExist)
        {
            try
            {
                string updateString = "UPDATE dbo.Container SET StatusMajor= " + StatusMajor + ", StatusMinor= 8, ImageExists= " + Convert.ToInt32(imageExist) +
                    " WHERE ULDNumber='" + row["ULDNumber"] + "' AND FlightNumber= '" + row["FlightNumber"] + "'";

                return base.UpdateDatabasePartialEntry(updateString);
            }
            catch
            {
                throw;
            }
        }

        public void UpdateStatusMajor(DataRow row, int StatusMajor)
        {
            try
            {
                string updateString = "UPDATE dbo.Container SET StatusMajor= " + StatusMajor + " WHERE ULDNumber='" + row["ULDNumber"] + "' AND FlightNumber= '" + row["FlightNumber"] + "'";

                base.UpdateDatabasePartialEntry(updateString);
            }
            catch
            {
                throw;
            }
        }

        public void UpdateImageExists(DataRow row, bool imageExist)
        {
            try
            {
                string updateString = "UPDATE dbo.Container SET ImageExists= " + Convert.ToInt32(imageExist) + " WHERE ULDNumber='" + row["ULDNumber"] + "' AND FlightNumber= '" + row["FlightNumber"] + "'";

                base.UpdateDatabasePartialEntry(updateString);
            }
            catch
            {
                throw;
            }
        }

        public void UpdateSequenceNum(DataRow row, int SeqNum)
        {
            try
            {
                string updateString = "UPDATE dbo.Container SET CheckInSequence= " + SeqNum + " WHERE ULDNumber='" + row["ULDNumber"] + "' AND FlightNumber= '" + row["FlightNumber"] + "'";

                base.UpdateDatabasePartialEntry(updateString);
            }
            catch
            {
                throw;
            }
        }

        public void UpdateBatchNum(DataRow row, int batchNum)
        {
            try
            {
                string updateString = "UPDATE dbo.Container SET Batch= " + batchNum + " WHERE ULDNumber='" + row["ULDNumber"] + "' AND FlightNumber= '" + row["FlightNumber"] + "'";

                base.UpdateDatabasePartialEntry(updateString);
            }
            catch
            {
                throw;
            }
        }

        public void UpdateStatusMinor(DataRow row, int statusMinor)
        {
            try
            {
                string updateString = "UPDATE dbo.Container SET StatusMinor= " + statusMinor + " WHERE ULDNumber='" + row["ULDNumber"] + "' AND FlightNumber= '" + row["FlightNumber"] + "'";

                base.UpdateDatabasePartialEntry(updateString);
            }
            catch
            {
                throw;
            }
        }

        #endregion
    }
}

