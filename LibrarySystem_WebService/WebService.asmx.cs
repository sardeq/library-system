using SchoolSystem.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using LibrarySystem_WebService.Auth;
using LibrarySystem_WebService.Admin;
using System.Data;
using LibrarySystem_WebService.Books;
using LibrarySystem_WebService.Report;
using System.Threading.Tasks;
using System.Net.Http;
using LibrarySystem_Shared.Models;

namespace LibrarySystem_WebService
{
    /// <summary>
    /// Summary description for WebService
    /// </summary>
    /// 

    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class WebService : System.Web.Services.WebService
    {
        #region Authentication
        [WebMethod]
        public LoginResult Login(string username, string password)
        {
            try
            {
                var loginService = new Login();
                return loginService.LoginClick(username, password);
            }
            catch (Exception ex)
            {
                return new LoginResult
                {
                    Success = false,
                    Message = "Login failed: " + ex.Message
                };
            }
        }
        #endregion

        #region User Management
        [WebMethod]
        public List<User> GetUsers()
        {
            try
            {
                var userService = new ManageUser();
                return userService.GetAllUsers();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting users: " + ex.Message);
            }
        }

        [WebMethod]
        public bool UpdateUserLanguage(int clientId, int languageId)
        {
            try
            {
                var userService = new ManageUser();
                return userService.UpdateUserLanguage(clientId, languageId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating language: " + ex.Message);
            }
        }

        [WebMethod]
        public bool SaveUser(User user)
        {
            try
            {
                var userService = new ManageUser();
                return userService.SaveUser(user);
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving user: " + ex.Message);
            }
        }

        [WebMethod]
        public bool DeleteUser(int clientId)
        {
            try
            {
                var userService = new ManageUser();
                return userService.DeleteUser(clientId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting user: " + ex.Message);
            }
        }

        [WebMethod]
        public DataTable GetUserTypes()
        {
            var userService = new ManageUser();

            return userService.GetUserTypes();
        }

        [WebMethod]
        public DataTable GetStatusList()
        {
            var userService = new ManageUser();

            return userService.GetStatusList();
        }

        [WebMethod]
        public DataTable GetGenders()
        {
            var userService = new ManageUser();

            return userService.GetGenders();
        }

        [WebMethod]
        public DataTable GetLanguages()
        {
            var userService = new ManageUser();

            return userService.GetLanguages();
        }

        [WebMethod]
        public User GetUserById(int clientId)
        {
            try
            {
                var userService = new ManageUser();
                return userService.GetUserById(clientId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting user: " + ex.Message);
            }
        }

        [WebMethod]
        public bool UpdateAccount(User user)
        {
            try
            {
                var userService = new ManageUser();
                return userService.SaveUser(user);
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating account: " + ex.Message);
            }
        }

        [WebMethod]
        public bool ChangePassword(int clientId, string currentPassword, string newPassword)
        {
            try
            {
                var userService = new ManageUser();
                return userService.ChangePassword(clientId, currentPassword, newPassword);
            }
            catch (Exception ex)
            {
                throw new Exception("Error changing password: " + ex.Message);
            }
        }
        #endregion

        #region Book Management

        [WebMethod]
        public List<Book> GetAvailableBooks(int userType)
        {
            try
            {
                var bookService = new LibrarySystem_WebService.Books.BookManagement();
                return bookService.GetAvailableBooks(userType);
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting available books: " + ex.Message);
            }
        }

        [WebMethod]
        public List<BorrowInfo> GetBorrowedBooks(int clientId)
        {
            try
            {
                var bookService = new LibrarySystem_WebService.Books.BookManagement();
                return bookService.GetBorrowedBooks(clientId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting borrowed books: " + ex.Message);
            }
        }

        [WebMethod]
        public List<BorrowInfo> GetDueSoonBooks(int clientId)
        {
            try
            {
                var bookService = new LibrarySystem_WebService.Books.BookManagement();
                return bookService.GetDueSoonBooks(clientId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting due soon books: " + ex.Message);
            }
        }

        [WebMethod]
        public List<BorrowInfo> SearchBorrowedBooks(string term)
        {
            try
            {
                var bookService = new LibrarySystem_WebService.Books.BookManagement();
                return bookService.SearchBorrowedBooks(term);
            }
            catch (Exception ex)
            {
                throw new Exception("Error searching borrowed books: " + ex.Message);
            }
        }

        [WebMethod]
        public bool RequestReturn(int clientId, string bookId)
        {
            try
            {
                var bookService = new BookManagement();
                return bookService.RequestReturn(clientId, bookId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error searching borrowed books: " + ex.Message);
            }
        }

        [WebMethod]
        public bool ConfirmReturn(int clientId, string bookId)
        {
            try
            {
                var bookService = new BookManagement();
                return bookService.ConfirmReturn(clientId, bookId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error searching borrowed books: " + ex.Message);
            }
        }

        [WebMethod]
        public BorrowResult BorrowBooks(int clientId, List<string> bookIds)
        {
            try
            {
                var bookService = new LibrarySystem_WebService.Books.BookManagement();
                var (success, errors) = bookService.BorrowBooks(clientId, bookIds);
                return new BorrowResult { Success = success, Errors = errors };
            }
            catch (Exception ex)
            {
                return new BorrowResult
                {
                    Success = false,
                    Errors = { "Error borrowing books: " + ex.Message }
                };
            }
        }

        [WebMethod]
        public bool AddBook(Book book)
        {
            try
            {
                var bookService = new BookManagement();
                return bookService.AddBook(book);
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding book: " + ex.Message);
            }
        }

        [WebMethod]
        public bool UpdateBook(Book book)
        {
            try
            {
                var bookService = new BookManagement();
                return bookService.UpdateBook(book);
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating book: " + ex.Message);
            }
        }

        [WebMethod]
        public List<BorrowInfo> GetPendingReturns()
        {
            try
            {
                var bookService = new BookManagement();
                return bookService.GetPendingReturns();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting returns: " + ex.Message);
            }
        }

        [WebMethod]
        public DeleteBookResult DeleteBook(string bookId)
        {
            try
            {
                var bookService = new BookManagement();
                var (success, errorMessage) = bookService.DeleteBook(bookId);
                return new DeleteBookResult { Success = success, ErrorMessage = errorMessage };
            }
            catch (Exception ex)
            {
                return new DeleteBookResult
                {
                    Success = false,
                    ErrorMessage = "Error deleting book: " + ex.Message
                };
            }
        }

        [WebMethod]
        public bool HasOverdueBooks(int clientId)
        {
            try
            {
                var bookService = new BookManagement();
                return bookService.HasOverdueBooks(clientId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error checking books: " + ex.Message);
            }
        }

        [WebMethod]
        public List<Book> GetAllBooks()
        {
            try
            {
                var bookService = new LibrarySystem_WebService.Books.BookManagement();
                return bookService.GetAllBooks();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting available books: " + ex.Message);
            }
        }

        #endregion

        #region Report Management

        [WebMethod]
        public List<BorrowInfo> GetBorrowReport(int? userId = null, string bookId = null,
                                       DateTime? fromDate = null, DateTime? toDate = null, bool dueSoon = false, 
                                       bool overdue = false)
        {
            try
            {
                var reportService = new ReportManagement();
                return reportService.GetBorrowReport(userId, bookId, fromDate, toDate, dueSoon, overdue);
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating report: " + ex.Message);
            }
        }

        #endregion


        #region Reviews

        [WebMethod]
        public ReviewResult ProcessReview(int userId, string bookId, string reviewText)
        {
            try
            {
                return Task.Run(() => ProcessReviewAsync(userId, bookId, reviewText)).Result; // Temporary bridge
            }
            catch (AggregateException ae)
            {
                return new ReviewResult
                {
                    Success = false,
                    Error = ae.Flatten().InnerException?.Message ?? "Unknown error"
                };
            }
            catch (Exception ex)
            {
                return new ReviewResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        private async Task<ReviewResult> ProcessReviewAsync(int userId, string bookId, string reviewText)
        {
            var sentiment = await ReviewManagement.GetSentimentFromChatGPT(reviewText);
            var details = ReviewManagement.GetUserDetails(userId);
            bool savedToDb = ReviewManagement.SaveReviewToDatabase(userId, reviewText, sentiment);
            bool csvUpdated = ReviewManagement.UpdateReviewDataCSV(
                userId, reviewText, sentiment, details.Age, details.BorrowedCount
            );

            return new ReviewResult
            {
                Success = savedToDb && csvUpdated,
                Sentiment = savedToDb && csvUpdated ? sentiment : null,
                Error = (!savedToDb || !csvUpdated) ? "Database/CSV update failed" : null
            };
        }



        #endregion

    }
}
