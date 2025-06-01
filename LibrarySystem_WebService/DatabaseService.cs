using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SchoolSystem.Data
{
    public class DatabaseService
    {
        private readonly string connString = ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;

        #region Queries
        public DataTable GetData(string query)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                DataTable dt = new DataTable();
                try
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
                    {
                        da.Fill(dt);
                        dt.TableName = "Result"; // Default name if not set later
                    }
                }
                catch (Exception ex)
                {
                    dt.TableName = "Error";
                    dt.Columns.Add("Error");
                    dt.Rows.Add(ex.Message);
                }
                return dt;
            }
        }

        public bool ExecuteNonQuery(string query, SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddRange(parameters);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Database Error: " + ex.Message);
                }
            }
        }

        public object ExecuteScalar(string query, SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddRange(parameters);
                try
                {
                    conn.Open();
                    return cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    throw new Exception("Database Error: " + ex.Message);
                }
            }
        }

        #endregion

        #region Procedure Handeling
        public bool ExecuteQuery(string procedureName, SqlParameter[] parameters, CommandType commandType = CommandType.StoredProcedure)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand(procedureName, conn);
                cmd.CommandType = commandType;
                cmd.Parameters.AddRange(parameters);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Database Error: " + ex.Message);
                }
            }
        }

        public DataTable GetProData(string query, CommandType commandType = CommandType.StoredProcedure)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.CommandType = commandType;

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();

                try
                {
                    conn.Open();
                    da.Fill(dt);
                }
                catch (Exception ex)
                {
                    throw new Exception("Database Error: " + ex.Message);
                }

                return dt;
            }
        }

        public DataTable GetProDataWithPara(string query, SqlParameter[] parameters = null)
        {
            CommandType commandType = CommandType.StoredProcedure;
            using (SqlConnection conn = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.CommandType = commandType;

                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    try
                    {
                        conn.Open();
                        da.Fill(dt);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Database Error: " + ex.Message);
                    }

                    return dt;
                }
            }
        }


        #endregion

        #region Others

        //for filters, will find a new way later
        public DataTable GetDataN(string query, SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddRange(parameters);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();

                try
                {
                    conn.Open();
                    da.Fill(dt);
                }
                catch (Exception ex)
                {
                    throw new Exception("Database Error: " + ex.Message);
                }

                return dt;
            }
        }


        #endregion
    }
}