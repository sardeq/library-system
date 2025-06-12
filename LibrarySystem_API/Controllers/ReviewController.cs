using LibrarySystem_API.Models;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace LibrarySystem_API.Controllers
{
    [RoutePrefix("api/review")]
    public class ReviewController : ApiController
    {
        [HttpPost]
        [Route("submit")]
        public async Task<IHttpActionResult> SubmitReview([FromBody] ReviewRequest request)
        {
            try
            {
                var response = await WebServiceClient.ProcessReviewAsync(
                    request.UserID,
                    request.BookID,
                    request.ReviewText
                );

                if (!response.Success)
                    return Content(HttpStatusCode.BadRequest, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        public class ReviewRequest
        {
            public int UserID { get; set; }
            public string BookID { get; set; }
            public string ReviewText { get; set; }
        }
    }
}