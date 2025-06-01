using LibrarySystem_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace LibrarySystem_API.Controllers
{
    [RoutePrefix("api/reports")]
    public class ReportController : ApiController
    {
        [HttpGet]
        [Route("report")]
        public IHttpActionResult GetBorrowReport(int? userId = null, string bookId = null,
                                       DateTime? fromDate = null, DateTime? toDate = null,
                                       bool dueSoon = false, bool overdue = false)
        {
            var _client = WebServiceClient.Instance;
            return Ok(_client.GetBorrowReport(userId, bookId, fromDate?.Date, toDate?.Date, dueSoon, overdue));
        }
    }
}
