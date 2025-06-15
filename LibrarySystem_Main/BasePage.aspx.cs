using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;

namespace LibrarySystem_Main
{
    public partial class BasePage : Page
    {
        public Models.User CurrentUser { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            if (Session["LoggedIn"] == null || !(bool)Session["LoggedIn"])
            {
                Session.Clear();
                Response.Redirect("~/Login.aspx");
            }
            else
            {
                CurrentUser = Session["CurrentUser"] as Models.User;
                if (CurrentUser == null)
                {
                    Response.Redirect("~/Login.aspx");
                }

                string currentPath = Request.AppRelativeCurrentExecutionFilePath ?? "";
                if (currentPath.StartsWith("~/general/", StringComparison.OrdinalIgnoreCase) ||
                    currentPath.Equals("~/general/account.aspx", StringComparison.OrdinalIgnoreCase))
                {
                    base.OnLoad(e);
                    return;
                }

                RedirectBasedOnUserType();
            }
            base.OnLoad(e);
        }

        protected override void InitializeCulture()
        {
            var currentUser = Session["CurrentUser"] as Models.User;
            if (currentUser != null)
            {
                var culture = currentUser.LanguageID == 2 ? "ar" : "en";
                System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            }
            else
            {
                base.InitializeCulture();
            }
        }

        private void RedirectBasedOnUserType()
        {
            var userType = Convert.ToInt32(Session["UserType"]);
            string currentPath = Request.AppRelativeCurrentExecutionFilePath ?? "";

            switch (userType)
            {
                case 1 when !currentPath.StartsWith("~/admin/", StringComparison.OrdinalIgnoreCase):
                    Response.Redirect("~/Admin/AdminDefault.aspx");
                    break;
                case 2 when !currentPath.StartsWith("~/teacher/", StringComparison.OrdinalIgnoreCase):
                    Response.Redirect("~/Teacher/TeacherDefault.aspx");
                    break;
                case 3 when !currentPath.StartsWith("~/student/", StringComparison.OrdinalIgnoreCase):
                    Response.Redirect("~/Student/StudentDefault.aspx");
                    break;
            }
        }

        private void ShowAlert(string message, string type = "warning")
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "alert",
                $@"alert('{message}');", true);
        }
    }

    [Serializable]
    public class Book
    {
        public string BookID { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int BorrowType { get; set; }
        public int BooksAvailable { get; set; }
        public int BorrowDuration { get; set; }
    }

    public class BorrowInfo
    {
        public int BorrowID { get; set; }
        public int ClientID { get; set; }
        public string ClientName { get; set; }
        public string BookID { get; set; }
        public string Title { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public bool PendingConfirmation { get; set; }
        public bool Returned { get; set; }
    }

    public class ReviewRequest
    {
        public int UserID { get; set; }
        public string BookID { get; set; }
        public string ReviewText { get; set; }
    }

    public enum UserType
    {
        Admin = 1,
        Teacher = 2,
        Student = 3
    }

    public class BorrowRequest
    {
        public int ClientID { get; set; }
        public List<string> BookIDs { get; set; }
    }

    public class ReturnRequest
    {
        public int ClientID { get; set; }
        public string BookID { get; set; }
    }

    public enum Status
    {
        Blocked = 0,
        Active = 1
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class UpdateLanguageRequest
    {
        public int ClientId { get; set; }
        public int LanguageId { get; set; }
    }

    public class ChatMessage
    {
        public string Sender { get; set; }
        public string Text { get; set; }
        public string CssClass { get; set; }
    }
}