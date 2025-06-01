using LibrarySystem_WebService.Books;
using SchoolSystem.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace LibrarySystem_WebService.Report
{
    public class ReportManagement
    {
        private readonly DatabaseService _db = new DatabaseService();

        public List<BorrowInfo> GetBorrowReport(int? userId, string bookId,
                                      DateTime? fromDate, DateTime? toDate,
                                      bool dueSoon, bool overdue)
        {
            string query = @"
                SELECT b.BorrowID, b.ClientID, c.ClientName, b.BookID, bk.Title, 
                       b.BorrowDate, b.ReturnDate, b.PendingConfirmation, b.Returned
                FROM Borrow b
                INNER JOIN Clients c ON b.ClientID = c.ClientID
                INNER JOIN Books bk ON b.BookID = bk.BookID
                WHERE 1=1";

            var parameters = new List<SqlParameter>();

            if (userId.HasValue)
            {
                query += " AND b.ClientID = @ClientID";
                parameters.Add(new SqlParameter("@ClientID", userId.Value));
            }

            if (!string.IsNullOrEmpty(bookId))
            {
                query += " AND b.BookID = @BookID";
                parameters.Add(new SqlParameter("@BookID", bookId));
            }

            if (fromDate.HasValue)
            {
                query += " AND b.BorrowDate >= @FromDate";
                parameters.Add(new SqlParameter("@FromDate", fromDate.Value));
            }

            if (toDate.HasValue)
            {
                query += " AND b.BorrowDate <= @ToDate";
                parameters.Add(new SqlParameter("@ToDate", toDate.Value));
            }

            if (dueSoon || overdue)
            {
                var conditions = new List<string>();
                if (dueSoon)
                {
                    conditions.Add("(CONVERT(date, b.ReturnDate) BETWEEN @DueSoonStart AND @DueSoonEnd AND b.Returned = 0 AND b.PendingConfirmation = 0)");
                    parameters.Add(new SqlParameter("@DueSoonStart", DateTime.Today));
                    parameters.Add(new SqlParameter("@DueSoonEnd", DateTime.Today.AddDays(1)));
                }
                if (overdue)
                {
                    conditions.Add("(CONVERT(date, b.ReturnDate) < @OverdueDate AND b.Returned = 0 AND b.PendingConfirmation = 0)");
                    parameters.Add(new SqlParameter("@OverdueDate", DateTime.Today));
                }
                query += " AND (" + string.Join(" OR ", conditions) + ")";
            }

            DataTable dt = _db.GetDataN(query, parameters.ToArray());

            List<BorrowInfo> reportData = new List<BorrowInfo>();
            foreach (DataRow row in dt.Rows)
            {
                reportData.Add(new BorrowInfo
                {
                    BorrowID = Convert.ToInt32(row["BorrowID"]),
                    ClientID = Convert.ToInt32(row["ClientID"]),
                    ClientName = row["ClientName"].ToString(),
                    BookID = row["BookID"].ToString(),
                    Title = row["Title"].ToString(),
                    BorrowDate = Convert.ToDateTime(row["BorrowDate"]),
                    ReturnDate = row["ReturnDate"] != DBNull.Value ?
                                 Convert.ToDateTime(row["ReturnDate"]) : (DateTime?)null,
                    PendingConfirmation = Convert.ToBoolean(row["PendingConfirmation"]),
                    Returned = Convert.ToBoolean(row["Returned"])
                });
            }
            return reportData;
        }
    }
}