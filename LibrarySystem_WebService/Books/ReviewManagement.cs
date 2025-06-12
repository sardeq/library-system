using Newtonsoft.Json;
using SchoolSystem.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace LibrarySystem_WebService.Books
{
    public class ReviewManagement
    {
        private static readonly DatabaseService _db = new DatabaseService();

        public static async Task<string> GetSentimentFromChatGPT(string review)
        {
            var apiKey = ConfigurationManager.AppSettings["OpenRouter_ApiKey"]?.Trim();
            var logPath = "C:\\Users\\sodeh\\logs.txt";

            using (var client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", apiKey);

                    client.DefaultRequestHeaders.Add("X-Title", "Library System");
                    client.DefaultRequestHeaders.Add("User-Agent", "LibrarySystem/1.0");

                    var prompt = @"Classify this book review in exactly one word: 
                        'positive', 'negative', 'mixed', or 'unknown'. 

                        Review: """ + review + @""" 
                        Classification:";

                    var requestBody = new
                    {
                        model = "google/gemma-3n-e4b-it:free",
                        messages = new[] { new { role = "user", content = prompt } },
                        max_tokens = 50,
                        temperature = 0.1
                    };

                    File.AppendAllText(logPath, $"{DateTime.UtcNow} - Request:\n{JsonConvert.SerializeObject(requestBody)}\n\n");

                    using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120)))
                    {
                        var response = await client.PostAsync(
                            "https://openrouter.ai/api/v1/chat/completions",
                            new StringContent(
                                JsonConvert.SerializeObject(requestBody),
                                Encoding.UTF8,
                                "application/json"),
                            cts.Token
                        );

                        var responseContent = await response.Content.ReadAsStringAsync();

                        File.AppendAllText(logPath,
                            $"{DateTime.UtcNow} - Status: {response.StatusCode}\n" +
                            $"Response: {responseContent}\n\n");

                        if (response.IsSuccessStatusCode)
                        {
                            var result = JsonConvert.DeserializeObject<OpenRouterResponse>(responseContent);

                            if (result?.choices?.Count > 0 &&
                            !string.IsNullOrEmpty(result.choices[0]?.message?.content))
                            {
                                var rawContent = result.choices[0].message.content;
                                var classification = rawContent
                                    .Trim()
                                    .Split(new[] { ' ', '\n', '\r', '.', ':', ';', '!', '?' },
                                          StringSplitOptions.RemoveEmptyEntries)
                                    .FirstOrDefault()?
                                    .ToLower();

                                File.AppendAllText(logPath,
                                    $"{DateTime.UtcNow} - Raw: '{rawContent}' | Cleaned: '{classification}'\n");

                                var validSentiments = new[] { "positive", "negative", "mixed", "unknown" };
                                return validSentiments.Contains(classification)
                                    ? classification
                                    : "unknown";
                            }
                            else
                            {
                                File.AppendAllText(logPath,
                                    $"{DateTime.UtcNow} - ERROR: Empty API response\n");
                            }
                        }
                        else
                        {
                            throw new HttpRequestException(
                                $"API returned {response.StatusCode}: {responseContent}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    File.AppendAllText(logPath, $"{DateTime.UtcNow} - ERROR: {ex}\n\n");
                    throw;
                }
            }
            return "unknown";
        }

        public static bool SaveReviewToDatabase(int userId, string reviewDesc, string sentiment)
        {
            var logPath = "C:\\Users\\sodeh\\logs.txt";

            try
            {
                if (string.IsNullOrEmpty(reviewDesc))
                {
                    File.AppendAllText(logPath, $"{DateTime.UtcNow} - Empty review detected\n");
                    return false;
                }

                string query = @"INSERT INTO Reviews (UserID, ReviewDesc, ReviewFeeling) VALUES (@UserID, @ReviewDesc, @ReviewFeeling)";
                SqlParameter[] insertParams = {
                    new SqlParameter("@UserID", userId),
                    new SqlParameter("@ReviewDesc", reviewDesc),
                    new SqlParameter("@ReviewFeeling", sentiment)
                };
                _db.ExecuteNonQuery(query, insertParams);
                return true;
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, $"{DateTime.UtcNow} - DB ERROR: {ex.Message}\n");
                return false;
            }
        }

        public static UserDetails GetUserDetails(int userId)
        {
            var dt = _db.GetDataN(
                "SELECT Age, (SELECT COUNT(*) FROM Borrow WHERE ClientID = @UserID) AS BorrowedCount FROM Clients WHERE ClientID = @UserID",
                new SqlParameter[] { new SqlParameter("@UserID", userId) });

            if (dt.Rows.Count > 0)
            {
                return new UserDetails
                {
                    Age = Convert.ToInt32(dt.Rows[0]["Age"]),
                    BorrowedCount = Convert.ToInt32(dt.Rows[0]["BorrowedCount"])
                };
            }
            return new UserDetails();
        }

        public class ReviewResult
        {
            public bool Success { get; set; }
            public string Sentiment { get; set; }
            public string Error { get; set; }
        }

        public static bool UpdateReviewDataCSV(int userId, string reviewDesc, string sentiment, int age, int borrowedCount)
        {
            try
            {
                var csvPath = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/ReviewData.csv");
                var dir = Path.GetDirectoryName(csvPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                // Create file with headers if it doesn't exist
                if (!File.Exists(csvPath))
                {
                    var headers = "UserID,Positive,Negative,Mixed,Unknown,ReviewDesc,Age,BorrowedCount";
                    File.WriteAllText(csvPath, headers + Environment.NewLine);
                }

                var line = $"{userId},{(sentiment == "positive" ? 1 : 0)},{(sentiment == "negative" ? 1 : 0)},{(sentiment == "mixed" ? 1 : 0)},{(sentiment == "unknown" ? 1 : 0)},\"{reviewDesc.Replace("\"", "\"\"")}\",{age},{borrowedCount}";
                File.AppendAllText(csvPath, line + Environment.NewLine);

                return true;
            }
            catch (Exception ex)
            {
                File.WriteAllText("C:\\Users\\sodeh\\csv.txt", ex.ToString());
                return false;
            }
        }
    }

    public class UserDetails
    {
        public int Age { get; set; }
        public int BorrowedCount { get; set; }
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