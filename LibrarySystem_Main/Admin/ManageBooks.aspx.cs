using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace LibrarySystem_Main.Admin
{
    public partial class ManageBooks : BasePage
    {
        protected async void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {
                await BindBooks();
            }
        }

        private async Task BindBooks()
        {
            var response = await APIClient.Instance.GetAsync("api/books");
            if (response.IsSuccessStatusCode)
            {
                var books = JsonConvert.DeserializeObject<List<Book>>(
                    await response.Content.ReadAsStringAsync());
                gvBooks.DataSource = books;
                gvBooks.DataBind();
            }
            else
            {
                ShowErrorMessage(response.RequestMessage.ToString());
            }
        }

        protected void gvBooks_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditBook" || e.CommandName == "DeleteBook")
            {
                LinkButton btn = (LinkButton)e.CommandSource;
                GridViewRow row = (GridViewRow)btn.NamingContainer;
                int rowIndex = row.RowIndex;

                string bookId = gvBooks.DataKeys[rowIndex].Value.ToString();

                if (e.CommandName == "EditBook")
                {
                    LoadBookDetails(bookId);
                    ScriptManager.RegisterStartupScript(this, GetType(),
                        "openModal", "$('#bookModal').modal('show');", true);
                }
                else if (e.CommandName == "DeleteBook")
                {
                    RegisterAsyncTask(new PageAsyncTask(async () =>
                    {
                        await DeleteBook(bookId);
                    }));
                }
            }
        }

        private async Task DeleteBook(string bookId)
        {
            var response = await APIClient.Instance.DeleteAsync($"api/books/{bookId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse>(content);

                if (result.Success)
                {
                    await BindBooks();
                    ShowSuccessMessage("Book deleted successfully.");
                }
                else
                {
                    ShowErrorMessage($"Failed to delete book: {result.ErrorMessage}");
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ShowErrorMessage($"Failed to delete book: {errorContent}");
            }
        }

        private async void LoadBookDetails(string bookId)
        {
            var response = await APIClient.Instance.GetAsync($"api/books/{bookId}");
            if (response.IsSuccessStatusCode)
            {
                var book = JsonConvert.DeserializeObject<Book>(
                    await response.Content.ReadAsStringAsync());
                txtBookID.Text = book.BookID;
                txtTitle.Text = book.Title;
                txtAuthor.Text = book.Author;
                txtReleaseDate.Text = book.ReleaseDate.ToString("yyyy-MM-dd");
                txtBorrowDuration.Text = book.BorrowDuration.ToString();
                txtTotalCopies.Text = book.BooksAvailable.ToString();
                ddlBorrowType.SelectedValue = book.BorrowType.ToString();
            }
            else
            {
                ShowErrorMessage("Failed to load book details.");
            }
        }

        protected async void btnSave_Click(object sender, EventArgs e)
        {
            var book = new Book
            {
                BookID = txtBookID.Text,
                Title = txtTitle.Text,
                Author = txtAuthor.Text,
                ReleaseDate = DateTime.Parse(txtReleaseDate.Text),
                BorrowDuration = Convert.ToInt32(txtBorrowDuration.Text),
                BooksAvailable = Convert.ToInt32(txtTotalCopies.Text),
                BorrowType = Convert.ToInt32(ddlBorrowType.SelectedValue)
            };

            var content = new StringContent(JsonConvert.SerializeObject(book),
                System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            if (string.IsNullOrEmpty(txtBookID.Text))
                response = await APIClient.Instance.PostAsync("api/books", content);
            else
                response = await APIClient.Instance.PutAsync("api/books", content);

            if (response.IsSuccessStatusCode)
            {
                ShowSuccessMessage("User saved successfully");
                await BindBooks();
                ScriptManager.RegisterStartupScript(this, GetType(),
                    "closeModal", "$('#bookModal').modal('hide');", true);
            }
        }


        protected void btnAddNew_Click(object sender, EventArgs e)
        {
            txtBookID.Text = string.Empty;
            txtTitle.Text = string.Empty;
            txtAuthor.Text = string.Empty;
            txtReleaseDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
            txtBorrowDuration.Text = "14";
            txtTotalCopies.Text = "1";
            ddlBorrowType.SelectedValue = "0";

            ScriptManager.RegisterStartupScript(this, GetType(),
                "openModal", "$('#bookModal').modal('show');", true);
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