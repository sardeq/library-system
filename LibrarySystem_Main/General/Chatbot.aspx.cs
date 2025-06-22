using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LibrarySystem_Main.General
{
    public partial class Chatbot : BasePage
    {
        private Guid CurrentChatId
        {
            get => (Guid)(ViewState["CurrentChatId"] ?? Guid.Empty);
            set => ViewState["CurrentChatId"] = value;
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
            if (!IsPostBack)
            {
                InitializeChatSession();
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
            CurrentChatId = CreateNewChat();
            var initialMessage = new ChatMessage
            {
                Sender = "Library Bot",
                Text = "Hello! How can I help you with library services today?",
                CssClass = "bot-message"
            };
            SaveMessageToHistory(initialMessage);
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
                var response = APIClient.Instance.GetAsync(
                    $"api/chatbot/chats?clientId={CurrentUser.ClientID}"
                ).Result;

                if (response.IsSuccessStatusCode)
                {
                    var chats = JsonConvert.DeserializeObject<List<ChatInfo>>(
                        response.Content.ReadAsStringAsync().Result
                    );
                    rptChatSessions.DataSource = chats;
                    rptChatSessions.DataBind();
                }
            }
            catch
            {
                // Handle error
            }
        }

        protected async void btnSend_Click(object sender, EventArgs e)
        {
            var messageText = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(messageText)) return;

            SaveMessageToHistory(new ChatMessage
            {
                Sender = "You",
                Text = messageText,
                CssClass = "user-message",
                Timestamp = DateTime.Now.ToString("hh:mm tt")
            });

            txtMessage.Text = "";
            RenderChatHistory();
            ScriptManager.RegisterStartupScript(this, GetType(), "showTyping",
                "showTypingIndicator();", true);
            ScriptManager.RegisterStartupScript(this, GetType(), "focusInput",
                "focusInput();", true);

            try
            {
                // Add API call to get bot response
                var requestData = new
                {
                    Message = messageText,
                    ChatId = CurrentChatId,
                    ClientId = CurrentUser.ClientID
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(requestData),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await APIClient.Instance.PostAsync(
                    "api/chatbot/respond", content
                );

                string botResponseText = "";
                if (response.IsSuccessStatusCode)
                {
                    botResponseText = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    botResponseText = "Error: " + response.ReasonPhrase;
                }

                SaveMessageToHistory(new ChatMessage
                {
                    Sender = "Library Bot",
                    Text = botResponseText,
                    CssClass = "bot-message",
                    Timestamp = DateTime.Now.ToString("hh:mm tt")
                });
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
                var requestData = new {
                    ClientId = CurrentUser.ClientID
                };
                var content = new StringContent(
                    JsonConvert.SerializeObject(requestData),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = APIClient.Instance.PostAsync("api/chatbot/newchat", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeAnonymousType(
                        response.Content.ReadAsStringAsync().Result,
                        new { ChatId = Guid.Empty } 
                    );
                    CurrentChatId = result.ChatId;
                }
            }
            catch
            {
                // later
            }

        }

        protected async void rptChatSessions_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "SelectChat")
            {
                if (Guid.TryParse(e.CommandArgument.ToString(), out Guid chatId))
                {
                    CurrentChatId = chatId;
                }
                else
                {
                    CurrentChatId = CreateNewChat();
                }
                RenderChatHistory();
            }
            else if (e.CommandName == "DeleteChat")
            {
                if (Guid.TryParse(e.CommandArgument.ToString(), out Guid chatId))
                {
                    var response = await APIClient.Instance.DeleteAsync(
                        $"api/chatbot/delete/{chatId}"
                    );

                    if (response.IsSuccessStatusCode)
                    {
                        if (chatId == CurrentChatId)
                        {
                            CurrentChatId = CreateNewChat();
                        }

                        if (ChatHistories.ContainsKey(chatId))
                        {
                            ChatHistories.Remove(chatId);
                        }
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
        }

        private void RenderChatHistory()
        {
            var messages = LoadMessagesFromDatabase(CurrentChatId);
            var sb = new StringBuilder();

            foreach (var msg in messages)
            {
                string containerClass = msg.CssClass == "user-message"
                    ? "chat-message-container user-message-container"
                    : "chat-message-container bot-message-container";

                sb.Append($@"<div class=""{containerClass}"">");
                sb.Append($@"<div class=""chat-message {msg.CssClass}"">");
                sb.Append($@"<div>{Server.HtmlEncode(msg.Text ?? "").Replace("\n", "<br />")}</div>");

                if (!string.IsNullOrEmpty(msg.Timestamp))
                {
                    sb.Append($@"<div class=""message-time"">{msg.Timestamp}</div>");
                }

                sb.Append("</div></div>");
            }

            litChatHistory.Text = sb.ToString();
            litActiveChatTitle.Text = GetActiveChatTitle();

            ScriptManager.RegisterStartupScript(this, GetType(), "scrollChat",
                "scrollToBottom();", true);
        }

        private string GetActiveChatTitle()
        {
            foreach (RepeaterItem item in rptChatSessions.Items)
            {
                if (item.ItemType == ListItemType.Item ||
                    item.ItemType == ListItemType.AlternatingItem)
                {
                    object chatIdObj = DataBinder.Eval(item.DataItem, "ChatId");
                    if (chatIdObj != null)
                    {
                        string chatId = chatIdObj.ToString();
                        if (Guid.TryParse(chatId, out Guid parsedId) &&
                            parsedId == CurrentChatId)
                        {
                            object titleObj = DataBinder.Eval(item.DataItem, "Title");
                            return titleObj?.ToString() ?? "New Chat";
                        }
                    }
                }
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