using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using LibrarySystem_API.Models;

namespace LibrarySystem_API.Controllers
{
    [RoutePrefix("api/chatbot")]
    public class ChatbotController : ApiController
    {
        [HttpPost]
        [Route("respond")]
        public async Task<HttpResponseMessage> Respond([FromBody] ChatRequest request)
        {
            try
            {
                var response = await WebServiceClient.GetChatbotResponseAsync(request.Message);

                return new HttpResponseMessage
                {
                    Content = new StringContent(response, Encoding.UTF8, "text/plain")
                };
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}