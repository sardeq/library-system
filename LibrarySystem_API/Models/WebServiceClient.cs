using System;
using System.Configuration;
using System.Threading.Tasks;
using LibrarySystem_API.LibraryWebServiceRef; // Contains WebService and its ReviewResult

namespace LibrarySystem_API.Models
{
    public static class WebServiceClient
    {
        private static readonly Lazy<WebService> _instance =
            new Lazy<WebService>(() => new WebService());

        public static WebService Instance => _instance.Value;

        public static ReviewResult ProcessReview(int userId, string bookId, string reviewText)
        {
            return Instance.ProcessReview(userId, bookId, reviewText);
        }

        public static async Task<ReviewResult> ProcessReviewAsync(int userId, string bookId, string reviewText)
        {
            return await Task.Run(() => Instance.ProcessReview(userId, bookId, reviewText));
        }

      
    }
}