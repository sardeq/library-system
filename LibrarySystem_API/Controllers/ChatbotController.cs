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
        public async Task<IHttpActionResult> Respond([FromBody] ChatRequest request)
        {
            try
            {
                var response = await WebServiceClient.GetChatbotResponseAsync(
                    request.Message, request.ChatId, request.ClientId,
                    request.ImageBase64, request.ImageMimeType
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
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
                bool wasDeleted = WebServiceClient.Instance.DeleteChat(chatId);

                if (wasDeleted)
                {
                    return Ok(new { Success = true, Message = "Chat deleted successfully." });
                }
                else
                {
                    return Content(HttpStatusCode.InternalServerError, new { Success = false, Message = "Failed to delete the chat in the backend service." });
                }
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

        [HttpPost]
        [Route("addmessage")]
        public IHttpActionResult AddMessage([FromBody] AddMessageRequest request)
        {
            try
            {
                WebServiceClient.Instance.AddMessageToChat(request.ChatId, request.Role, request.Content);
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }

    public class AddMessageRequest
    {
        public Guid ChatId { get; set; }
        public string Role { get; set; }
        public string Content { get; set; }
    }

    public class ChatRequest
    {
        public string SessionId { get; set; }
        public string ImageBase64 { get; set; }
        public string ImageMimeType { get; set; }
        public Guid ChatId { get; set; }
        public string Message { get; set; }
        public int ClientId { get; set; }
    }

    public class NewChatRequest
    {
        
    }
}