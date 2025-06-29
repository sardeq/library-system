using LibrarySystem_Shared.Models;
using Newtonsoft.Json;
using SchoolSystem.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using static LibrarySystem_WebService.WebService;

namespace LibrarySystem_WebService.Chatbot
{
    public class ChatbotManagement
    {

        public class Message
        {
            public string Role { get; set; }
            public string Content { get; set; }
        }

        public static async Task<string> GetChatbotResponse(string message, int clientId, Guid chatId, 
            string imageBase64, string imageMimeType)
        {
            var apiKey = ConfigurationManager.AppSettings["OpenRouter_ApiKey"]?.Trim();
            var logPath = HostingEnvironment.MapPath("~/App_Data/chatbot_log.txt");

            var history = GetChatHistory(chatId.ToString());

            string imageAnalysis = "";
            if (!string.IsNullOrEmpty(imageBase64))
            {
                imageAnalysis = await AnalyzeImageWithVisionModel(imageBase64, imageMimeType, logPath, apiKey);

                if (!string.IsNullOrEmpty(imageAnalysis) && !imageAnalysis.Contains("failed"))
                {
                    message = string.IsNullOrEmpty(message)
                        ? $"[Image uploaded: {imageAnalysis}]"
                        : $"[Image context: {imageAnalysis}] {message}";
                }
            }

            SaveMessageToDatabase(chatId.ToString(), "user", message, imageBase64, imageMimeType);
            history.Add(new Message { Role = "user", Content = message });

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

            var messagesList = BuildMessages(history, bookResults, imageAnalysis);

            using (var client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                    client.DefaultRequestHeaders.Add("X-Title", "Library Chatbot");
                    client.DefaultRequestHeaders.Add("User-Agent", "LibrarySystem/1.0");

                    var requestBody = new
                    {
                        model = "deepseek/deepseek-chat-v3-0324:free",
                        messages = messagesList,
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
                    File.AppendAllText(logPath, $"{DateTime.UtcNow} - Status: {response.StatusCode}\nResponse: {responseContent}\n\n");

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
                    
                    SaveMessageToDatabase(chatId.ToString(), "assistant", assistantResponse);
                    return assistantResponse;
                }
                catch (Exception ex)
                {
                    File.AppendAllText(logPath, $"{DateTime.UtcNow} - ERROR: {ex}\n\n");
                    return "Sorry, I'm experiencing technical difficulties.";
                }
            }
        }

        private static List<Dictionary<string, string>> BuildMessages(List<Message> history, string bookData, string imageAnalysis = "")
        {
            var messages = new List<Dictionary<string, string>>();

            messages.Add(new Dictionary<string, string>
            {
                {"role", "system"},
                {"content", @"You are a helpful library assistant. Follow these rules:
                1. When asked about books, ALWAYS use our database first
                2. For book requests: Provide 3-5 relevant books from database
                3. If user asks for general recommendations, show popular books
                4. Never mention you're checking a database - respond naturally
                5. If no books match, suggest alternatives or ask for clarification
                6. Maintain conversation context between questions
                7. If the user message includes image context or uploaded an image, acknowledge and respond to it appropriately
                8. Be helpful and conversational"}
            });

            int startIndex = Math.Max(0, history.Count - 8);
            for (int i = startIndex; i < history.Count; i++)
            {
                messages.Add(new Dictionary<string, string>
        {
            {"role", history[i].Role},
            {"content", history[i].Content}
        });
            }

            if (!string.IsNullOrEmpty(bookData))
            {
                messages.Add(new Dictionary<string, string>
        {
            {"role", "system"},
            {"content", bookData}
        });
            }

            return messages;
        }

        /*
        private static async Task<string> AnalyzeImage(string imageBase64, string mimeType, string logPath, string apiKey)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.DefaultRequestHeaders.Add("X-Title", "Library Image Analysis");

                var requestBody = new
                {
                    model = "deepseek/deepseek-r1-0528:free",
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = new object[]
                            {
                                new { type = "text", text = "Describe this image in detail, focusing on any text, book covers, or library-related content you can see." },
                                new {
                                    type = "image_url",
                                    image_url = new {
                                        url = $"data:{mimeType};base64,{imageBase64}"
                                    }
                                }
                            }
                        }
                    },
                    max_tokens = 2000,
                    temperature = 0.2
                };

                File.AppendAllText(logPath, $"{DateTime.UtcNow} - R1 Image Request\n\n");

                try
                {
                    var response = await client.PostAsync(
                        "https://openrouter.ai/api/v1/chat/completions",
                        new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json")
                    );

                    var responseContent = await response.Content.ReadAsStringAsync();
                    File.AppendAllText(logPath, $"{DateTime.UtcNow} - R1 Response (Status: {response.StatusCode}):\n{responseContent}\n\n");

                    if (response.IsSuccessStatusCode)
                    {
                        var result = JsonConvert.DeserializeObject<OpenRouterResponse>(responseContent);
                        var content = result?.choices?[0]?.message?.content?.Trim() ?? "";

                        // Check if the model actually processed the image
                        if (content.Contains("can't directly view") || content.Contains("I can't view") ||
                            content.Contains("provide a detailed text description"))
                        {
                            return "R1 model cannot process images";
                        }

                        return content;
                    }
                }
                catch (Exception ex)
                {
                    File.AppendAllText(logPath, $"{DateTime.UtcNow} - R1 ERROR: {ex.Message}\n\n");
                }

                return "Image analysis failed";
            }
        }
        */
        private static async Task<string> AnalyzeImageWithVisionModel(string imageBase64, string mimeType, string logPath, string apiKey)
        {
            var visionModels = new[]
            {
                "meta-llama/llama-3.2-11b-vision-instruct:free",
            };

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(45);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.DefaultRequestHeaders.Add("X-Title", "Library Vision Analysis");

                foreach (var model in visionModels)
                {
                    try
                    {
                        var requestBody = new
                        {
                            model = model,
                            messages = new[]
                            {
                                new
                                {
                                    role = "user",
                                    content = new object[]
                                    {
                                        new { type = "text", text = "Describe what you see in this image, especially any text, book titles, or library-related content." },
                                        new {
                                            type = "image_url",
                                            image_url = new {
                                                url = $"data:{mimeType};base64,{imageBase64}"
                                            }
                                        }
                                    }
                                }
                            },
                            max_tokens = 1000,
                            temperature = 0.1
                        };

                        File.AppendAllText(logPath, $"{DateTime.UtcNow} - Vision Model ({model}) Request\n\n");

                        var response = await client.PostAsync(
                            "https://openrouter.ai/api/v1/chat/completions",
                            new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json")
                        );

                        var responseContent = await response.Content.ReadAsStringAsync();
                        File.AppendAllText(logPath, $"{DateTime.UtcNow} - Vision Model ({model}) Response (Status: {response.StatusCode}):\n{responseContent}\n\n");

                        if (response.IsSuccessStatusCode)
                        {
                            var result = JsonConvert.DeserializeObject<OpenRouterResponse>(responseContent);
                            var content = result?.choices?[0]?.message?.content?.Trim() ?? "";

                            if (!string.IsNullOrEmpty(content) && !content.Contains("can't") && !content.Contains("unable"))
                            {
                                return content;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logPath, $"{DateTime.UtcNow} - Vision Model ({model}) ERROR: {ex.Message}\n\n");
                        continue;
                    }
                }
            }

            return "Unable to analyze image with available vision models";
        }

        private static void SaveMessageToDatabase(string chatId, string role, string content, 
            string imageBase64 = null, string mimeType = null)
        {
            try
            {
                Guid chatGuid = Guid.Parse(chatId);

                string imageData = string.IsNullOrEmpty(imageBase64) ?
                    null : $"data:{mimeType};base64,{imageBase64}";

                string query = @"
            INSERT INTO ChatMessages (ChatID, Role, Content, ImageData) 
            VALUES (@ChatID, @Role, @Content, @ImageData)";

                SqlParameter[] parameters = {
            new SqlParameter("@ChatID", SqlDbType.UniqueIdentifier) { Value = chatGuid },
            new SqlParameter("@Role", role),
            new SqlParameter("@Content", content),
            new SqlParameter("@ImageData", imageData ?? (object)DBNull.Value) 
        };

                var db = new DatabaseService();
                db.ExecuteNonQuery(query, parameters);
            }
            //catch (Exception ex)
            catch
            {
                // Log error here
            }
        }

        public static List<Message> GetChatHistory(string chatId)
        {
            Guid chatGuid = Guid.Parse(chatId);

            string query = @"
                SELECT Role, Content 
                FROM ChatMessages 
                WHERE ChatID = @ChatID 
                ORDER BY Timestamp";

            SqlParameter[] parameters = {
                new SqlParameter("@ChatID", SqlDbType.UniqueIdentifier) {
                    Value = chatGuid
                }
            };

            var db = new DatabaseService();
            DataTable dt = db.GetDataN(query, parameters);

            return dt.AsEnumerable().Select(row => new Message
            {
                Role = row["Role"].ToString(),
                Content = row["Content"].ToString()
            }).ToList();
        }

        private static bool IsGeneralBookInquiry(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return false;

            var lowerMessage = message.ToLower();

            var generalPhrases = new[] {
                "what books", "list books", "show books", "do you have",
                "what kind of books", "available books", "library collection",
                "books do you have", "all the books", "any books", "give me all the books",

                "in the catalog", "in the database", "what do you have", "just curious",
                "can i check it out", "don't worry about the genre"
            };

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
            prompt.AppendLine("You are a helpful library assistant. Follow these rules:");
            prompt.AppendLine("1. When asked about books, ALWAYS use our database first");
            prompt.AppendLine("2. For book requests: Provide 3-5 relevant books from database");
            prompt.AppendLine("3. If user asks for general recommendations, show popular books");
            prompt.AppendLine("4. Never mention you're checking a database - respond naturally");
            prompt.AppendLine("5. If no books match, suggest alternatives or ask for clarification");
            prompt.AppendLine("6. Maintain conversation context between questions");

            int startIndex = Math.Max(0, history.Count - 8);
            for (int i = startIndex; i < history.Count; i++)
            {
                prompt.AppendLine($"{history[i].Role}: {history[i].Content}");
            }

            if (!string.IsNullOrEmpty(bookData))
            {
                prompt.AppendLine($"system: {bookData}");
            }

            prompt.Append("Assistant: ");
            return prompt.ToString();
        }

        public static bool IsFirstUserMessage(string chatId)
        {
            Guid chatGuid = Guid.Parse(chatId);

            string query = @"
                SELECT COUNT(*) 
                FROM ChatMessages 
                WHERE ChatID = @ChatID AND Role = 'user'";

                    SqlParameter[] parameters = {
                new SqlParameter("@ChatID", SqlDbType.UniqueIdentifier) { Value = chatGuid }
            };

            var db = new DatabaseService();
            int count = Convert.ToInt32(db.ExecuteScalar(query, parameters));
            return count == 1;
        }

        public static Guid CreateNewChat(int clientId)
        {
            try
            {
                string query = @"
                    INSERT INTO Chats (ClientID, ChatTitle) 
                    OUTPUT INSERTED.ChatID
                    VALUES (@ClientID, 'New Chat')";

                        SqlParameter[] parameters = {
                    new SqlParameter("@ClientID", clientId)
                };

                var db = new DatabaseService();
                return (Guid)db.ExecuteScalar(query, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Chat creation failed in database", ex);
            }
        }

        public static List<ChatInfo> GetChatSessions(int clientId)
        {
            string query = @"
                SELECT c.ChatID, c.ChatTitle, c.CreatedDate
                FROM Chats c
                WHERE c.ClientID = @ClientID
                ORDER BY c.CreatedDate DESC";

            SqlParameter[] parameters = {
                new SqlParameter("@ClientID", clientId)
            };

            var db = new DatabaseService();
            DataTable dt = db.GetDataN(query, parameters);

            return dt.AsEnumerable().Select(row => new ChatInfo
            {
                ChatId = row["ChatID"].ToString(),
                Title = row["ChatTitle"].ToString(),
                CreatedDate = Convert.ToDateTime(row["CreatedDate"])
            }).ToList();
        }

        public static void UpdateChatTitle(string chatId, string title)
        {
            string query = @"
                UPDATE Chats 
                SET ChatTitle = @Title 
                WHERE ChatID = @ChatID";

            SqlParameter[] parameters = {
                new SqlParameter("@ChatID", chatId),
                new SqlParameter("@Title", title)
            };

            var db = new DatabaseService();
            db.ExecuteNonQuery(query, parameters);
        }

        public static bool DeleteChat(Guid chatId)
        {
            try
            {
                string query = @"
                    DELETE FROM ChatMessages WHERE ChatID = @ChatID;
                    DELETE FROM Chats WHERE ChatID = @ChatID;";

                SqlParameter[] parameters = {
                    new SqlParameter("@ChatID", chatId)
                };

                var db = new DatabaseService();
                db.ExecuteNonQuery(query, parameters);
                return true;
            }
            catch
            {
                return false;
            }
        }


        private static DateTime GetChatCreatedDate(List<Message> chat)
        {
            return chat.Count > 0 ? DateTime.UtcNow.AddMinutes(-chat.Count) : DateTime.UtcNow;
        }

        private static string Truncate(string value, int maxLength)
        {
            return string.IsNullOrEmpty(value)
                ? value
                : value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        }
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