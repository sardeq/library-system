using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using LibrarySystem_API.LibraryWebServiceRef;
using LibrarySystem_Shared.Models;
using System.Linq;

namespace LibrarySystem_API.Models
{
    public static class WebServiceClient
    {
        private static readonly Lazy<WebService> _instance =
            new Lazy<WebService>(() => new WebService());

        public static WebService Instance => _instance.Value;

        public static LibraryWebServiceRef.ReviewResult ProcessReview(int userId, string bookId, string reviewText)
        {
            return Instance.ProcessReview(userId, bookId, reviewText);
        }

        public static async Task<LibraryWebServiceRef.ReviewResult> ProcessReviewAsync(int userId, string bookId, string reviewText)
        {
            return await System.Threading.Tasks.Task.Run(() => Instance.ProcessReview(userId, bookId, reviewText));
        }


        public static Guid CreateNewChat(int clientId)
        {
            return Instance.CreateNewChat(clientId);
        }

        public static List<LibraryWebServiceRef.ChatInfo> GetChatSessions(int clientId)
        {
            return Instance.GetChatSessions(clientId).ToList();
        }

        public static async Task<string> GetChatbotResponseAsync(string message, Guid chatId, int clientId)
        {
            var response = await Task.Run(() => Instance.GetChatbotResponse(message, chatId, clientId));
            return response.Message;
        }

    }
}