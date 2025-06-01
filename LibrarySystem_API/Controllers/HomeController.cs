using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace LibrarySystem_API.Controllers
{
    public class HomeController : ApiController
    {
        [Route("")]
        [HttpGet]
        public IHttpActionResult GetRoot()
        {
            return Ok("API");
        }
    }
}
