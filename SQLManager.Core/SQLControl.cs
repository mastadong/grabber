using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;

namespace SQLManager.Core
{
    public class SQLControl
    {
        private SqlConnection DBConn { get; set; }
        private SqlCommand DBComm { get; set; }
        public SqlDataAdapter DBDA { get; set; }
        public DataTable DBDT { get; set; }

        public List<SqlParameter> Parameters { get; set; }
        public List<SqlParameter> OutputParameters { get; set; }
        public int RecordCount { get; private set; }
        public string Exception { get; private set; } = "";

        /// <summary>
        /// Constructor requires a valid connection string.
        /// </summary>
        /// <param name="connectionString"></param>
        public SQLControl(string connectionString)
        {
            DBConn = new SqlConnection(connectionString);
            Parameters = new List<SqlParameter>();
            OutputParameters = new List<SqlParameter>();
        }

        /// <summary>
        /// Add parameters which will be used when the command is executed.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="val"></param>
        public void AddParam(string name, object val)
        {
            SqlParameter newParameter = new SqlParameter(name, val);
            Parameters.Add(newParameter);
        }

        /// <summary>
        /// Specify output parameters which will receive data from the executed command. 
        /// </summary>
        /// <param name="outputParameter"></param>
        /// <param name="parameterType"></param>
        /// <param name="size"></param>
        public void AddOutputParam(string outputParameter, SqlDbType parameterType, int size)
        {
            SqlParameter newParameter = new SqlParameter(outputParameter, parameterType, size);
            OutputParameters.Add(newParameter);
        }

        /// <summary>
        /// Returns true if an internal exception is encountered; returns false otherwise.
        /// </summary>
        /// <returns></returns>
        public bool HasException()
        {
            if(string.IsNullOrEmpty(Exception))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Executes the provided query string against the database.
        /// </summary>
        /// <param name="query"></param>
        public void ExecQuery(string query)
        {
            RecordCount = 0;
            Exception = "";

            try
            {
                DBConn.Open();
                DBComm = new SqlCommand(query, DBConn);

                //Load the parameters into the command object.
                foreach(SqlParameter p in Parameters)
                {
                    DBComm.Parameters.Add(p);
                }
                //Clear the parameters list.
                Parameters.Clear();

                DBDT = new DataTable();
                DBDA = new SqlDataAdapter(DBComm);
                RecordCount = DBDA.Fill(DBDT);
            }
            catch (Exception e)
            {
                Exception = "ExecQuery error: \n" + e.Message.ToString();
            }
            finally
            {
                if (DBConn.State == ConnectionState.Open)
                {
                    DBConn.Close();
                }
            }
        }

        /// <summary>
        /// Executes a nonquery statement against the database.
        /// </summary>
        /// <param name="nonQuery"></param>
        public void ExecNonQuery(string nonQuery)
        {
            RecordCount = 0;
            Exception = "";

            try
            {
                DBConn.Open();
                DBComm = new SqlCommand(nonQuery, DBConn);

                //Load parameters
                foreach(SqlParameter p in Parameters)
                {
                    DBComm.Parameters.Add(p);
                }

                Parameters.Clear();

                //Execute the command
                DBComm.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                Exception = "ExecNonQuery error: \n" + e.Message.ToString();
            }
            finally
            {
                if (DBConn.State == ConnectionState.Open)
                {
                    DBConn.Close();
                }
            }

        }

        /// <summary>
        /// Executes the specified stored procedure.  If the procedure requires input paramters, add them by calling "AddParam" before 
        /// executing the stored procedure.
        /// </summary>
        /// <param name="procedure"></param>
        public void ExecuteStoredProcedure(string procedure)
        {
            RecordCount = 0;
            Exception = "";

            try
            {
                //Initialize the command object.
                DBConn.Open();
                DBComm = new SqlCommand()
                {
                    Connection = DBConn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = procedure
                };

                //Load the parameters into the Command object.
                foreach(SqlParameter p in Parameters)
                {
                    DBComm.Parameters.Add(p);
                }

                Parameters.Clear();

                //Execute the command
                DBComm.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                Exception = "ExecuteStoredProcedure error: \n" + e.Message.ToString();
            }
            finally
            {
                if(DBConn.State == ConnectionState.Open)
                {
                    DBConn.Close();
                }
            }

        }

        /// <summary>
        /// Returns a single output parameter from a stored procedure, of type specified by caller.
        /// </summary>
        /// <param name="procedure"></param>
        /// <param name="outputParameter"></param>
        /// <returns></returns>
        public object GetReturnValueFromStoredProcedure(string procedure, object outputParameter)
        {
            object outputValue;

            RecordCount = 0;
            Exception = "";

            try
            {
                DBConn.Open();

                DBComm = new SqlCommand()
                {
                    Connection = DBConn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = procedure
                };
                
                //Load the input and output parameters
                foreach(SqlParameter p in Parameters)
                {
                    DBComm.Parameters.Add(p);
                }
                foreach(SqlParameter o in OutputParameters)
                {
                    DBComm.Parameters.Add(o).Direction = ParameterDirection.Output;
                }

                Parameters.Clear();
                OutputParameters.Clear();

                //Execute the command
                DBComm.ExecuteNonQuery();
                outputValue = DBComm.Parameters[0];

                return outputValue;

            }
            catch (Exception e)
            {
                Exception = "ReturnValueFromStoredProcedure error: \n" + e.Message.ToString();
                return null;
            }
            finally
            {
                if(DBConn.State == ConnectionState.Open)
                {
                    DBConn.Close();
                }
            }

        }

        /// <summary>
        /// Populates the DBDT object with the results of the stored procedure.  This function does not return any value(s), but the 
        /// DBDT is accessible once the command has been executed.
        /// </summary>
        /// <remarks>
        /// 7/9/20 - TODO: In version 2, change this to return a Dataset object to the caller
        /// </remarks>
        /// <param name="procedure"></param>
        public void GetDatasetFromStoredProcedure(string procedure)
        {
            RecordCount = 0;
            Exception = "";

            try
            {
                //Initialize the command object.
                DBConn.Open();
                DBComm = new SqlCommand()
                {
                    Connection = DBConn,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = procedure
                };

                //Load the parameters into the Command object.
                foreach (SqlParameter p in Parameters)
                {
                    DBComm.Parameters.Add(p);
                }

                Parameters.Clear();

                //Execute the command
                DBDT = new DataTable();
                DBDA = new SqlDataAdapter(DBComm);
                RecordCount = DBDA.Fill(DBDT);

            }
            catch (Exception e)
            {
                Exception = "GetDatasetFromStoredProcedure error: \n" + e.Message.ToString();
            }
            finally
            {
                if (DBConn.State == ConnectionState.Open)
                {
                    DBConn.Close();
                }
            }
        }


    }
}
