using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibrarySystem.Shared
{
    public class ReviewResult
    {
        public bool Success { get; set; }
        public string Sentiment { get; set; }
        public string Error { get; set; }
    }
}