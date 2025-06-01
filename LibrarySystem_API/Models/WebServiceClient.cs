using System;
using System.Threading.Tasks;
using LibrarySystem_API.LibraryWebServiceRef;
using Task = System.Threading.Tasks.Task;

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
    }
}