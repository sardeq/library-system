using LibrarySystem_API.LibraryWebServiceRef;
using LibrarySystem_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace LibrarySystem_API.Controllers
{
    [RoutePrefix("api/books")]
    public class BooksController : ApiController
    {
        [HttpGet]
        public IHttpActionResult GetAllBooks()
        {
            var _client = WebServiceClient.Instance;
            return Ok(_client.GetAllBooks());
        }

        [HttpPost]
        public IHttpActionResult AddBook([FromBody] Book book)
        {
            var _client = WebServiceClient.Instance;
            return Ok(_client.AddBook(book));
        }

        [HttpGet]
        [Route("has-overdue/{clientId}")]
        public IHttpActionResult HasOverdueBooks(int clientId)
        {
            var _client = WebServiceClient.Instance;
            return Ok(_client.HasOverdueBooks(clientId));
        }

        [HttpGet]
        [Route("borrowed/{clientId}")]
        public IHttpActionResult GetBorrowedBooks(int clientId)
        {
            var _client = WebServiceClient.Instance;
            return Ok(_client.GetBorrowedBooks(clientId));
        }

        [HttpPost]
        [Route("update")]
        public IHttpActionResult UpdateBook([FromBody] Book book)
        {
            var _client = WebServiceClient.Instance;
            return Ok(_client.UpdateBook(book));
        }

        [HttpGet]
        [Route("{bookId}")]
        public IHttpActionResult GetBook(string bookId)
        {
            var _client = WebServiceClient.Instance;
            var books = _client.GetAllBooks();
            var book = books.FirstOrDefault(b => b.BookID == bookId);
            if (book == null)
                return NotFound();
            return Ok(book);
        }

        [HttpPost]
        [Route("delete/{bookId}")]
        public IHttpActionResult DeleteBook(string bookId)
        {
            try
            {
                var _client = WebServiceClient.Instance;
                var result = _client.DeleteBook(bookId);

                if (!result.Success)
                {
                    return Content(HttpStatusCode.Conflict, new
                    {
                        Success = false,
                        Message = result.ErrorMessage
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Book deleted successfully!"
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("available/{userType}")]
        public IHttpActionResult GetAvailableBooks(int userType)
        {
            var _client = WebServiceClient.Instance;
            return Ok(_client.GetAvailableBooks(userType));
        }

        [HttpGet]
        [Route("pending-returns")]
        public IHttpActionResult GetPendingReturns()
        {
            var _client = WebServiceClient.Instance;
            return Ok(_client.GetPendingReturns());
        }

        [HttpPost]
        [Route("request-return")]
        public IHttpActionResult RequestReturn([FromBody] ReturnRequest request)
        {
            var _client = WebServiceClient.Instance;
            return Ok(_client.RequestReturn(request.ClientID, request.BookID));
        }

        [HttpPost]
        [Route("confirm-return")]
        public IHttpActionResult ConfirmReturn([FromBody] ReturnRequest request)
        {
            var _client = WebServiceClient.Instance;
            return Ok(_client.ConfirmReturn(request.ClientID, request.BookID));
        }

        [HttpPost]
        [Route("borrow")]
        public IHttpActionResult BorrowBooks([FromBody] BorrowRequest request)
        {
            try
            {
                var _client = WebServiceClient.Instance;
                var result = _client.BorrowBooks(request.ClientID, request.BookIDs.ToArray());
                return Ok(new
                {
                    result.Success,
                    Message = result.Success ? "Books borrowed successfully!" : "Partial/failure",
                    result.Errors
                });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, new
                {
                    Error = ex.Message
                });
            }
        }

        [HttpGet]
        [Route("due-soon/{clientId}")]
        public IHttpActionResult GetDueSoonBooks(int clientId)
        {
            var _client = WebServiceClient.Instance;
            return Ok(_client.GetDueSoonBooks(clientId));
        }

        [HttpGet]
        [Route("search")]
        public IHttpActionResult SearchBorrowedBooks(string term)
        {
            var _client = WebServiceClient.Instance;
            return Ok(_client.SearchBorrowedBooks(term));
        }
    }

    public class BorrowRequest
    {
        public int ClientID { get; set; }
        public List<string> BookIDs { get; set; }
    }

    public class ReturnRequest
    {
        public int ClientID { get; set; }
        public string BookID { get; set; }
    }
}
