using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibrarySystem_Shared.Models
{
    public class ChatbotResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class ChatInfo
    {
        public string ChatId { get; set; }
        public string Title { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}