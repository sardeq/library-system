using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using SchoolSystem.Data;

namespace LibrarySystem_WebService.Auth
{
    public class Login
    {
        private readonly DatabaseService databaseService = new DatabaseService();

        public LoginResult LoginClick(string username, string password)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Username", username),
                new SqlParameter("@Password", password)
            };

            string query = @"SELECT ClientID, Type FROM Clients 
                     WHERE ClientName = @Username 
                     AND Password = @Password";

            DataTable result = databaseService.GetDataN(query, parameters);

            if (result.Rows.Count > 0)
            {
                var row = result.Rows[0];

                return new LoginResult
                {
                    Success = true,
                    Message = "Login successful",
                    ClientID = Convert.ToInt32(row["ClientID"]),
                    UserType = row["Type"].ToString()
                };
            }
            else
            {
                return new LoginResult
                {
                    Success = false,
                    Message = "Invalid username or password"
                };
            }
        }


        private void RedirectBasedOnUserType(int userType)
        {
            switch (userType)
            {
                case 1:
                    HttpContext.Current.Response.Redirect("~/Admin/AdminDefault.aspx");
                    break;
                case 2:
                    HttpContext.Current.Response.Redirect("~/Teacher/TeacherDefault.aspx");
                    break;
                case 3:
                    HttpContext.Current.Response.Redirect("~/Student/StudentDefault.aspx");
                    break;
                default:
                    HttpContext.Current.Response.Redirect("~/Login.aspx");
                    break;
            }
        }
    }

    public class LoginResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? ClientID { get; set; }
        public string UserType { get; set; }
    }

}
