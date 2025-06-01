using SchoolSystem.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace LibrarySystem_WebService.Admin
{
    public class ManageUser
    {
        private readonly DatabaseService _db = new DatabaseService();

        public List<User> GetAllUsers()
        {
            string query = @"SELECT c.*, 
                        g.GenderDesc, 
                        s.StatusDesc, 
                        t.TypeDesc, 
                        l.LanguageDesc
                        FROM Clients c
                        LEFT JOIN Gender g ON c.Gender = g.GenderID
                        LEFT JOIN Status s ON c.Status = s.StatusID
                        LEFT JOIN Types t ON c.Type = t.TypeID
                        LEFT JOIN Languages l ON c.LanguageID = l.LanguageID";

            DataTable dt = _db.GetData(query);

            return dt.AsEnumerable().Select(row => new User
            {
                ClientID = Convert.ToInt32(row["ClientID"]),
                ClientName = row["ClientName"].ToString(),
                LanguageID = row["LanguageID"] != DBNull.Value ? Convert.ToInt32(row["LanguageID"]) : 0,
                Name = row["Name"].ToString(),
                BirthDate = row["BirthDate"] != DBNull.Value ? Convert.ToDateTime(row["BirthDate"]) : DateTime.MinValue,
                Age = row["Age"] != DBNull.Value ? Convert.ToInt32(row["Age"]) : 0,
                Gender = row["Gender"] != DBNull.Value ? Convert.ToInt32(row["Gender"]) : 0,
                Status = row["Status"] != DBNull.Value ? Convert.ToInt32(row["Status"]) : 0,
                Type = row["Type"] != DBNull.Value ? Convert.ToInt32(row["Type"]) : 0,
                Password = row["Password"].ToString(),
                BooksQuota = row["BooksQuota"] != DBNull.Value ? Convert.ToInt32(row["BooksQuota"]) : 0,
                BorrowDuration = row["BorrowDuration"] != DBNull.Value ? Convert.ToInt32(row["BorrowDuration"]) : 0,
                GenderDesc = row["GenderDesc"].ToString(),
                StatusDesc = row["StatusDesc"].ToString(),
                TypeDesc = row["TypeDesc"].ToString(),
                LanguageDesc = row["LanguageDesc"].ToString()
            }).ToList();
        }

        public bool UpdateUserLanguage(int clientId, int languageId)
        {
            string query = "UPDATE Clients SET LanguageID = @LanguageID WHERE ClientID = @ClientID";
            SqlParameter[] parameters = {
                new SqlParameter("@LanguageID", languageId),
                new SqlParameter("@ClientID", clientId)
            };
            return _db.ExecuteNonQuery(query, parameters);
        }

        public bool SaveUser(User user)
        {
            string query;
            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter("@ClientName", (object)user.ClientName ?? DBNull.Value),
                new SqlParameter("@LanguageID", user.LanguageID),
                new SqlParameter("@Name", (object)user.Name ?? DBNull.Value),
                new SqlParameter("@BirthDate", user.BirthDate != DateTime.MinValue ? (object)user.BirthDate : DBNull.Value),
                new SqlParameter("@Age", user.Age),
                new SqlParameter("@Gender", user.Gender),
                new SqlParameter("@Status", user.Status),
                new SqlParameter("@Type", user.Type),
                new SqlParameter("@BooksQuota", user.BooksQuota),
                new SqlParameter("@BorrowDuration", user.BorrowDuration)
            };

            if (user.ClientID == 0)
            {
                query = @"INSERT INTO Clients (ClientName, LanguageID, Name, BirthDate, Age, Gender, 
                  Status, Type, Password, BooksQuota, BorrowDuration)
                  VALUES (@ClientName, @LanguageID, @Name, @BirthDate, @Age, @Gender, 
                  @Status, @Type, @Password, @BooksQuota, @BorrowDuration)";
                parameters.Add(new SqlParameter("@Password", (object)user.Password ?? DBNull.Value));
            }
            else
            {
                query = @"UPDATE Clients SET 
                ClientName = @ClientName,
                LanguageID = @LanguageID,
                Name = @Name,
                BirthDate = @BirthDate,
                Age = @Age,
                Gender = @Gender,
                Status = @Status,
                Type = @Type,
                BooksQuota = @BooksQuota,
                BorrowDuration = @BorrowDuration";

                if (!string.IsNullOrEmpty(user.Password))
                {
                    query += ", Password = @Password";
                    parameters.Add(new SqlParameter("@Password", user.Password));
                }

                query += " WHERE ClientID = @ClientID";
                parameters.Add(new SqlParameter("@ClientID", user.ClientID));
            }

            return _db.ExecuteNonQuery(query, parameters.ToArray());
        }

        public bool DeleteUser(int clientId)
        {
            string checkQuery = @"SELECT COUNT(*) FROM Borrow WHERE ClientID = @ClientID";
            SqlParameter[] checkParams = { new SqlParameter("@ClientID", clientId) };
            int borrowedCount = Convert.ToInt32(_db.ExecuteScalar(checkQuery, checkParams));

            if (borrowedCount > 0)
            {
                return false;
            }

            string deleteQuery = "DELETE FROM Clients WHERE ClientID = @ClientID";
            SqlParameter[] deleteParams = { new SqlParameter("@ClientID", clientId) };
            return _db.ExecuteNonQuery(deleteQuery, deleteParams);
        }

        public DataTable GetUserTypes()
        {
            return _db.GetData("SELECT TypeID, TypeDesc FROM Types");
        }

        public DataTable GetStatusList()
        {
            return _db.GetData("SELECT StatusID, StatusDesc FROM Status");
        }

        public DataTable GetGenders()
        {
            return _db.GetData("SELECT GenderID, GenderDesc FROM Gender");
        }

        public DataTable GetLanguages()
        {
            return _db.GetData("SELECT LanguageID, LanguageDesc FROM Languages");
        }

        public User GetUserById(int clientId)
        {
            const string query = @"
                      SELECT c.*, 
                             g.GenderDesc, 
                             s.StatusDesc, 
                             t.TypeDesc, 
                             l.LanguageDesc
                        FROM Clients c
                      LEFT JOIN Gender    g ON c.Gender     = g.GenderID
                      LEFT JOIN Status    s ON c.Status     = s.StatusID
                      LEFT JOIN Types     t ON c.Type       = t.TypeID
                      LEFT JOIN Languages l ON c.LanguageID = l.LanguageID
                       WHERE c.ClientID = @ClientID";

            var p = new[] { new SqlParameter("@ClientID", clientId) };
            var dt = _db.GetDataN(query, p);

            return dt.AsEnumerable().Select(row => new User
            {
                ClientID = (int)row["ClientID"],
                ClientName = (string)row["ClientName"],
                LanguageID = row["LanguageID"] != DBNull.Value
                                 ? (int)row["LanguageID"]
                                 : 1,
                Name = (string)row["Name"],
                BirthDate = row["BirthDate"] != DBNull.Value
                                 ? (DateTime)row["BirthDate"]
                                 : DateTime.MinValue,
                Age = row["Age"] != DBNull.Value
                                 ? (int)row["Age"]
                                 : 0,
                Gender = row["Gender"] != DBNull.Value
                                 ? (int)row["Gender"]
                                 : 0,
                Status = row["Status"] != DBNull.Value
                                 ? (int)row["Status"]
                                 : 0,
                Type = row["Type"] != DBNull.Value
                                 ? (int)row["Type"]
                                 : 0,
                Password = (string)row["Password"],
                BooksQuota = row["BooksQuota"] != DBNull.Value
                                 ? (int)row["BooksQuota"]
                                 : 0,
                BorrowDuration = row["BorrowDuration"] != DBNull.Value
                                 ? (int)row["BorrowDuration"]
                                 : 0,
                GenderDesc = (string)row["GenderDesc"],
                StatusDesc = (string)row["StatusDesc"],
                TypeDesc = (string)row["TypeDesc"],
                LanguageDesc = (string)row["LanguageDesc"]
            }).FirstOrDefault();
        }



        public bool ChangePassword(int clientId, string currentPassword, string newPassword)
        {
            string query = @"UPDATE Clients SET Password = @NewPassword 
                   WHERE ClientID = @ClientID AND Password = @CurrentPassword";

            SqlParameter[] parameters = {
                new SqlParameter("@ClientID", clientId),
                new SqlParameter("@CurrentPassword", SqlDbType.VarChar, 15) { Value = currentPassword ?? (object)DBNull.Value },
                new SqlParameter("@NewPassword", SqlDbType.VarChar, 15) { Value = newPassword?.Trim() ?? (object)DBNull.Value }
            };

            return _db.ExecuteNonQuery(query, parameters);
        }
    }
}