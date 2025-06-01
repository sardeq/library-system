using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Text;
using System.Web.UI;

namespace LibrarySystem_Main.General
{
    public partial class Borrow : BasePage
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
                var books = JsonConvert.DeserializeObject<List<Book>>(await response.Content.ReadAsStringAsync());
                if (CurrentUser.Type == (int)UserType.Student)
                    books = books.Where(b => b.BorrowType == 0).ToList();
                gvBooks.DataSource = books.Where(b => b.BooksAvailable > 0).ToList();
                gvBooks.DataBind();
            }
        }

        private List<string> GetSelectedBookIds()
        {
            List<string> selectedIds = new List<string>();
            foreach (GridViewRow row in gvBooks.Rows)
            {
                CheckBox chkSelect = (CheckBox)row.FindControl("chkSelect");
                if (chkSelect != null && chkSelect.Checked)
                {
                    string bookId = gvBooks.DataKeys[row.RowIndex].Value.ToString();
                    selectedIds.Add(bookId);
                }
            }
            return selectedIds;
        }

        protected async void btnBorrow_Click(object sender, EventArgs e)
        {
            var selectedBookIds = GetSelectedBookIds();
            if (selectedBookIds.Count == 0)
            {
                ShowErrorMessage("Please select books to borrow.");
                return;
            }

            var borrowRequest = new BorrowRequest
            {
                ClientID = CurrentUser.ClientID,
                BookIDs = selectedBookIds
            };
            var content = new StringContent(JsonConvert.SerializeObject(borrowRequest),
                Encoding.UTF8, "application/json");
            var response = await APIClient.Instance.PostAsync("api/books/borrow", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(responseContent);

                if (result.Success == true)
                {
                    ShowErrorMessage($"{result.Message}");
                    await BindBooks();
                    ClearCheckboxes();
                }
                else
                {
                    var errors = ((IEnumerable<dynamic>)result.Errors)
                               .Select(err => (string)err).ToList();
                    ShowErrorMessage($"Failed: {string.Join(", ", errors)}");
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                ShowErrorMessage($"API Error: {error}");
            }
        }

        private void ClearCheckboxes()
        {
            foreach (GridViewRow row in gvBooks.Rows)
            {
                CheckBox chkSelect = (CheckBox)row.FindControl("chkSelect");
                if (chkSelect != null)
                    chkSelect.Checked = false;
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