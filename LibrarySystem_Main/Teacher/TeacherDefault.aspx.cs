using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LibrarySystem_Main
{
    public partial class TeacherDefault : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserType"] == null || Convert.ToInt32(Session["UserType"]) != 2)
            {
                Response.Redirect("~/AccessDenied.aspx");
            }
        }
    }
}