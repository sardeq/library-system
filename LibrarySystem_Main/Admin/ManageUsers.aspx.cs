using LibrarySystem_Main.LibraryWebServiceRef;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LibrarySystem_Main
{
    public partial class ManageUsers : BasePage
    {
        public class LookupItem
        {
            public int ID { get; set; }
            public string Description { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                RegisterAsyncTask(new PageAsyncTask(BindDataAsync));
            }
        }

        private async Task BindDataAsync()
        {
            await BindUsersAsync();
            await BindLookupDataAsync();
        }

        private async Task BindUsersAsync()
        {
            try
            {
                var response = await APIClient.Instance.GetAsync("api/user");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var users = JsonConvert.DeserializeObject<List<LibrarySystem_Main.Models.User>>(jsonString);
                    gvUsers.DataSource = users;
                    gvUsers.DataBind();
                }
                else
                {
                    ShowErrorMessage("Error loading users");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading users: {ex.Message}");
            }
        }

        private async Task BindLookupDataAsync()
        {
            try
            {
                var typeResponse = await APIClient.Instance.GetAsync("api/user/types");
                if (typeResponse.IsSuccessStatusCode)
                {
                    var typeJson = await typeResponse.Content.ReadAsStringAsync();
                    var types = JsonConvert.DeserializeObject<List<TypeInfo>>(typeJson);
                    ddlUserType.DataSource = types;
                    ddlUserType.DataTextField = "TypeDesc";
                    ddlUserType.DataValueField = "TypeID";
                    ddlUserType.DataBind();
                }

                var statusResponse = await APIClient.Instance.GetAsync("api/user/statuses");
                if (statusResponse.IsSuccessStatusCode)
                {
                    var statusJson = await statusResponse.Content.ReadAsStringAsync();
                    var statuses = JsonConvert.DeserializeObject<List<dynamic>>(statusJson);
                    ddlStatus.DataSource = statuses;
                    ddlStatus.DataTextField = "StatusDesc";
                    ddlStatus.DataValueField = "StatusID";
                    ddlStatus.DataBind();
                }

                var genderResponse = await APIClient.Instance.GetAsync("api/user/genders");
                if (genderResponse.IsSuccessStatusCode)
                {
                    var genderJson = await genderResponse.Content.ReadAsStringAsync();
                    var genders = JsonConvert.DeserializeObject<List<dynamic>>(genderJson);
                    ddlGender.DataSource = genders;
                    ddlGender.DataTextField = "GenderDesc";
                    ddlGender.DataValueField = "GenderID";
                    ddlGender.DataBind();
                }

                var languageResponse = await APIClient.Instance.GetAsync("api/user/languages");
                if (languageResponse.IsSuccessStatusCode)
                {
                    var languageJson = await languageResponse.Content.ReadAsStringAsync();
                    var languages = JsonConvert.DeserializeObject<List<dynamic>>(languageJson);
                    ddlLanguage.DataSource = languages;
                    ddlLanguage.DataTextField = "LanguageDesc";
                    ddlLanguage.DataValueField = "LanguageID";
                    ddlLanguage.DataBind();
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading lookup data: {ex.Message}");
            }
        }

        protected async void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (!int.TryParse(txtBookQuota.Text, out int booksQuota))
                {
                    ShowErrorMessage("Book Quota must be a number.");
                    return;
                }
                if (!int.TryParse(txtBorrowDuration.Text, out int borrowDuration))
                {
                    ShowErrorMessage("Borrow Duration must be a number.");
                    return;
                }

                string password = txtPassword.Text.Trim();
                if (Convert.ToInt32(hdnClientID.Value) != 0 && string.IsNullOrEmpty(password))
                {
                    var getUserResponse = await APIClient.Instance.GetAsync($"api/user/{hdnClientID.Value}");
                    if (getUserResponse.IsSuccessStatusCode)
                    {
                        var jsonString = await getUserResponse.Content.ReadAsStringAsync();
                        var existingUser = JsonConvert.DeserializeObject<LibrarySystem_Main.Models.User>(jsonString);
                        password = existingUser.Password;
                    }
                }

                var user = new User
                {
                    ClientID = Convert.ToInt32(hdnClientID.Value),
                    ClientName = txtUsername.Text.Trim(),
                    Password = password,
                    Name = txtFullName.Text.Trim(),
                    BirthDate = DateTime.Parse(txtBirthDate.Text),
                    Type = Convert.ToInt32(ddlUserType.SelectedValue),
                    Status = Convert.ToInt32(ddlStatus.SelectedValue),
                    Gender = Convert.ToInt32(ddlGender.SelectedValue),
                    LanguageID = Convert.ToInt32(ddlLanguage.SelectedValue),
                    BooksQuota = booksQuota,
                    BorrowDuration = borrowDuration
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(user),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var response = await APIClient.Instance.PostAsync("api/user", content);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    bool success = bool.Parse(result);
                    if (success)
                    {
                        ShowSuccessMessage("User saved successfully");
                        await BindUsersAsync();
                        ScriptManager.RegisterStartupScript(this, GetType(), "closeModal", "$('#userModal').modal('hide');", true);
                    }
                }
                else
                {
                    ShowErrorMessage("Error saving user");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error saving user: {ex.Message}");
            }
        }

        protected async void gvUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                int clientId = Convert.ToInt32(e.CommandArgument);

                if (e.CommandName == "DeleteUser")
                {
                    var response = await APIClient.Instance.DeleteAsync($"api/user/{clientId}");
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ApiResponse>(content);

                        if (result.Success)
                        {
                            ShowSuccessMessage(result.ErrorMessage);
                            await BindUsersAsync();
                        }
                        else
                        {
                            ShowErrorMessage(result.ErrorMessage);
                        }
                    }
                    else
                    {
                        ShowErrorMessage("Error deleting user: " + response.ReasonPhrase);
                    }
                }
                else if (e.CommandName == "EditUser")
                {
                    var response = await APIClient.Instance.GetAsync($"api/user/{clientId}");
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        var user = JsonConvert.DeserializeObject<LibrarySystem_Main.Models.User>(jsonString);

                        if (user != null)
                        {
                            PopulateForm(user);
                            ScriptManager.RegisterStartupScript(this, GetType(), "openModal", "$('#userModal').modal('show');", true);
                        }
                    }
                    else
                    {
                        ShowErrorMessage("Error loading user data");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error processing request: {ex.Message}");
            }
        }

        private void PopulateForm(LibrarySystem_Main.Models.User user)
        {
            hdnClientID.Value = user.ClientID.ToString();
            txtUsername.Text = user.ClientName;
            txtPassword.Text = user.Password;
            txtFullName.Text = user.Name;
            txtBirthDate.Text = user.BirthDate.ToString("yyyy-MM-dd");
            ddlUserType.SelectedValue = user.Type.ToString();
            ddlStatus.SelectedValue = user.Status.ToString();
            ddlGender.SelectedValue = user.Gender.ToString();
            ddlLanguage.SelectedValue = user.LanguageID.ToString();
            txtBookQuota.Text = user.BooksQuota.ToString();
            txtBorrowDuration.Text = user.BorrowDuration.ToString();

            ScriptManager.RegisterStartupScript(this, GetType(), "updateTitle",
                "document.getElementById('modalTitle').innerText = 'Edit User';", true);
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

    public class TypeInfo
    {
        public int TypeID { get; set; }
        public string TypeDesc { get; set; }
    }
}