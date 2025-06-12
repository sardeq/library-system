
using System.Web.Http;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;
using System;
using LibrarySystem_API.Models;

namespace LibrarySystem_API.Controllers
{
    [RoutePrefix("api/analysis")]
    public class AnalysisController : ApiController
    {

        [HttpGet]
        [Route("sentiment-summary")]
        public IHttpActionResult GetSentimentSummary()
        {
            try
            {
                var _client = WebServiceClient.Instance;

                var summary = _client.GetReviewSentimentSummary();
                return Ok(summary);
            }
            catch (InvalidOperationException ex)
            {

                return Content(HttpStatusCode.InternalServerError, "Server configuration error: " + ex.Message);
            }
            catch (System.Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("reviews-by-age")]
        public IHttpActionResult GetReviewsByAge()
        {
            try
            {
                var _client = WebServiceClient.Instance;

                var ageData = _client.GetReviewsByAgeGroup();
                return Ok(ageData);
            }
            catch (InvalidOperationException ex)
            {
                return Content(HttpStatusCode.InternalServerError, "Server configuration error: " + ex.Message);
            }
            catch (System.Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("reviews-by-borrowed-count")]
        public IHttpActionResult GetReviewsByBorrowedCount()
        {
            try
            {
                var _client = WebServiceClient.Instance;

                var borrowedData = _client.GetReviewsByBorrowedCount();
                return Ok(borrowedData);
            }
            catch (InvalidOperationException ex)
            {
                return Content(HttpStatusCode.InternalServerError, "Server configuration error: " + ex.Message);
            }
            catch (System.Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
