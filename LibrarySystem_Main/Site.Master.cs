using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace LibrarySystem_Main
{
    public partial class SiteMaster : MasterPage
    {
        public Models.User CurrentUser
        {
            get => Session["CurrentUser"] as Models.User;
            set => Session["CurrentUser"] = value;
        }

        protected void Page_Load(object sender, EventArgs e)
        {


            bool isLoggedIn = Session["LoggedIn"] != null && (bool)Session["LoggedIn"];

            sidebar.Visible = isLoggedIn;
           

            if (isLoggedIn)
            {
                if (CurrentUser == null)
                {
                    Response.Redirect("~/Login.aspx");
                    return;
                }

                ApplyLanguageSettings();

                if (!IsPostBack)
                {

                    ApplyRoleStyling();
                    UpdateWelcomeMessage();
                    CheckNotifications();
                }
                ConfigureNavigationMenu();
            }
        }

        private void ApplyLanguageSettings()
        {
            var culture = CurrentUser.LanguageID == 2 ? "ar" : "en";
            htmlTag.Attributes["dir"] = culture == "ar" ? "rtl" : "ltr";
            htmlTag.Attributes["lang"] = culture;

            styleSheetLink.Attributes["href"] = culture == "ar"
                ? $"~/Content/rtl.css?t={DateTime.Now.Ticks}"
                : $"~/Content/Site.css?t={DateTime.Now.Ticks}";
        }

        private void UpdateWelcomeMessage()
        {
            WelcomeMessage.Text = Session["Username"] != null
                ? $"Logged in as: {Session["Username"]}"
                : "";
        }

        private void ApplyRoleStyling()
        {
            var body = Page.Master.FindControl("body") as HtmlGenericControl;
            if (body != null)
            {
                body.Attributes["class"] = GetRoleClass();
            }
        }

        private void CheckNotifications()
        {
            if (CurrentUser == null) return;

            var response = APIClient.Instance.GetAsync($"api/books/due-soon/{CurrentUser.ClientID}").Result;
            if (response.IsSuccessStatusCode)
            {
                var dueSoon = JsonConvert.DeserializeObject<List<BorrowInfo>>(
                    response.Content.ReadAsStringAsync().Result);
                if (dueSoon.Count > 0)
                {
                    ShowAlert($"You have {dueSoon.Count} books due soon!");
                }
            }

            response = APIClient.Instance.GetAsync($"api/books/has-overdue/{CurrentUser.ClientID}").Result;
            if (response.IsSuccessStatusCode &&
                bool.Parse(response.Content.ReadAsStringAsync().Result))
            {
                ShowAlert("You have overdue books! Account blocked until return.", "danger");
            }
        }

        private void ShowAlert(string message, string type = "warning")
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "alert",
                $@"alert('{message}');", true);
        }

        private string GetRoleClass()
        {
            if (Session["UserType"] == null) return "";

            int userType = Convert.ToInt32(Session["UserType"]);
            switch (userType)
            {
                case 1: return "admin";
                case 2: return "teacher";
                case 3: return "student";
                default: return "";
            }
        }

        private void ConfigureNavigationMenu()
        {
            if (Session["UserType"] == null) return;

            adminMenu.Visible = false;
            teacherMenu.Visible = false;
            studentMenu.Visible = false;

            switch (Convert.ToInt32(Session["UserType"]))
            {
                case 1:
                    adminMenu.Visible = true;
                    break;
                case 2:
                    teacherMenu.Visible = true;
                    break;
                case 3:
                    studentMenu.Visible = true;
                    break;
            }

            adminMenu.DataBind();
            teacherMenu.DataBind();
            studentMenu.DataBind();
        }


        protected void Logout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Login.aspx");
        }
    }
}