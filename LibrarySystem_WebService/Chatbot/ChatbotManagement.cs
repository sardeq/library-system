using Newtonsoft.Json;
using SchoolSystem.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace LibrarySystem_WebService.Chatbot
{
    public class ChatbotManagement
    {
        private static readonly ConcurrentDictionary<string, List<Message>> _conversationHistory =
            new ConcurrentDictionary<string, List<Message>>();

        private static readonly object _historyLock = new object();

        public class Message
        {
            public string Role { get; set; }
            public string Content { get; set; }
        }

        public static async Task<string> GetChatbotResponse(string message, string sessionId)
        {
            var apiKey = ConfigurationManager.AppSettings["OpenRouter_ApiKey"]?.Trim();
            var logPath = HostingEnvironment.MapPath("~/App_Data/chatbot_log.txt");

            // Get or create conversation history
            var history = _conversationHistory.GetOrAdd(sessionId, new List<Message>());

            // Add user message to history
            lock (_historyLock)
            {
                history.Add(new Message { Role = "user", Content = message });
            }

            // Check for book-related queries
            string bookResults = "";
            if (IsBookRelated(message))
            {
                try
                {
                    var dbService = new DatabaseService();
                    DataTable books;

                    if (IsGeneralBookInquiry(message))
                    {
                        books = dbService.SearchBooks("", true); // General search
                        bookResults = "Here's a selection of books from our library:\n";
                    }
                    else
                    {
                        books = dbService.SearchBooks(message); // Specific search
                        bookResults = "I found these matching books:\n";
                    }

                    bookResults += FormatBookResults(books);
                }
                catch (Exception ex)
                {
                    File.AppendAllText(logPath, $"{DateTime.UtcNow} - DB ERROR: {ex}\n\n");
                    bookResults = "Error accessing book database";
                }
            }

            // Build enhanced prompt
            var prompt = BuildPrompt(history, bookResults);

            using (var client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", apiKey);

                    client.DefaultRequestHeaders.Add("X-Title", "Library Chatbot");
                    client.DefaultRequestHeaders.Add("User-Agent", "LibrarySystem/1.0");

                    var requestBody = new
                    {
                        //model = "google/gemma-3n-e4b-it:free",
                        //model = "deepseek/deepseek-chat-v3-0324:free",
                        model = "meta-llama/llama-4-maverick:free",
                        messages = new[] { new { role = "user", content = prompt } },
                        max_tokens = 2000,
                        temperature = 0.8
                    };

                    File.AppendAllText(logPath, $"{DateTime.UtcNow} - Request:\n{JsonConvert.SerializeObject(requestBody)}\n\n");

                    var response = await client.PostAsync(
                        "https://openrouter.ai/api/v1/chat/completions",
                        new StringContent(
                            JsonConvert.SerializeObject(requestBody),
                            Encoding.UTF8,
                            "application/json")
                    );

                    var responseContent = await response.Content.ReadAsStringAsync();

                    File.AppendAllText(logPath,
                        $"{DateTime.UtcNow} - Status: {response.StatusCode}\n" +
                        $"Response: {responseContent}\n\n");

                    string assistantResponse = "";
                    if (response.IsSuccessStatusCode)
                    {
                        var result = JsonConvert.DeserializeObject<OpenRouterResponse>(responseContent);
                        assistantResponse = result?.choices?[0]?.message?.content?.Trim() ?? "I couldn't process that request.";
                    }
                    else
                    {
                        assistantResponse = $"Error: API returned {response.StatusCode}";
                    }

                    // Add assistant response to history
                    lock (_historyLock)
                    {
                        history.Add(new Message { Role = "assistant", Content = assistantResponse });
                    }

                    return assistantResponse;
                }
                catch (Exception ex)
                {
                    File.AppendAllText(logPath, $"{DateTime.UtcNow} - ERROR: {ex}\n\n");
                    return "Sorry, I'm experiencing technical difficulties.";
                }
            }
        }

        private static bool IsGeneralBookInquiry(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return false;

            // Convert message to lowercase for easier matching
            var lowerMessage = message.ToLower();

            var generalPhrases = new[] {
                "what books", "list books", "show books", "do you have",
                "what kind of books", "available books", "library collection",
                "books do you have", "all the books", "any books", "give me all the books",

                "in the catalog", "in the database", "what do you have", "just curious",
                "can i check it out", "don't worry about the genre"
            };

            // Check if the message contains any of the general inquiry phrases
            return generalPhrases.Any(phrase => lowerMessage.Contains(phrase));
        }

        private static bool IsBookRelated(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return false;

            var keywords = new[] { "book", "books", "author", "title", "recommend" };
            return keywords.Any(k =>
                message.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static string FormatBookResults(DataTable books)
        {
            if (books.Rows.Count == 0)
                return "No matching books found.\n";

            StringBuilder sb = new StringBuilder();
            foreach (DataRow row in books.Rows)
            {
                sb.AppendLine($"- \"{row["Title"]}\" by {row["Author"]} " +
                             $"({row["BooksAvailable"]} available)");
            }
            return sb.ToString();
        }

        private static string BuildPrompt(List<Message> history, string bookData)
        {
            var prompt = new StringBuilder();
            prompt.AppendLine("You are a helpful library assistant. Remember:");
            prompt.AppendLine("- Maintain conversation context");
            prompt.AppendLine("- Use book database when available");
            prompt.AppendLine("- Keep responses concise and library-focused");
            prompt.AppendLine("- When asked about books from our database, just give some answers first then ask about specifics if needed");
            prompt.AppendLine("- Dont ask for specifics immediately, give a few books from the database then ask if the user wants anything specific.");
            prompt.AppendLine("- When asked about a book always assume the user means in the database first, unless the user specifies");

            // Add conversation history (last 4 exchanges)
            int startIndex = Math.Max(0, history.Count - 8);
            for (int i = startIndex; i < history.Count; i++)
            {
                prompt.AppendLine($"{history[i].Role}: {history[i].Content}");
            }

            // Append book data if available
            if (!string.IsNullOrEmpty(bookData))
            {
                prompt.AppendLine($"system: {bookData}");
            }

            prompt.Append("Assistant: ");
            return prompt.ToString();
        }

        // Clean up old sessions (call periodically)
        public static void CleanOldSessions(TimeSpan maxAge)
        {
            var cutoff = DateTime.UtcNow - maxAge;
            var oldSessions = _conversationHistory.Where(kvp =>
                kvp.Value.LastOrDefault()?.Role == "assistant" &&
                kvp.Value.Last().Content.Contains(cutoff.ToString("O"))).ToList();

            foreach (var session in oldSessions)
            {
                _conversationHistory.TryRemove(session.Key, out _);
            }
        }
    }



    public class Message
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }

    public class OpenRouterResponse
    {
        public List<OpenRouterChoice> choices { get; set; }
    }

    public class OpenRouterChoice
    {
        public OpenRouterMessage message { get; set; }
    }

    public class OpenRouterMessage
    {
        public string content { get; set; }
    }
}