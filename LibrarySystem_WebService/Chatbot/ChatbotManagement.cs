using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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
        public static async Task<string> GetChatbotResponse(string message)
        {
            var apiKey = ConfigurationManager.AppSettings["OpenRouter_ApiKey"]?.Trim();
            var logPath = HostingEnvironment.MapPath("~/App_Data/chatbot_log.txt");

            using (var client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", apiKey);

                    client.DefaultRequestHeaders.Add("X-Title", "Library Chatbot");
                    client.DefaultRequestHeaders.Add("User-Agent", "LibrarySystem/1.0");

                    var prompt = @"You are a helpful assistant for a library system. Your role is to assist users with library-related queries. 
            You can help with questions about borrowing books, returning books, account information, and general library policies. 
            Please keep your responses concise and relevant to the library context but if asked about books you can recommend some from the internet. If not given details, give a speculated answer and ask it to repeat with details needed.

            User: " + message + @"
            Assistant:";

                    var requestBody = new
                    {
                        model = "google/gemma-3n-e4b-it:free",
                        messages = new[] { new { role = "user", content = prompt } },
                        max_tokens = 200,
                        temperature = 0.7
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

                    if (response.IsSuccessStatusCode)
                    {
                        var result = JsonConvert.DeserializeObject<OpenRouterResponse>(responseContent);
                        return result?.choices?[0]?.message?.content?.Trim() ?? "I couldn't process that request.";
                    }
                    else
                    {
                        return $"Error: API returned {response.StatusCode}";
                    }
                }
                catch (Exception ex)
                {
                    File.AppendAllText(logPath, $"{DateTime.UtcNow} - ERROR: {ex}\n\n");
                    return "Sorry, I'm experiencing technical difficulties.";
                }
            }
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