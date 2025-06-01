using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using LibrarySystem_Main.Models;
using Newtonsoft.Json;

namespace LibrarySystem_Main.Admin
{
    public partial class Reports : BasePage
    {
        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                await LoadFilters();
            }
        }

        protected void gvReport_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var borrowInfo = (BorrowInfo)e.Row.DataItem;
                var lblUser = (Label)e.Row.FindControl("lblClientName");

                if (lblUser == null) return;

                lblUser.CssClass = "";

                if (!borrowInfo.PendingConfirmation)
                {
                    if (borrowInfo.ReturnDate < DateTime.Now && !borrowInfo.Returned)
                    {
                        lblUser.CssClass = "overdue-user";
                    }
                    else if (borrowInfo.ReturnDate?.Date == DateTime.Now.Date.AddDays(1) && !borrowInfo.Returned)
                    {
                        lblUser.CssClass = "due-soon-user";
                    }
                }
            }
        }

        private async Task LoadFilters()
        {
            var usersResponse = await APIClient.Instance.GetAsync("api/user");
            if (usersResponse.IsSuccessStatusCode)
            {
                var users = JsonConvert.DeserializeObject<List<User>>(
                    await usersResponse.Content.ReadAsStringAsync());
                ddlUsers.DataValueField = "ClientID";
                ddlUsers.DataTextField = "ClientName";
                ddlUsers.DataSource = users;
                ddlUsers.DataBind();
            }

            var booksResponse = await APIClient.Instance.GetAsync("api/books");
            if (booksResponse.IsSuccessStatusCode)
            {
                var books = JsonConvert.DeserializeObject<List<Book>>(
                    await booksResponse.Content.ReadAsStringAsync());
                ddlBooks.DataValueField = "BookID";
                ddlBooks.DataTextField = "Title";
                ddlBooks.DataSource = books;
                ddlBooks.DataBind();
            }
        }

        protected async void btnView_Click(object sender, EventArgs e)
        {
            var filters = new
            {
                userId = string.IsNullOrEmpty(ddlUsers.SelectedValue) ?
                        (int?)null : Convert.ToInt32(ddlUsers.SelectedValue),
                bookId = ddlBooks.SelectedValue,
                fromDate = string.IsNullOrEmpty(txtFromDate.Text) ?
                         (DateTime?)null : DateTime.Parse(txtFromDate.Text),
                toDate = string.IsNullOrEmpty(txtToDate.Text) ?
                       (DateTime?)null : DateTime.Parse(txtToDate.Text),
                dueSoon = chkDueSoon.Checked,
                overdue = chkOverdue.Checked
            };

            var response = await APIClient.Instance.GetAsync(
                $"api/reports/report?" +
                $"userId={filters.userId}&" +
                $"bookId={filters.bookId}&" +
                $"fromDate={filters.fromDate?.ToString("yyyy-MM-dd")}&" +
                $"toDate={filters.toDate?.ToString("yyyy-MM-dd")}&" +
                $"dueSoon={filters.dueSoon}&" +
                $"overdue={filters.overdue}");

            if (response.IsSuccessStatusCode)
            {
                var data = JsonConvert.DeserializeObject<List<BorrowInfo>>(
                    await response.Content.ReadAsStringAsync());
                gvReport.DataSource = data;
                gvReport.DataBind();
            }
        }
    }
}