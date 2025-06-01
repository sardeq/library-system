using LibrarySystem_API.Models;
using System;
using System.Web.Http;

namespace LibrarySystem_API.Controllers
{
    [RoutePrefix("api/login")]
    public class LoginController : ApiController
    {

        [Route("initiateLogin")]
        [HttpPost]
        public IHttpActionResult Login(LoginRequest request)
        {
            var _webService = WebServiceClient.Instance;
            try
            {
                var wsResult = _webService.Login(request.Username, request.Password);

                var apiResult = new LoginResult
                {
                    Success = wsResult.Success,
                    Message = wsResult.Message,
                    ClientID = wsResult.ClientID,
                    UserType = wsResult.UserType
                };

                return Ok(apiResult);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }


    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? ClientID { get; set; }
        public string UserType { get; set; }
    }
}