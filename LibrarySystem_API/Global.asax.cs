using LibrarySystem_API.App_Start;
using System.Web.Http;

namespace LibrarySystem_API
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}