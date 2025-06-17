using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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

        #region Additions
        public DataTable SearchBooks(string userInput, bool isGeneralSearch = false)
        {
            var stopWords = new HashSet<string> {
                "a", "an", "the", "is", "with", "by", "for", "in", "book",
                "books", "author", "authors", "available", "search", "find",
                "get", "give", "me", "what", "about"
            };

            if (isGeneralSearch)
            {
                string generalQuery = @"SELECT TOP 10 BookID, Title, Author, BooksAvailable 
                                FROM Books 
                                ORDER BY NEWID()";
                return GetDataN(generalQuery, new SqlParameter[0]);
            }

            var titleAuthorMatch = Regex.Match(userInput, "\"(.+?)\"\\s+by\\s+\"(.+?)\"", RegexOptions.IgnoreCase);
            if (titleAuthorMatch.Success)
            {
                string title = titleAuthorMatch.Groups[1].Value;
                string author = titleAuthorMatch.Groups[2].Value;

                string exactQuery = @"SELECT TOP 5 BookID, Title, Author, BooksAvailable 
                              FROM Books 
                              WHERE Title LIKE @title AND Author LIKE @author";
                SqlParameter[] titleAuthorParams = {
            new SqlParameter("@title", $"%{title}%"),
            new SqlParameter("@author", $"%{author}%")
        };
                return GetDataN(exactQuery, titleAuthorParams);
            }

            var titleMatch = Regex.Match(userInput, "\"(.+?)\"", RegexOptions.IgnoreCase);
            if (titleMatch.Success)
            {
                string title = titleMatch.Groups[1].Value;
                string titleQuery = @"SELECT TOP 5 BookID, Title, Author, BooksAvailable 
                              FROM Books 
                              WHERE Title LIKE @title";
                return GetDataN(titleQuery, new[] { new SqlParameter("@title", $"%{title}%") });
            }

            var keywords = userInput.Split(new[] { ' ', ',', '.', '?', '!' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(word => !stopWords.Contains(word.ToLower()))
                .Distinct()
                .ToList();

            if (keywords.Count == 0) return new DataTable();

            var queryBuilder = new StringBuilder("SELECT TOP 5 BookID, Title, Author, BooksAvailable FROM Books WHERE ");
            var parameters = new List<SqlParameter>();

            for (int i = 0; i < keywords.Count; i++)
            {
                string paramName = $"@keyword{i}";
                queryBuilder.Append($"(Title LIKE {paramName} OR Author LIKE {paramName})");

                if (i < keywords.Count - 1)
                    queryBuilder.Append(" OR ");

                parameters.Add(new SqlParameter(paramName, $"%{keywords[i]}%"));
            }

            return GetDataN(queryBuilder.ToString(), parameters.ToArray());
        }
        #endregion
    }
}