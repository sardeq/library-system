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
            var apiKey = ConfigurationManager.AppSettings["OpenRouter_ApiKey"];

            ServicePointManager.ServerCertificateValidationCallback +=
                 (sender, cert, chain, sslPolicyErrors) => true;

            using (var client = new HttpClient())
            {
                try
                {
                    client.Timeout = TimeSpan.FromSeconds(60);

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
                        messages = new[]
                        {
                    new { role = "user", content = prompt }
                },
                        max_tokens = 8,
                        temperature = 0.1
                    };

                    //File.WriteAllText("C:\\temp\\openrouter_request.txt",
                    //JsonConvert.SerializeObject(requestBody));

                    using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(55)))
                    {
                        var response = await client.PostAsync(
                            "https://openrouter.ai/api/v1/chat/completions",
                            new StringContent(JsonConvert.SerializeObject(requestBody),
                                Encoding.UTF8,
                                "application/json"),
                            cts.Token
                        );

                        //File.WriteAllText("C:\\temp\\openrouter_response.txt",
                        //$"{DateTime.UtcNow} - Status: {response.StatusCode}\n{response.Content}");

                        if (response.IsSuccessStatusCode)
                        {
                            var responseContent = await response.Content.ReadAsStringAsync();
                            System.IO.File.WriteAllText("C:\\temp\\openrouter_response.txt", responseContent);
                            var result = JsonConvert.DeserializeObject<OpenRouterResponse>(responseContent);

                            if (result?.choices?.Count > 0 && !string.IsNullOrEmpty(result.choices[0]?.message?.content))
                            {
                                var classification = result.choices[0].message.content
                                    .Trim()
                                    .Split(' ')[0]
                                    .ToLower()
                                    .Replace(".", "");

                                var validSentiments = new[] { "positive", "negative", "mixed", "unknown" };
                                return validSentiments.Contains(classification)
                                    ? classification
                                    : "unknown";
                            }
                        }
                        else
                        {
                            // Log the error response for debugging
                            var errorContent = await response.Content.ReadAsStringAsync();
                            throw new HttpRequestException($"API returned {response.StatusCode}: {errorContent}");
                        }
                    }
                }
                catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
                {
                    throw new TaskCanceledException("Request to OpenRouter API timed out", ex);
                }
                catch (TaskCanceledException ex)
                {
                    throw new TaskCanceledException("Request was canceled", ex);
                }
                catch (HttpRequestException ex)
                {
                    throw new HttpRequestException("Network error calling OpenRouter API", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Unexpected error calling OpenRouter API: {ex.Message}", ex);
                }
            }

            return "unknown";
        }

        public static bool SaveReviewToDatabase(int userId, string reviewDesc, string sentiment)
        {
            try
            {
                string query = @"INSERT INTO Reviews (UserID, ReviewDesc, ReviewFeeling) VALUES (@UserID, @ReviewDesc, @ReviewFeeling)";
                SqlParameter[] insertParams = {
                    new SqlParameter("@UserID", userId),
                    new SqlParameter("@ReviewDesc", reviewDesc),
                    new SqlParameter("@ReviewFeeling", sentiment)
                };
                _db.ExecuteNonQuery(query, insertParams);
                return true;
            }
            catch
            {
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
                var line = $"{userId},{(sentiment == "positive" ? 1 : 0)},{(sentiment == "negative" ? 1 : 0)},{(sentiment == "mixed" ? 1 : 0)},{(sentiment == "unknown" ? 1 : 0)},\"{reviewDesc.Replace("\"", "\"\"")}\",{age},{borrowedCount}";

                var dir = Path.GetDirectoryName(csvPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                File.AppendAllText(csvPath, line + Environment.NewLine);
                return true;
            }
            catch (Exception ex)
            {
                File.WriteAllText("C:\\temp\\csv_error.txt", ex.ToString());
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