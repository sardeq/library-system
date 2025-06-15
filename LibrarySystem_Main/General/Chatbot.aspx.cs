using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

namespace LibrarySystem_Main.General
{
    public partial class Chatbot : BasePage
    {
        private List<ChatMessage> ChatHistory
        {
            get { return (List<ChatMessage>)Session["ChatHistory"] ?? new List<ChatMessage>(); }
            set { Session["ChatHistory"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var initialHistory = new List<ChatMessage>
                {
                    new ChatMessage { Sender = "Library Bot", Text = "Hello! How can I help you with library services today?", CssClass = "bot-message" }
                };
                ChatHistory = initialHistory;
            }
            RenderChatHistory();
        }

        protected async void btnSend_Click(object sender, EventArgs e)
        {
            var messageText = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(messageText)) return;

            var currentHistory = ChatHistory;
            currentHistory.Add(new ChatMessage { Sender = "You", Text = messageText, CssClass = "user-message" });
            ChatHistory = currentHistory;

            txtMessage.Text = "";
            RenderChatHistory();

            try
            {
                var requestData = new { Message = messageText };
                var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

                var response = await APIClient.Instance.PostAsync("api/chatbot/respond", content);

                string botResponseText;
                string cssClass = "bot-message";

                if (response.IsSuccessStatusCode)
                {
                    botResponseText = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(botResponseText))
                    {
                        botResponseText = "I didn't get a response. Please try again.";
                    }
                }
                else
                {
                    botResponseText = $"API Error: Could not get a response. (Status: {response.StatusCode})";
                    cssClass = "error-message";
                }

                currentHistory = ChatHistory;
                currentHistory.Add(new ChatMessage { Sender = "Library Bot", Text = botResponseText, CssClass = cssClass });
                ChatHistory = currentHistory;
            }
            catch (Exception ex)
            {
                currentHistory = ChatHistory; // was (var currentHistory = ChatHistory;) in case an error happens
                currentHistory.Add(new ChatMessage { Sender = "System", Text = $"A technical error occurred: {ex.Message}", CssClass = "error-message" });
                ChatHistory = currentHistory;
            }

            RenderChatHistory();
        }

        private void RenderChatHistory()
        {
            var sb = new StringBuilder();
            foreach (var msg in ChatHistory)
            {
                sb.AppendFormat(
                    @"<div class=""chat-message {0}""><strong>{1}:</strong> {2}</div>",
                    msg.CssClass,
                    Server.HtmlEncode(msg.Sender),
                    Server.HtmlEncode(msg.Text ?? "")
                        .Replace("\n", "<br />")
                );
            }
            litChatHistory.Text = sb.ToString();
        }
    }
}