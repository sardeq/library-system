using SchoolSystem.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace LibrarySystem_WebService.Books
{
    public class BookManagement
    {
        private readonly DatabaseService _db = new DatabaseService();

        public List<Book> GetAllBooks()
        {
            string query = "SELECT * FROM Books";
            DataTable dt = _db.GetData(query);
            List<Book> books = new List<Book>();
            foreach (DataRow row in dt.Rows)
            {
                books.Add(new Book
                {
                    BookID = row["BookID"].ToString(),
                    Title = row["Title"].ToString(),
                    Author = row["Author"].ToString(),
                    ReleaseDate = Convert.ToDateTime(row["ReleaseDate"]),
                    BooksAvailable = Convert.ToInt32(row["BooksAvailable"]),
                    BorrowType = Convert.ToInt32(row["BorrowType"]),
                    BorrowDuration = Convert.ToInt32(row["BorrowDuration"])
                }); ;
            }
            return books;
        }

        public List<Book> GetAvailableBooks(int userType)
        {
            string query = "SELECT * FROM Books WHERE BooksAvailable > 0";
            if (userType == (int)UserType.Student)
            {
                query += " AND BorrowType = 0";
            }
            DataTable dt = _db.GetData(query);
            List<Book> books = new List<Book>();
            foreach (DataRow row in dt.Rows)
            {
                books.Add(new Book
                {
                    BookID = row["BookID"].ToString(),
                    Title = row["Title"].ToString(),
                    Author = row["Author"].ToString(),
                    ReleaseDate = Convert.ToDateTime(row["ReleaseDate"]),
                    BooksAvailable = Convert.ToInt32(row["BooksAvailable"]),
                    BorrowType = Convert.ToInt32(row["BorrowType"]),
                    BorrowDuration = Convert.ToInt32(row["BorrowDuration"])
                });
            }
            return books;
        }

        public List<BorrowInfo> GetBorrowedBooks(int clientId)
        {
            string query = @"
                SELECT b.BookID, bk.Title, b.BorrowDate, b.ReturnDate
                FROM Borrow b
                INNER JOIN Books bk ON b.BookID = bk.BookID
                WHERE b.ClientID = @ClientID 
                AND b.PendingConfirmation = 0
                AND b.Returned = 0";

            SqlParameter[] parameters = { new SqlParameter("@ClientID", clientId) };
            DataTable dt = _db.GetDataN(query, parameters);

            List<BorrowInfo> borrowInfos = new List<BorrowInfo>();
            foreach (DataRow row in dt.Rows)
            {
                borrowInfos.Add(new BorrowInfo
                {
                    BookID = row["BookID"].ToString(),
                    Title = row["Title"].ToString(),
                    BorrowDate = Convert.ToDateTime(row["BorrowDate"]),
                    ReturnDate = Convert.ToDateTime(row["ReturnDate"])
                });
            }
            return borrowInfos;
        }

        public bool RequestReturn(int clientId, string bookId)
        {
            string query = @"UPDATE Borrow SET PendingConfirmation = 1, ReturnDate = GETDATE()
                   WHERE ClientID = @ClientID AND BookID = @BookID ";

            SqlParameter[] parameters = {
                new SqlParameter("@ClientID", clientId),
                new SqlParameter("@BookID", bookId),
            };
            return _db.ExecuteNonQuery(query, parameters);
        }

        public bool ConfirmReturn(int clientId, string bookId)
        {
            string query = @"UPDATE Borrow SET Returned = 1
                   WHERE ClientID = @ClientID AND BookID = @BookID ";

            SqlParameter[] parameters = {
                new SqlParameter("@ClientID", clientId),
                new SqlParameter("@BookID", bookId),
            };

            bool deleted = _db.ExecuteNonQuery(query, parameters);

            if (deleted)
            {
                string updateBookQuery = "UPDATE Books SET BooksAvailable = BooksAvailable + 1 WHERE BookID = @BookID";
                _db.ExecuteNonQuery(updateBookQuery, new[] { new SqlParameter("@BookID", bookId) });
                return true;
            }
            return false;
        }

        public List<BorrowInfo> GetPendingReturns()
        {
            string query = @"
                SELECT b.ClientID, c.ClientName, b.BookID, bk.Title, b.BorrowDate
                FROM Borrow b
                INNER JOIN Clients c ON b.ClientID = c.ClientID
                INNER JOIN Books bk ON b.BookID = bk.BookID
                WHERE b.PendingConfirmation = 1 AND b.Returned = 0";

            DataTable dt = _db.GetData(query);

            List<BorrowInfo> pendingReturns = new List<BorrowInfo>();
            foreach (DataRow row in dt.Rows)
            {
                pendingReturns.Add(new BorrowInfo
                {
                    ClientID = Convert.ToInt32(row["ClientID"]),
                    ClientName = row["ClientName"].ToString(),
                    BookID = row["BookID"].ToString(),
                    Title = row["Title"].ToString(),
                    BorrowDate = Convert.ToDateTime(row["BorrowDate"])
                });
            }
            return pendingReturns;
        }

        public List<BorrowInfo> GetDueSoonBooks(int clientId)
        {
            string tomorrow = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            string query = @"
                SELECT b.BookID, bk.Title, b.BorrowDate, b.ReturnDate
                FROM Borrow b
                INNER JOIN Books bk ON b.BookID = bk.BookID
                WHERE b.ClientID = @ClientID AND CONVERT(date, b.ReturnDate) = @Tomorrow AND Returned = 0 AND PendingConfirmation = 0";
            SqlParameter[] parameters = {
                new SqlParameter("@ClientID", clientId),
                new SqlParameter("@Tomorrow", tomorrow)
            };
            DataTable dt = _db.GetDataN(query, parameters);
            List<BorrowInfo> borrowInfos = new List<BorrowInfo>();
            foreach (DataRow row in dt.Rows)
            {
                borrowInfos.Add(new BorrowInfo
                {
                    BookID = row["BookID"].ToString(),
                    Title = row["Title"].ToString(),
                    BorrowDate = Convert.ToDateTime(row["BorrowDate"]),
                    ReturnDate = Convert.ToDateTime(row["ReturnDate"])
                });
            }
            return borrowInfos;
        }

        public List<BorrowInfo> SearchBorrowedBooks(string term)
        {
            string query = @"
                SELECT b.ClientID, c.ClientName, b.BookID, bk.Title, 
                       b.BorrowDate, b.ReturnDate
                FROM Borrow b
                INNER JOIN Clients c ON b.ClientID = c.ClientID
                INNER JOIN Books bk ON b.BookID = bk.BookID
                WHERE (c.ClientName LIKE @Term OR bk.Title LIKE @Term)
                ";
            string searchTerm = "%" + term + "%";
            SqlParameter[] parameters = { new SqlParameter("@Term", searchTerm) };
            DataTable dt = _db.GetDataN(query, parameters);
            List<BorrowInfo> borrowInfos = new List<BorrowInfo>();
            foreach (DataRow row in dt.Rows)
            {
                borrowInfos.Add(new BorrowInfo
                {
                    ClientID = Convert.ToInt32(row["ClientID"]),
                    ClientName = row["ClientName"].ToString(),
                    BookID = row["BookID"].ToString(),
                    Title = row["Title"].ToString(),
                    BorrowDate = Convert.ToDateTime(row["BorrowDate"]),
                    ReturnDate = Convert.ToDateTime(row["ReturnDate"])
                });
            }
            return borrowInfos;
        }

        private User GetUser(int clientId)
        {
            string query = "SELECT * FROM Clients WHERE ClientID = @ClientID";
            SqlParameter[] parameters = { new SqlParameter("@ClientID", clientId) };
            DataTable dt = _db.GetDataN(query, parameters);

            if (dt.Rows.Count == 0) return null;

            DataRow row = dt.Rows[0];
            return new User
            {
                ClientID = clientId,
                Type = Convert.ToInt32(row["Type"]),
                Status = Convert.ToInt32(row["Status"]),
                BooksQuota = Convert.ToInt32(row["BooksQuota"]),
                BorrowDuration = Convert.ToInt32(row["BorrowDuration"])
            };
        }

        private Book GetBook(string bookId)
        {
            string query = "SELECT * FROM Books WHERE BookID = @BookID";
            SqlParameter[] parameters = { new SqlParameter("@BookID", bookId) };
            DataTable dt = _db.GetDataN(query, parameters);

            if (dt.Rows.Count == 0) return null;

            DataRow row = dt.Rows[0];
            return new Book
            {
                BookID = bookId,
                Title = row["Title"].ToString(),
                BooksAvailable = Convert.ToInt32(row["BooksAvailable"]),
                BorrowType = Convert.ToInt32(row["BorrowType"]),
                BorrowDuration = Convert.ToInt32(row["BorrowDuration"])
            };
        }

        private int GetBorrowedCount(int clientId)
        {
            string query = @"SELECT COUNT(*) FROM Borrow 
                   WHERE ClientID = @ClientID 
                   AND Returned = 0 
                   AND PendingConfirmation = 0";
            SqlParameter[] parameters = { new SqlParameter("@ClientID", clientId) };
            return Convert.ToInt32(_db.ExecuteScalar(query, parameters));
        }
        public bool HasOverdueBooks(int clientId)
        {
            string query = @"SELECT COUNT(*) FROM Borrow 
                   WHERE ClientID = @ClientID 
                   AND ReturnDate < GETDATE() 
                   AND Returned = 0 AND PendingConfirmation = 0";
            SqlParameter[] parameters = { new SqlParameter("@ClientID", clientId) };
            return Convert.ToInt32(_db.ExecuteScalar(query, parameters)) > 0;
        }

        public (bool Success, List<string> Errors) BorrowBooks(int clientId, List<string> bookIds)
        {
            var errors = new List<string>();
            var user = GetUser(clientId);

            if (user.Type != (int)UserType.Admin && user.Status == (int)Status.Blocked)
            {
                errors.Add("Account is blocked.");
                return (false, errors);
            }

            if (user.Type != (int)UserType.Admin && HasOverdueBooks(clientId))
            {
                errors.Add("Account blocked due to overdue books.");
                return (false, errors);
            }

            int currentBorrowed = GetBorrowedCount(clientId);
            if (user.Type != (int)UserType.Admin &&
                user.BooksQuota - currentBorrowed < bookIds.Count)
            {
                errors.Add($"Quota exceeded. Available: {user.BooksQuota - currentBorrowed}");
                return (false, errors);
            }

            bool anySuccess = false;

            foreach (var bookId in bookIds)
            {
                var book = GetBook(bookId);
                if (book == null)
                {
                    errors.Add($"Book {bookId} not found.");
                    continue;
                }

                if (book.BooksAvailable <= 0)
                {
                    errors.Add($"'{book.Title}' is unavailable.");
                    continue;
                }

                if (user.Type != (int)UserType.Admin &&
                    user.Type == (int)UserType.Student &&
                    book.BorrowType != 0)
                {
                    errors.Add($"'{book.Title}' is teacher-only.");
                    continue;
                }

                if (BorrowBook(clientId, bookId, user.BorrowDuration, book.BorrowDuration))
                {
                    anySuccess = true;
                }
                else
                {
                    errors.Add($"Failed to borrow '{book.Title}'. You may have already borrowed this book.");
                }
            }

            return (anySuccess, errors);
        }

        private bool BorrowBook(int clientId, string bookId, int userDuration, int bookDuration)
        {
            try
            {
                int duration = userDuration > 0 ? userDuration : bookDuration;

                string query = @"
            INSERT INTO Borrow (ClientID, BookID, BorrowDate, ReturnDate, Returned, PendingConfirmation)
            VALUES (@ClientID, @BookID, GETDATE(), DATEADD(day, @Duration, GETDATE()), 0, 0)";

                SqlParameter[] insertParams = {
                    new SqlParameter("@ClientID", clientId),
                    new SqlParameter("@BookID", bookId),
                    new SqlParameter("@Duration", duration)
                };

                _db.ExecuteNonQuery(query, insertParams);

                string updateQuery = "UPDATE Books SET BooksAvailable = BooksAvailable - 1 WHERE BookID = @BookID";
                _db.ExecuteNonQuery(updateQuery, new[] { new SqlParameter("@BookID", bookId) });
                return true;
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool AddBook(Book book)
        {
            if (string.IsNullOrEmpty(book.BookID))
            {
                book.BookID = Guid.NewGuid().ToString();
            }

            string query = @"INSERT INTO Books 
            (BookID, Title, Author, ReleaseDate, BorrowDuration, BooksAvailable, BorrowType)
            VALUES (@BookID, @Title, @Author, @ReleaseDate, @BorrowDuration, @BooksAvailable, @BorrowType)";

            SqlParameter[] parameters = {
                new SqlParameter("@BookID", book.BookID),
                new SqlParameter("@Title", book.Title),
                new SqlParameter("@Author", book.Author),
                new SqlParameter("@ReleaseDate", book.ReleaseDate),
                new SqlParameter("@BorrowDuration", book.BorrowDuration),
                new SqlParameter("@BooksAvailable", book.BooksAvailable),
                new SqlParameter("@BorrowType", book.BorrowType)
            };

            return _db.ExecuteNonQuery(query, parameters);
        }

        public bool UpdateBook(Book book)
        {
            string query = @"UPDATE Books SET 
        Title = @Title,
        Author = @Author,
        ReleaseDate = @ReleaseDate,
        BorrowDuration = @BorrowDuration,
        BooksAvailable = @BooksAvailable,
        BorrowType = @BorrowType
        WHERE BookID = @BookID";

            SqlParameter[] parameters = {
                new SqlParameter("@BookID", book.BookID),
                new SqlParameter("@Title", book.Title),
                new SqlParameter("@Author", book.Author),
                new SqlParameter("@ReleaseDate", book.ReleaseDate),
                new SqlParameter("@BorrowDuration", book.BorrowDuration),
                new SqlParameter("@BooksAvailable", book.BooksAvailable),
                new SqlParameter("@BorrowType", book.BorrowType)
            };

            return _db.ExecuteNonQuery(query, parameters);
        }

        public (bool Success, string ErrorMessage) DeleteBook(string bookId)
        {
            SqlParameter[] checkParams = { new SqlParameter("@BookID", bookId) };
            int borrowedCount = Convert.ToInt32(_db.ExecuteScalar(
                "SELECT COUNT(*) FROM Borrow WHERE BookID = @BookID", checkParams));

            if (borrowedCount > 0)
            {
                return (false, "Book cannot be deleted as it is currently borrowed.");
            }

            SqlParameter[] deleteParams = { new SqlParameter("@BookID", bookId) };
            bool success = _db.ExecuteNonQuery(
                "DELETE FROM Books WHERE BookID = @BookID", deleteParams);

            return (success, success ? "" : "Failed to delete book.");
        }
    }



    public class BorrowInfo
    {
        public int BorrowID { get; set; }
        public int ClientID { get; set; }
        public string ClientName { get; set; }
        public string BookID { get; set; }
        public string Title { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public bool PendingConfirmation { get; set; }
        public bool Returned { get; set; }
    }

    public class Book
    {
        public string BookID { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int BorrowType { get; set; }
        public int BooksAvailable { get; set; }
        public int BorrowDuration { get; set; }
    }

    public enum UserType
    {
        Student = 0,
        Teacher = 1,
        Admin = 2
    }

    public enum Status
    {
        Blocked = 0,
        Active = 1
    }

    [Serializable]
    public class BorrowResult
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    [Serializable]
    public class DeleteBookResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}
