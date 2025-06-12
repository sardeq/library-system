using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace LibrarySystem_Shared.Models
{
    [Serializable]
    public class ReviewDataEntry
    {
        public int UserID { get; set; }
        public int Positive { get; set; }
        public int Negative { get; set; }
        public int Mixed { get; set; }
        public int Unknown { get; set; }
        public string ReviewDesc { get; set; }
        public int Age { get; set; }
        public int BorrowedCount { get; set; }
    }

    public class ReviewSummary
    {
        public int TotalPositive { get; set; }
        public int TotalNegative { get; set; }
        public int TotalMixed { get; set; }
        public int TotalUnknown { get; set; }
        public string OverallSentiment { get; set; }
        public string MostCommonSentiment { get; set; }
    }

    public class AgeGroupReview
    {
        public string AgeGroup { get; set; }
        public int ReviewCount { get; set; }
    }

    public class BorrowedCountReview
    {
        public string BorrowedRange { get; set; }
        public int ReviewCount { get; set; }
    }
}
