using System.Web.Http;
using LibrarySystem_Shared.Models;

namespace LibrarySystem_API.Controllers
{
    public class TestController : ApiController
    {
        [HttpPost]
        [Route("api/test/review")]
        public IHttpActionResult TestReview([FromBody] ReviewRequest request)
        {
            return Ok(new ReviewResult
            {
                Success = true,
                Sentiment = "positive",
                Error = null
            });
        }

        public class ReviewRequest
        {
            public int UserID { get; set; }
            public string BookID { get; set; }
            public string ReviewText { get; set; }
        }
    }
}