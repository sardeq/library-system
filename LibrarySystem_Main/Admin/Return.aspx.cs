using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Text;

namespace LibrarySystem_Main.Admin
{
    public partial class Return : BasePage
    {
        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                await BindPendingReturns();
            }
        }

        private async Task BindPendingReturns()
        {
            var response = await APIClient.Instance.GetAsync("api/books/pending-returns");
            if (response.IsSuccessStatusCode)
            {
                var pending = JsonConvert.DeserializeObject<List<BorrowInfo>>(
                    await response.Content.ReadAsStringAsync());
                gvPendingReturns.DataSource = pending;
                gvPendingReturns.DataBind();
            }
        }

        protected async void btnConfirmReturns_Click(object sender, EventArgs e)
        {
            foreach (GridViewRow row in gvPendingReturns.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("chkSelect");
                if (chk != null && chk.Checked)
                {
                    int clientId = Convert.ToInt32(gvPendingReturns.DataKeys[row.RowIndex].Values["ClientID"]);
                    string bookId = gvPendingReturns.DataKeys[row.RowIndex].Values["BookID"].ToString();

                    var response = await APIClient.Instance.PostAsync("api/books/confirm-return",
                        new StringContent(JsonConvert.SerializeObject(new
                        {
                            ClientID = clientId,
                            BookID = bookId
                        }), Encoding.UTF8, "application/json"));
                }
            }
            await BindPendingReturns();
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