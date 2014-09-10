using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.SqlServerCe;
using System.Windows;
using L3.Cargo.Communications.Common;
using System.Data;
using System.Data.Common;
using System.Configuration;

namespace L3.Cargo.Communications.Common
{
    public class LocalDatabase : Database, IDisposable
    {
        #region Private Memembers

        private string connectionString;        
        private string QuerySelectRowString = "SELECT * FROM LocalArchiveData";

        private SqlCeDataAdapter m_adapter;
        private SqlCeCommandBuilder m_builder;
        private SqlCeConnection m_connection;         

        #endregion

        public LocalDatabase(string FilePath)
            : base()
        {
            try
            {
                string fileName = Path.Combine(FilePath, "caselist.sdf");               

                connectionString = string.Format("Data Source=\"{0}\"", fileName);

                SqlCeEngine en = new SqlCeEngine(connectionString);

                if (!File.Exists(fileName))
                {                    
                    en.CreateDatabase();
                }

                m_connection = new SqlCeConnection();
                m_connection.ConnectionString = connectionString;

                if (m_connection.State == System.Data.ConnectionState.Closed)
                {
                    try
                    {
                        m_connection.Open();
                    }
                    catch (SqlCeInvalidDatabaseFormatException)
                    {
                        en.Upgrade();
                        m_connection.Open();
                    }                  
                }

                SqlCeCommand SelectTableCommand = new SqlCeCommand();
                SelectTableCommand.CommandText = QuerySelectRowString;
                SelectTableCommand.Connection = m_connection;

                m_adapter = new SqlCeDataAdapter((SqlCeCommand)SelectTableCommand);

                // Create the DbCommandBuilder.
                m_builder = new SqlCeCommandBuilder();
                m_builder.DataAdapter = m_adapter;

                m_adapter.SelectCommand = SelectTableCommand;                
            }
            catch
            {
                throw;
            }
        }

        public void CreateTable(CaseListDataSet.CaseListTableDataTable table)
        {
            SqlCeCommand cmd;

            //if table already exist then take not action
            string tableAlreadyExist = "Select * from information_schema.tables where table_name = 'LocalArchiveData'";

            cmd = new SqlCeCommand(tableAlreadyExist, m_connection);
            object result;

            try
            {
                result =cmd.ExecuteScalar();
            }
            catch (SqlCeException sqlexception)
            {
                throw sqlexception;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (result == null)
            {

                string sql = "create table LocalArchiveData (";

                foreach (DataColumn column in table.Columns)
                {
                    string datatype = "nvarchar (256)";
                    String allowNull = string.Empty;

                    if (column.ColumnName == "Image")
                        datatype = "image";

                    switch (column.ColumnName)
                    {
                        case "Image":
                            datatype = "image";
                            break;
                        case "CaseId":
                            datatype = "nvarchar (24)";
                            allowNull = "not null";
                            break;
                        case "Result":                        
                            datatype = "nvarchar (24)";
                            break;
                        case "AnalystComment":                        
                            datatype = "nvarchar (256)";
                            break;
                        case "ObjectId":
                        case "FlightNumber":
                        case "CreateTime":                       
                            datatype = "nvarchar (50)";
                            break;                        
                        case "Analyst":
                            datatype = "nvarchar (64)";
                            break;
                        case "CaseDirectory":
                            datatype = "nvarchar (96)";
                            break;
                        case "ReferenceImage":
                            datatype = "bit";
                            allowNull = "not null";
                            break;
                        case "Archived":
                        case "DFCMatch":
                            datatype = "bit";
                            break;                        
                        case "UpdateTime":
                            datatype = "datetime";
                            break;                         
                        case "AnalysisTime":
                            datatype = "int";
                            break; 
                    }
                    
                    if(column.ColumnName != "CTI" && column.ColumnName != "Area" && column.ColumnName != "AssignedId")
                        sql += column.ColumnName + " " + datatype + " " + allowNull + ", ";

                }
                sql += "Constraint PK Primary Key (";

                foreach (DataColumn column in table.PrimaryKey)
                {
                    sql += column.ColumnName + ", ";
                }

                sql = sql.Substring(0, sql.LastIndexOf(","));
                sql += ") )";

                cmd = new SqlCeCommand(sql, m_connection);

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SqlCeException sqlexception)
                {
                    throw sqlexception;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            try
            {
                string cmdText = "INSERT INTO LocalArchiveData (CaseId, AnalystComment, ObjectId, FlightNumber, Analyst, CaseDirectory, ReferenceImage, Result, UpdateTime, CreateTime, Image, DFCMatch) " +
                    "VALUES (@CaseId, @AnalystComment, @ObjectId, @FlightNumber, @Analyst, @CaseDirectory, @ReferenceImage, @Result, @UpdateTime, @CreateTime, @Image, @DFCMatch)";

                SqlCeCommand command = new SqlCeCommand(cmdText, m_connection);
                command.Parameters.Add("@CaseId", SqlDbType.NVarChar, 24, "CaseId");
                command.Parameters.Add("@AnalystComment", SqlDbType.NVarChar, 256, "AnalystComment");
                command.Parameters.Add("@ObjectId", SqlDbType.NVarChar, 50, "ObjectId");
                command.Parameters.Add("@FlightNumber", SqlDbType.NVarChar, 50, "FlightNumber");
                command.Parameters.Add("@Analyst", SqlDbType.NVarChar, 64, "Analyst");
                command.Parameters.Add("@CaseDirectory", SqlDbType.NVarChar, 96, "CaseDirectory");
                command.Parameters.Add("@ReferenceImage", SqlDbType.Bit, 1, "ReferenceImage");
                command.Parameters.Add("@Result", SqlDbType.NVarChar, 24, "Result");
                command.Parameters.Add("@UpdateTime", SqlDbType.DateTime, 50, "UpdateTime");
                command.Parameters.Add("@CreateTime", SqlDbType.NVarChar, 50, "CreateTime");
                command.Parameters.Add("@Image", SqlDbType.Image, 20000, "Image");
                command.Parameters.Add("@DFCMatch", SqlDbType.Bit, 1, "DFCMatch");               

                m_adapter.InsertCommand = command;

                cmdText = "UPDATE LocalArchiveData SET CaseId = @CaseId, AnalystComment = @AnalystComment, ObjectId = @ObjectId," +
                    "FlightNumber = @FlightNumber, Analyst = @Analyst, CaseDirectory = @CaseDirectory, ReferenceImage = @ReferenceImage," +
                    "Result = @Result, UpdateTime = @UpdateTime, CreateTime = @CreateTime, Image = @Image, DFCMatch = @DFCMatch " +
                    "WHERE CaseId = @oldCaseId AND ReferenceImage = @oldReferenceImage";

                command = new SqlCeCommand(cmdText, m_connection);
                command.Parameters.Add("@CaseId", SqlDbType.NVarChar, 24, "CaseId");
                command.Parameters.Add("@AnalystComment", SqlDbType.NVarChar, 256, "AnalystComment");
                command.Parameters.Add("@ObjectId", SqlDbType.NVarChar, 50, "ObjectId");
                command.Parameters.Add("@FlightNumber", SqlDbType.NVarChar, 50, "FlightNumber");
                command.Parameters.Add("@Analyst", SqlDbType.NVarChar, 64, "Analyst");
                command.Parameters.Add("@CaseDirectory", SqlDbType.NVarChar, 96, "CaseDirectory");
                command.Parameters.Add("@ReferenceImage", SqlDbType.Bit, 1, "ReferenceImage");
                command.Parameters.Add("@Result", SqlDbType.NVarChar, 24, "Result");
                command.Parameters.Add("@UpdateTime", SqlDbType.DateTime, 50, "UpdateTime");
                command.Parameters.Add("@CreateTime", SqlDbType.NVarChar, 50, "CreateTime");                
                command.Parameters.Add("@Image", SqlDbType.Image, 20000, "Image");
                command.Parameters.Add("@DFCMatch", SqlDbType.Bit, 1, "DFCMatch");                
                
                SqlCeParameter parameter = command.Parameters.Add("@oldCaseId", SqlDbType.NVarChar, 24, "CaseId");
                parameter.SourceVersion = DataRowVersion.Original;

                parameter = command.Parameters.Add("@oldReferenceImage", SqlDbType.Bit, 1, "ReferenceImage");
                parameter.SourceVersion = DataRowVersion.Original;
                
                m_adapter.UpdateCommand = command;

                cmdText = "DELETE FROM LocalArchiveData WHERE CaseId = @oldCaseId AND ReferenceImage = @oldReferenceImage";
                command = new SqlCeCommand(cmdText, m_connection);
                parameter = parameter = command.Parameters.Add("@oldCaseId", SqlDbType.NVarChar, 24, "CaseId");
                parameter.SourceVersion = DataRowVersion.Original;

                parameter = command.Parameters.Add("@oldReferenceImage", SqlDbType.Bit, 1, "ReferenceImage");
                parameter.SourceVersion = DataRowVersion.Original;

                m_adapter.DeleteCommand = command;
                m_adapter.MissingSchemaAction = MissingSchemaAction.Ignore;
            }
            catch
            {
                throw;
            }
        }

        override public void update(CaseListDataSet.CaseListTableDataTable table)
        {
            try
            {
                if (m_connection.State == ConnectionState.Closed)
                    m_connection.Open();

                SqlCeTransaction se = m_connection.BeginTransaction(IsolationLevel.Serializable);
                try
                {
                    m_adapter.Update(table);
                }
                catch (Exception ex)
                {
                    m_connection.Close();
                    m_connection.Open();

                    se = m_connection.BeginTransaction(IsolationLevel.Serializable);
                    m_adapter.Update(table);
                }

                se.Commit(CommitMode.Immediate);
            }
            catch (SqlCeException ex)
            {
                if (!ex.Message.Contains("duplicate"))
                    throw;
            }
            catch
            {
                throw;
            }
        }

        public int LoadDataSet(CaseListDataSet csDataSet)
        {

            int rowAffected = 0;

            try
            {                
                rowAffected = m_adapter.Fill(csDataSet.CaseListTable);
            }
            catch (SqlCeException sqlexception)
            {
                throw sqlexception;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return rowAffected;

        }

        public void Dispose()
        {
            m_connection.Close();
        }
    }
}
