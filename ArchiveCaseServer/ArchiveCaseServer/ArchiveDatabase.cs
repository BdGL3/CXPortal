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
    public class ArchiveDatabase : Database
    {
        #region private memembers

        private string connectionString = null;
        private string QuerySelectRowString = "SELECT CaseId, ObjectId, AnalystComment, FlightNumber, Analyst, CaseDirectory, ReferenceImage, Result, UpdateTime, Archived, AnalysisTime, Image, DFCMatch, CreateTime FROM dbo.ArchiveData";

        #endregion

        #region Constructors

        public ArchiveDatabase()
            : base()
        {
            try
            {
                string ArchiveDBName = (String)ConfigurationManager.AppSettings["ArchiveDBName"];
                string providerName = base.GetProviderNameByDBName(ArchiveDBName);
                connectionString = base.GetConnectionStringByDBName(ArchiveDBName);

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

                adapter.ContinueUpdateOnError = true;
                
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
            try
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();
               
                adapter.Update(table);

                if (table.HasErrors)
                {
                    CaseListDataSet.CaseListTableRow[] rows = (CaseListDataSet.CaseListTableRow[])table.GetErrors();
                    foreach (CaseListDataSet.CaseListTableRow row in rows)
                    {
                        if (row.RowState == DataRowState.Added && row.RowError.Contains("duplicate"))
                            UpdateCaseFields(row);
                        else
                            base.logger.PrintInfoLine(row.RowError);
                        
                    }
                }
            }
            catch (DbException ex)
            {
                //duplicate row being inserted error
                if (ex.ErrorCode == -2146232060)
                {                   
                    CaseListDataSet.CaseListTableRow[] rows = (CaseListDataSet.CaseListTableRow[])table.GetErrors();

                    foreach (CaseListDataSet.CaseListTableRow row in rows)
                    {
                        if(row.RowState == DataRowState.Added)
                            UpdateCaseFields(row);
                    }                    
                }
                else
                    throw;

            }
            catch
            {
                throw;
            }
        }

        override public void delete(string caseid)
        {
        }

        #endregion

        #region public members

        public void insert(string CaseId, string casefilepath, bool refimg, DateTime updateTime, int AnalysisTimeInSeconds,
            string user, string comment, int Result, string ObjectId, string FlightNumber, bool archived, byte[] thumbdata, bool DFCMatch)
        {
            try
            {
                string insertStringColumn = "INSERT INTO ArchiveData (";
                string insrtStringValue = "VALUES (";

                insertStringColumn += "CaseId";
                insrtStringValue += "@CaseId";

                List<DbParameter> parameterList = new List<DbParameter>();

                DbParameter parameter = factory.CreateParameter();
                parameter.Value = CaseId;
                parameter.ParameterName = "CaseId";
                parameterList.Add(parameter);

                insertStringColumn += ", CaseDirectory";
                insrtStringValue += ", @CaseDirectory";

                parameter = factory.CreateParameter();
                parameter.Value = casefilepath;
                parameter.ParameterName = "CaseDirectory";
                parameterList.Add(parameter);

                insertStringColumn += ", ReferenceImage";
                insrtStringValue += ", @ReferenceImage";

                parameter = factory.CreateParameter();
                parameter.ParameterName = "ReferenceImage";
                parameter.Value = refimg;
                parameterList.Add(parameter);

                if (updateTime != null)
                {
                    insertStringColumn += ", UpdateTime";
                    insrtStringValue += ", @UpdateTime";

                    parameter = factory.CreateParameter();
                    parameter.ParameterName = "UpdateTime";
                    parameter.Value = updateTime;
                    parameterList.Add(parameter);
                }

                if (AnalysisTimeInSeconds != 0)
                {
                    insertStringColumn += ", AnalysisTime";
                    insrtStringValue += ", @AnalysisTime";

                    parameter = factory.CreateParameter();
                    parameter.ParameterName = "AnalysisTime";
                    parameter.Value = AnalysisTimeInSeconds;
                    parameterList.Add(parameter);
                }

                insertStringColumn += ", Analyst";
                insrtStringValue += ", @Analyst";

                parameter = factory.CreateParameter();
                parameter.ParameterName = "Analyst";
                parameter.Value = user;
                parameterList.Add(parameter);

                insertStringColumn += ", AnalystComment";
                insrtStringValue += ", @AnalystComment";

                parameter = factory.CreateParameter();
                parameter.ParameterName = "AnalystComment";
                parameter.Value = comment;
                parameterList.Add(parameter);

                insertStringColumn += ", Result";
                insrtStringValue += ", @Result";

                parameter = factory.CreateParameter();
                parameter.ParameterName = "Result";
                parameter.Value = Result;
                parameterList.Add(parameter);

                insertStringColumn += ", ObjectID";
                insrtStringValue += ", @ObjectID";

                parameter = factory.CreateParameter();
                parameter.ParameterName = "ObjectID";
                parameter.Value = ObjectId;
                parameterList.Add(parameter);

                insertStringColumn += ", FlightNumber";
                insrtStringValue += ", @FlightNumber";

                parameter = factory.CreateParameter();
                parameter.ParameterName = "FlightNumber";
                parameter.Value = FlightNumber;
                parameterList.Add(parameter);

                insertStringColumn += ", Archived";
                insrtStringValue += ", @Archived";

                parameter = factory.CreateParameter();
                parameter.ParameterName = "Archived";
                parameter.Value = archived;
                parameterList.Add(parameter);

                if (thumbdata != null)
                {
                    insertStringColumn += ", Image";
                    insrtStringValue += ", @Image";

                    parameter = factory.CreateParameter();
                    parameter.ParameterName = "Image";
                    parameter.Value = thumbdata;
                    parameterList.Add(parameter);
                }

                insertStringColumn += ", DFCMatch";
                insrtStringValue += ", @DFCMatch";

                parameter = factory.CreateParameter();
                parameter.ParameterName = "DFCMatch";
                parameter.Value = DFCMatch;
                parameterList.Add(parameter);

                insertStringColumn += ")";
                insrtStringValue += ")";

                base.insert(insertStringColumn + insrtStringValue, parameterList.ToArray());
            }
            catch
            {
                throw;
            }
        }

        override public CaseListDataSet query(string caseid)
        {
            try
            {
                string queryString = QuerySelectRowString + " WHERE CaseId = '" + caseid + "'";

                int rows;
                CaseListDataSet ds = new CaseListDataSet();

                // Create the DbCommand.

                DbCommand SelectTableCommand = factory.CreateCommand();
                SelectTableCommand.CommandText = queryString;
                SelectTableCommand.Connection = connection;

                //adapter = factory.CreateDataAdapter();
                adapter.SelectCommand = SelectTableCommand;

                rows = adapter.Fill(ds.CaseListTable);

                foreach (CaseListDataSet.CaseListTableRow row in ds.CaseListTable)
                {
                    try
                    {
                        row[ds.CaseListTable.ResultColumn] = Enum.GetName(typeof(WorkstationDecision), Convert.ToInt32(row[ds.CaseListTable.ResultColumn]));
                    }
                    catch (Exception)
                    {
                        row[ds.CaseListTable.ResultColumn] = WorkstationDecision.Unknown;
                    }
                }

                return ds;
            }
            catch
            {
                throw;
            }
        }

        public CaseListDataSet getAllEntries(UInt16 timeSpan)
        {
            string queryString;

            if (timeSpan > 0)
            {
                DateTime datetime = DateTime.Now.AddDays(-timeSpan);
                queryString = QuerySelectRowString + " WHERE UpdateTime >= '" + datetime + "' ORDER BY UpdateTime DESC";
            }
            else
                queryString = QuerySelectRowString + " ORDER BY UpdateTime DESC";

            try
            {
                int rows;
                CaseListDataSet ds = new CaseListDataSet();

                // Create the DbCommand.

                DbCommand SelectTableCommand = factory.CreateCommand();
                SelectTableCommand.CommandText = queryString;
                SelectTableCommand.Connection = connection;

                //adapter = factory.CreateDataAdapter();
                adapter.SelectCommand = SelectTableCommand;

                rows = adapter.Fill(ds.CaseListTable);

                //foreach (CaseListDataSet.CaseListTableRow row in ds.CaseListTable)
                //{
                //    try
                //    {
                //        row[ds.CaseListTable.ResultColumn] = Enum.GetName(typeof(WorkstationDecision), Convert.ToInt32(row[ds.CaseListTable.ResultColumn]));
                //    }
                //    catch (Exception)
                //    {
                //        row[ds.CaseListTable.ResultColumn] = WorkstationDecision.Unknown;
                //    }
                //}

                return ds;
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        public void UpdateCaseArchivedField(string caseid, bool archived)
        {
            try
            {
                string updateString = "UPDATE ArchiveData SET Archived= " + Convert.ToInt32(archived) + " WHERE CaseId='" + caseid + "' AND ReferenceImage= " + Convert.ToInt32(false);

                base.UpdateDatabasePartialEntry(updateString);
            }
            catch
            {
                throw;
            }
        }

        public void UpdateCaseFields(CaseListDataSet.CaseListTableRow row)
        {
            try
            {
                string ImageStr = "0x" + BitConverter.ToString(row.Image).Replace("-", string.Empty);
                
                string updateString = "UPDATE ArchiveData SET " +
                    "[CaseId]= '" + row.CaseId + "'" +
                    ", [ReferenceImage]= " + Convert.ToInt32(row.ReferenceImage) +
                    ", [ObjectId]= '" + row.ObjectId + "'" +
                    ", [AnalystComment]= '" + row.AnalystComment + "'" +
                    ", [FlightNumber]= '" + row.FlightNumber + " '" +
                    ", [Analyst]= '" + row.Analyst + "'" +
                    ", [CaseDirectory]= '" + row.CaseDirectory + "'" +
                    ", [Result]= " + row.Result +
                    ", [UpdateTime]= '" + row.UpdateTime.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss'.'fff") + "'" +
                    ", [Archived]= " + Convert.ToInt32(row.Archived) + 
                    ", [AnalysisTime]= " + row.AnalysisTime +                   
                    ", [DFCMatch]= " + Convert.ToInt32(row.DFCMatch) +
                    ", [CreateTime]= '" + row.CreateTime.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss'.'fff") + "'" +
                    ", [Image] = " + ImageStr +
                    " WHERE CaseId='" + row.CaseId + "' AND ReferenceImage= " + Convert.ToInt32(row.ReferenceImage);

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
