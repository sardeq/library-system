using System;
using LibrarySystem_Main.LibraryWebServiceRef;
using System.Web.UI;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;

namespace LibrarySystem_Main.General
{
    public partial class Account : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadUserData();
                ddlLanguage.SelectedValue = CurrentUser.LanguageID.ToString();
            }
        }

        protected async void ddlLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            int clientId = CurrentUser.ClientID;
            int languageId = Convert.ToInt32(ddlLanguage.SelectedValue);

            using (var httpClient = new HttpClient())
            {
                var request = new UpdateLanguageRequest { ClientId = clientId, LanguageId = languageId };
                string json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await APIClient.Instance.PostAsync("api/user/update-language", content);

                if (response.IsSuccessStatusCode)
                {
                    CurrentUser.LanguageID = languageId;
                    Session["CurrentUser"] = CurrentUser;

                    ShowSuccessMessage("Language updated successfully");

                    Response.Redirect(Request.Url.AbsolutePath + "?t=" + DateTime.Now.Ticks);
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    ShowErrorMessage($"Failed to update language: {errorContent}");
                    ddlLanguage.SelectedValue = CurrentUser.LanguageID.ToString();
                }
            }
        }

        protected void Logout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Login.aspx");
        }

        private void LoadUserData()
        {
            var client = WebServiceClient.Instance;
            int clientId = Convert.ToInt32(Session["ClientID"]);

            try
            {
                var user = client.GetUserById(clientId);
                if (user != null)
                {
                    txtUsername.Text = user.ClientName;
                    txtFullName.Text = user.Name;
                    txtBirthDate.Text = user.BirthDate.ToString("yyyy-MM-dd");
                    txtUserType.Text = user.TypeDesc;
                    txtBookQuota.Text = user.BooksQuota.ToString();
                    txtBorrowDuration.Text = user.BorrowDuration.ToString() + " days";
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading account data: {ex.Message}");
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            var client = WebServiceClient.Instance;
            int clientId = Convert.ToInt32(Session["ClientID"]);

            try
            {
                var existingUser = client.GetUserById(clientId);
                if (existingUser == null)
                {
                    ShowErrorMessage("User not found.");
                    return;
                }

                existingUser.ClientName = txtUsername.Text.Trim();
                existingUser.Name = txtFullName.Text.Trim();
                existingUser.BirthDate = DateTime.Parse(txtBirthDate.Text);

                bool success = client.UpdateAccount(existingUser);
                if (success)
                {
                    ShowSuccessMessage("Account updated successfully");
                    LoadUserData();
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error updating account: {ex.Message}");
            }
        }

        protected void btnChangePassword_Click(object sender, EventArgs e)
        {
            string newPassword = txtNewPassword.Text.Trim();

            if (newPassword != txtConfirmPassword.Text.Trim())
            {
                ShowErrorMessage("New passwords do not match");
                return;
            }

            var client = WebServiceClient.Instance;
            int clientId = Convert.ToInt32(Session["ClientID"]);

            try
            {
                bool success = client.ChangePassword(
                    clientId,
                    txtCurrentPassword.Text,
                    newPassword
                );

                if (success)
                {
                    ShowSuccessMessage("Password changed successfully");
                    ScriptManager.RegisterStartupScript(this, GetType(),
                        "closeModal", "$('#passwordModal').modal('hide');", true);
                }
                else
                {
                    ShowErrorMessage("Current password is incorrect");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error changing password: {ex.Message}");
            }
        }

        private void ShowSuccessMessage(string message)
        {
            successMsg.Text = message;
        }

        private void ShowErrorMessage(string message)
        {
            errorMsg.Text = message;
        }
    }
}