using LibrarySystem_Shared.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LibrarySystem_Main.General
{
    public partial class Chatbot : BasePage
    {
        private List<ChatInfo> _chatSessions;

        private Guid CurrentChatId
        {
            get => Session["CurrentChatId"] == null ? Guid.Empty : (Guid)Session["CurrentChatId"];
            set => Session["CurrentChatId"] = value;
        }

        private Dictionary<Guid, List<ChatMessage>> ChatHistories
        {
            get
            {
                if (Session["ChatHistories"] == null)
                    Session["ChatHistories"] = new Dictionary<Guid, List<ChatMessage>>();
                return (Dictionary<Guid, List<ChatMessage>>)Session["ChatHistories"];
            }
            set => Session["ChatHistories"] = value;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Form.Enctype = "multipart/form-data";

            if (!IsPostBack)
            {
                if (CurrentChatId == Guid.Empty)
                {
                    InitializeChatSession();
                }
                LoadChatSessions();
            }
        }



        protected void Page_PreRender(object sender, EventArgs e)
        {
            RenderChatHistory();
        }


        public bool IsActiveChat(object chatIdObj)
        {
            if (chatIdObj == null) return false;
            return Guid.TryParse(chatIdObj.ToString(), out Guid chatId) &&
                   chatId == CurrentChatId;
        }

        private void InitializeChatSession()
        {
            if (CurrentChatId == Guid.Empty)
            {
                CurrentChatId = CreateNewChat();
            }

            if (!ChatHistories.ContainsKey(CurrentChatId) || ChatHistories[CurrentChatId].Count == 0)
            {
                var initialMessage = new ChatMessage
                {
                    Sender = "Library Bot",
                    Text = "Hello! How can I help you with library services today?",
                    CssClass = "bot-message"
                };

                var requestData = new
                {
                    ChatId = CurrentChatId,
                    Role = "assistant",
                    Content = initialMessage.Text
                };
                var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
                var response = APIClient.Instance.PostAsync("api/chatbot/addmessage", content).Result;

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to save initial message: " + response.ReasonPhrase);
                }

                SaveMessageToHistory(initialMessage);
            }
        }

        private Guid CreateNewChat()
        {
            var response = APIClient.Instance.PostAsync(
                $"api/chatbot/newchat?clientId={CurrentUser.ClientID}",
                null
            ).Result;
            
            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeAnonymousType(
                    response.Content.ReadAsStringAsync().Result,
                    new { ChatId = Guid.Empty }
                );
                return result.ChatId;
            }
            else
            {
                var errorContent = response.Content.ReadAsStringAsync().Result;
                throw new Exception($"Chat creation failed: {response.StatusCode} - {errorContent}");
            }
        }


        private void LoadChatSessions()
        {
            try
            {
                var response = APIClient.Instance.GetAsync($"api/chatbot/chats?clientId={CurrentUser.ClientID}").Result;
                if (response.IsSuccessStatusCode)
                {
                    var chats = JsonConvert.DeserializeObject<List<ChatInfo>>(response.Content.ReadAsStringAsync().Result);
                    _chatSessions = chats; 
                    rptChatSessions.DataSource = _chatSessions;
                    rptChatSessions.DataBind();
                }
            }
            catch { /* Handle error */ }
        }

        protected async void btnSend_Click(object sender, EventArgs e)
        {
            var messageText = txtMessage.Text.Trim();
            string imageBase64 = null;
            string imageMimeType = null;

            if (fileUploadImage.HasFile && !messageText.StartsWith("/image "))
            {
                using (var stream = new MemoryStream())
                {
                    fileUploadImage.PostedFile.InputStream.CopyTo(stream);
                    imageBase64 = Convert.ToBase64String(stream.ToArray());
                    imageMimeType = fileUploadImage.PostedFile.ContentType;
                }
            }

            if (string.IsNullOrEmpty(messageText) && string.IsNullOrEmpty(imageBase64)) return;

            SaveMessageToHistory(new ChatMessage
            {
                Sender = "You",
                Text = !string.IsNullOrEmpty(messageText) ? messageText : "[Image Attachment]",
                CssClass = "user-message",
                Timestamp = DateTime.Now.ToString("hh:mm tt")
            });

            txtMessage.Text = "";
            //fileUploadImage.Attributes.Clear();
            RenderChatHistory();
            ScriptManager.RegisterStartupScript(this, GetType(), "showTyping", "showTypingIndicator();", true);
            ScriptManager.RegisterStartupScript(this, GetType(), "focusInput", "focusInput();", true);

            try
            {
                var requestData = new
                {
                    Message = messageText,
                    ImageBase64 = imageBase64,
                    ImageMimeType = imageMimeType,
                    ChatId = CurrentChatId,
                    ClientId = CurrentUser.ClientID
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(requestData),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await APIClient.Instance.PostAsync("api/chatbot/respond", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var botResponse = JsonConvert.DeserializeObject<ChatbotResponse>(responseContent);
                    SaveMessageToHistory(new ChatMessage
                    {
                        Sender = "Library Bot",
                        Text = botResponse.Message,
                        ImageData = botResponse.ImageData,
                        CssClass = "bot-message",
                        Timestamp = DateTime.Now.ToString("hh:mm tt")
                    });
                }
                else
                {
                    SaveMessageToHistory(new ChatMessage
                    {
                        Sender = "System",
                        Text = "Error: " + response.ReasonPhrase,
                        CssClass = "error-message"
                    });
                }
            }
            catch (Exception ex)
            {
                SaveMessageToHistory(new ChatMessage
                {
                    Sender = "System",
                    Text = $"Error: {ex.Message}",
                    CssClass = "error-message"
                });
            }

            RenderChatHistory();
        }

        protected void btnNewChat_Click(object sender, EventArgs e)
        {
            try
            {
                Guid newChatId = CreateNewChat();
                if (newChatId != Guid.Empty)
                {
                    CurrentChatId = newChatId;

                    var initialMessage = "Hello! How can I help you with library services today?";
                    var requestData = new
                    {
                        ChatId = CurrentChatId,
                        Role = "assistant",
                        Content = initialMessage
                    };
                    var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
                    var response = APIClient.Instance.PostAsync("api/chatbot/addmessage", content).Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        System.Diagnostics.Debug.WriteLine("Failed to save initial message for new chat.");
                    }

                    LoadChatSessions();
                    RenderChatHistory();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("New chat creation failed: " + ex.Message);
            }
        }

        protected async void rptChatSessions_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (Guid.TryParse(e.CommandArgument.ToString(), out Guid chatId))
            {
                if (e.CommandName == "SelectChat")
                {
                    CurrentChatId = chatId;
                    LoadChatSessions();
                    RenderChatHistory();
                }
                else if (e.CommandName == "DeleteChat")
                {
                    //var response = await APIClient.Instance.DeleteAsync($"chatbot/delete/{chatId}");

                    var response = await APIClient.Instance.PostAsync(
                        $"api/chatbot/delete/{chatId}",
                        null
                    );

                    if (response.IsSuccessStatusCode)
                    {
                        if (chatId == CurrentChatId)
                        {
                            LoadChatSessions();
                            CurrentChatId = (_chatSessions != null && _chatSessions.Any())
                                            ? Guid.Parse(_chatSessions.First().ChatId)
                                            : CreateNewChat();
                        }
                        LoadChatSessions();
                        RenderChatHistory();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to delete chat {chatId}. Status: {response.StatusCode}");
                    }
                }
            }
        }

        private void SaveMessageToHistory(ChatMessage message)
        {
            if (!ChatHistories.ContainsKey(CurrentChatId))
            {
                ChatHistories[CurrentChatId] = new List<ChatMessage>();
            }
            ChatHistories[CurrentChatId].Add(message);
        }

        private void ClearChatHistory()
        {
            if (ChatHistories.ContainsKey(CurrentChatId))
            {
                ChatHistories[CurrentChatId].Clear();
            }
        }

        private List<ChatMessage> LoadMessagesFromDatabase(Guid chatId)
        {
            var messages = new List<ChatMessage>();
            try
            {
                var response = APIClient.Instance.GetAsync($"api/chatbot/history/{chatId}").Result;
                if (response.IsSuccessStatusCode)
                {
                    var dbMessages = JsonConvert.DeserializeObject<List<DbMessage>>(
                        response.Content.ReadAsStringAsync().Result
                    );

                    foreach (var msg in dbMessages)
                    {
                        messages.Add(new ChatMessage
                        {
                            Sender = msg.Role == "user" ? "You" : "Library Bot",
                            Text = msg.Content,
                            ImageData = msg.ImageData,
                            CssClass = msg.Role == "user" ? "user-message" : "bot-message"
                        });
                    }
                }
            }
            catch { /* Handle error */ }
            return messages;
        }

        public class DbMessage
        {
            public string Role { get; set; }
            public string Content { get; set; }
            public string ImageData { get; set; }
        }

        private void RenderChatHistory()
        {
            var messages = LoadMessagesFromDatabase(CurrentChatId);

            // Show placeholder if there are no messages
            if (messages.Count == 0)
            {
                phEmptyChat.Visible = true;
                litChatHistory.Text = "";
            }
            else
            {
                phEmptyChat.Visible = false;
                var sb = new StringBuilder();

                foreach (var msg in messages)
                {
                    string containerClass = msg.CssClass == "user-message"
                        ? "chat-message-container user-message-container"
                        : "chat-message-container bot-message-container";

                    sb.Append($@"<div class='{containerClass}'>");
                    sb.Append($@"<div class='chat-message {msg.CssClass}'>");
                    sb.Append($@"<div>{Server.HtmlEncode(msg.Text ?? "").Replace("\n", "<br />")}</div>");

                    if (!string.IsNullOrEmpty(msg.ImageData))
                    {
                        sb.Append($@"<div class='chat-image-preview mt-2'>
                                <img src='{msg.ImageData}' alt='Attached image' style='max-width: 200px;'/>
                             </div>");
                    }

                    if (!string.IsNullOrEmpty(msg.Timestamp))
                    {
                        sb.Append($@"<div class='message-time'>{msg.Timestamp}</div>");
                    }

                    sb.Append("</div></div>");
                }
                litChatHistory.Text = sb.ToString();
            }

            litActiveChatTitle.Text = GetActiveChatTitle();

            ScriptManager.RegisterStartupScript(this, GetType(), "scrollChat",
                "scrollToBottom();", true);
        }

        private string GetActiveChatTitle()
        {
            if (_chatSessions != null)
            {
                var activeChat = _chatSessions.FirstOrDefault(c => Guid.Parse(c.ChatId) == CurrentChatId);
                return activeChat?.Title ?? "New Chat";
            }
            return "New Chat";
        }


        public string GetChatItemCss(object chatIdObj)
        {
            if (chatIdObj == null) return "";

            if (Guid.TryParse(chatIdObj.ToString(), out Guid chatId))
            {
                return chatId == CurrentChatId ? "active-chat" : "";
            }
            return "";
        }
    }
}