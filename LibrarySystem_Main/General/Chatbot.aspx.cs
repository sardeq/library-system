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
            else
            {
                RenderChatHistory();
            }
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
                CssClass = "user-message"
            });

            txtMessage.Text = "";
            RenderChatHistory();

            try
            {
                var requestData = new
                {
                    ChatId = CurrentChatId,
                    Message = messageText,
                    ClientId = CurrentUser.ClientID
                };
                var content = new StringContent(
                    JsonConvert.SerializeObject(requestData),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await APIClient.Instance.PostAsync("api/chatbot/respond", content);

                string botResponseText;
                string cssClass = "bot-message";

                if (response.IsSuccessStatusCode)
                {
                    botResponseText = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    botResponseText = $"Error: {response.StatusCode}";
                    cssClass = "error-message";
                }

                SaveMessageToHistory(new ChatMessage
                {
                    Sender = "Library Bot",
                    Text = botResponseText,
                    CssClass = cssClass
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

                LoadChatSessions();
                RenderChatHistory();
            }
            catch
            {
                // later
            }

        }

        protected void rptChatSessions_ItemCommand(object source, RepeaterCommandEventArgs e)
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

        private void RenderChatHistory()
        {
            if (!ChatHistories.ContainsKey(CurrentChatId)) return;

            var sb = new StringBuilder();
            foreach (var msg in ChatHistories[CurrentChatId])
            {
                sb.AppendFormat(
                    @"<div class=""chat-message {0}""><strong>{1}:</strong> {2}</div>",
                    msg.CssClass,
                    Server.HtmlEncode(msg.Sender),
                    Server.HtmlEncode(msg.Text ?? "").Replace("\n", "<br />")
                );
            }
            litChatHistory.Text = sb.ToString();

            ScriptManager.RegisterStartupScript(this, GetType(), "scrollChat",
                "var container = document.getElementById('chatScrollContainer');" +
                "container.scrollTop = container.scrollHeight;", true);
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