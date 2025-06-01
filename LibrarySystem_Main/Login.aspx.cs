using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Net.Http.Headers;
using System.Net.Http;



namespace LibrarySystem_Main
{
    public partial class Login : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["LoggedIn"] != null && (bool)Session["LoggedIn"])
            {
                RedirectBasedOnUserType();
            }
        }

        protected async void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            try
            {
                var content = new StringContent(
                    Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        Username = username,
                        Password = password
                    }),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var response = await APIClient.Instance.PostAsync("api/login/initiateLogin", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var result = Newtonsoft.Json.JsonConvert.DeserializeObject<LoginResult>(jsonString);

                    if (result.Success)
                    {
                        Session["ClientID"] = result.ClientID;
                        Session["Username"] = username;
                        Session["LoggedIn"] = true;
                        Session["UserType"] = result.UserType;

                        var userResponse = await APIClient.Instance.GetAsync($"api/user/{result.ClientID}");
                        if (userResponse.IsSuccessStatusCode)
                        {
                            var userJson = await userResponse.Content.ReadAsStringAsync();
                            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.User>(userJson);
                            Session["CurrentUser"] = user;
                        }
                        else
                        {
                            lblMessage.Text = "Error fetching user details. Please try again.";
                            return;
                        }

                        RedirectBasedOnUserType();
                    }
                    else
                    {
                        lblMessage.Text = result.Message;
                    }
                }
                else
                {
                    lblMessage.Text = "Login failed. Please try again.";
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Error: {ex.Message}";
            }
        }

        private void RedirectBasedOnUserType()
        {
            var userType = Convert.ToInt32(Session["UserType"]);
            string currentPath = Request.AppRelativeCurrentExecutionFilePath ?? "";

            if (currentPath.Equals("~/general/account.aspx", StringComparison.OrdinalIgnoreCase))
                return;

            string redirectUrl = null;

            switch (userType)
            {
                case 1 when !currentPath.StartsWith("~/admin/", StringComparison.OrdinalIgnoreCase):
                    redirectUrl = "~/Admin/AdminDefault.aspx";
                    break;
                case 2 when !currentPath.StartsWith("~/teacher/", StringComparison.OrdinalIgnoreCase):
                    redirectUrl = "~/Teacher/TeacherDefault.aspx";
                    break;
                case 3 when !currentPath.StartsWith("~/student/", StringComparison.OrdinalIgnoreCase):
                    redirectUrl = "~/Student/StudentDefault.aspx";
                    break;
            }

            if (redirectUrl != null)
            {
                Response.Redirect(redirectUrl, false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }
    }
}