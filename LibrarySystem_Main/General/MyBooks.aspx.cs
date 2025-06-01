using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Text;
using System.Web.UI;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.IO;
using LibrarySystem_Main.Models;

namespace LibrarySystem_Main.General
{
    public partial class MyBooks : BasePage
    {
        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                await BindMyBooks();
        }

        private async Task BindMyBooks()
        {
            var response = await APIClient.Instance.GetAsync($"api/books/borrowed/{CurrentUser.ClientID}");
            if (response.IsSuccessStatusCode)
            {
                var borrows = JsonConvert.DeserializeObject<List<BorrowInfo>>(await response.Content.ReadAsStringAsync());
                gvMyBooks.DataSource = borrows;
                gvMyBooks.DataBind();
            }
            else
            {
                ShowErrorMessage("API Error: " + response.StatusCode + response.ReasonPhrase);
            }
        }

        protected async void btnReturn_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string bookId = btn.CommandArgument.ToString();

            var response = await APIClient.Instance.PostAsync("api/books/request-return",
                new StringContent(JsonConvert.SerializeObject(new
                {
                    ClientID = CurrentUser.ClientID,
                    BookID = bookId
                }), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                await BindMyBooks();
                ShowSuccessMessage("Return requested! Waiting for admin confirmation.");
            }
            else
            {
                ShowErrorMessage("API Error: " + response.StatusCode + response.ReasonPhrase);
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


        #region Review
        protected async void btnSubmitReview_Click(object sender, EventArgs e)
        {
            btnSubmitReview.Enabled = false;
            btnSubmitReview.Text = "Processing...";

            string bookId = hdnSelectedBookId.Value;
            string reviewText = txtReviewText.Text;

            var user = (User)Session["CurrentUser"];
            if (user == null)
            {
                ShowErrorMessage("User not logged in.");
                return;
            }

            var returnResponse = await APIClient.Instance.PostAsync("api/books/request-return",
                new StringContent(JsonConvert.SerializeObject(new
                {
                    ClientID = user.ClientID,
                    BookID = bookId
                }), Encoding.UTF8, "application/json"));

            if (!returnResponse.IsSuccessStatusCode)
            {
                ShowErrorMessage("Return request failed.");
                return;
            }

            // Process review if text exists
            if (!string.IsNullOrEmpty(reviewText))
            {
                var reviewRequest = new
                {
                    UserID = user.ClientID,
                    BookID = bookId,
                    ReviewText = reviewText
                };

                var reviewResponse = await APIClient.Instance.PostAsync("api/review/submit",
                    new StringContent(JsonConvert.SerializeObject(reviewRequest),
                    Encoding.UTF8, "application/json"));

                if (!reviewResponse.IsSuccessStatusCode)
                {
                    ShowErrorMessage("Review submission failed.");
                    return;
                }
            }

            ShowSuccessMessage("Book returned and review submitted successfully!");
            await BindMyBooks();
        }



        public class ReturnReviewResult
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
        }
        #endregion
    }
}