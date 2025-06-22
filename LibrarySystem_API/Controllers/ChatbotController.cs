using System;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
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
                var response = await WebServiceClient.GetChatbotResponseAsync(
                    request.Message, request.ChatId, request.ClientId
                );

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


        [HttpPost]
        [Route("newchat")]
        public HttpResponseMessage CreateNewChat(int clientId)
        {
            try
            {
                Guid chatId = WebServiceClient.CreateNewChat(clientId);
                return Request.CreateResponse(HttpStatusCode.OK, new { ChatId = chatId });
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("chats")]
        public HttpResponseMessage GetChatSessions(int clientId)
        {
            try
            {
                var chats = WebServiceClient.GetChatSessions(clientId);
                return Request.CreateResponse(HttpStatusCode.OK, chats);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpDelete]
        [Route("delete/{chatId}")]
        public IHttpActionResult DeleteChat(Guid chatId)
        {
            try
            {
                var _client = WebServiceClient.Instance;
                var result = _client.DeleteChat(chatId);

                return Ok(new
                {
                    Success = true,
                    Message = "Book deleted successfully!"
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("history/{chatId}")]
        public HttpResponseMessage GetChatHistory(Guid chatId)
        {
            try
            {
                var _client = WebServiceClient.Instance;
                var history = _client.GetChatHistory(chatId.ToString());
                return Request.CreateResponse(HttpStatusCode.OK, history);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }

    public class ChatRequest
    {
        public string SessionId { get; set; }
        public Guid ChatId { get; set; }
        public string Message { get; set; }
        public int ClientId { get; set; }
    }

    public class NewChatRequest
    {
        
    }
}