using LibrarySystem_Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace LibrarySystem_WebService.Books
{
    public class AnalysisManagement
    {
        private static string CsvFilePath
        {
            get
            {
                if (HttpContext.Current == null)
                {
                    throw new InvalidOperationException("HttpContext is not available. Cannot determine CSV file path for analysis.");
                }
                return HttpContext.Current.Server.MapPath("~/App_Data/ReviewData.csv");
            }
        }

        public List<ReviewDataEntry> GetReviewsFromCsv()
        {
            List<ReviewDataEntry> reviews = new List<ReviewDataEntry>();

            if (!File.Exists(CsvFilePath))
            {
                Console.WriteLine($"Error: ReviewData.csv not found at: {CsvFilePath}");
                return reviews;
            }

            try
            {
                var lines = File.ReadAllLines(CsvFilePath).Skip(1);

                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    if (parts.Length >= 8)
                    {
                        reviews.Add(new ReviewDataEntry
                        {
                            UserID = int.Parse(parts[0]),
                            Positive = int.Parse(parts[1]),
                            Negative = int.Parse(parts[2]),
                            Mixed = int.Parse(parts[3]),
                            Unknown = int.Parse(parts[4]),
                            ReviewDesc = parts[5],
                            Age = int.Parse(parts[6]),
                            BorrowedCount = int.Parse(parts[7])
                        });
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Skipping malformed CSV line: {line}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while reading ReviewData.csv: {ex.Message}");
            }

            return reviews;
        }

        public ReviewSummary GetReviewSentimentSummary()
        {
            var reviews = GetReviewsFromCsv();

            int totalPositive = reviews.Sum(r => r.Positive);
            int totalNegative = reviews.Sum(r => r.Negative);
            int totalMixed = reviews.Sum(r => r.Mixed);
            int totalUnknown = reviews.Sum(r => r.Unknown);

            string overallSentiment = "Mixed";
            string mostCommon = "Mixed";

            var sentimentCounts = new Dictionary<string, int>
            {
                { "Positive", totalPositive },
                { "Negative", totalNegative },
                { "Mixed", totalMixed },
                { "Unknown", totalUnknown }
            };

            if (sentimentCounts.Any())
            {
                mostCommon = sentimentCounts.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
            }

            if (totalPositive > totalNegative && totalPositive > totalMixed && totalPositive > totalUnknown)
            {
                overallSentiment = "Mostly Positive";
            }
            else if (totalNegative > totalPositive && totalNegative > totalMixed && totalNegative > totalUnknown)
            {
                overallSentiment = "Mostly Negative";
            }
            else if (totalMixed > totalPositive && totalMixed > totalNegative && totalMixed > totalUnknown)
            {
                overallSentiment = "Mostly Mixed";
            }
            else if (totalUnknown > totalPositive && totalUnknown > totalNegative && totalUnknown > totalMixed)
            {
                overallSentiment = "Mostly Unknown";
            }

            return new ReviewSummary
            {
                TotalPositive = totalPositive,
                TotalNegative = totalNegative,
                TotalMixed = totalMixed,
                TotalUnknown = totalUnknown,
                OverallSentiment = overallSentiment,
                MostCommonSentiment = mostCommon
            };
        }

        public List<AgeGroupReview> GetReviewsByAgeGroup()
        {
            var reviews = GetReviewsFromCsv();

            var ageGroups = new Dictionary<string, int>
            {
                { "0-17 (Child/Teen)", 0 },
                { "18-24 (Young Adult)", 0 },
                { "25-34 (Adult)", 0 },
                { "35-49 (Middle Age)", 0 },
                { "50+ (Senior)", 0 }
            };

            foreach (var review in reviews)
            {
                if (review.Age >= 0 && review.Age <= 17) ageGroups["0-17 (Child/Teen)"]++;
                else if (review.Age >= 18 && review.Age <= 24) ageGroups["18-24 (Young Adult)"]++;
                else if (review.Age >= 25 && review.Age <= 34) ageGroups["25-34 (Adult)"]++;
                else if (review.Age >= 35 && review.Age <= 49) ageGroups["35-49 (Middle Age)"]++;
                else if (review.Age >= 50) ageGroups["50+ (Senior)"]++;
            }

            return ageGroups.Select(kv => new AgeGroupReview { AgeGroup = kv.Key, ReviewCount = kv.Value }).ToList();
        }

        public List<BorrowedCountReview> GetReviewsByBorrowedCount()
        {
            var reviews = GetReviewsFromCsv();

            var borrowedCountGroups = new Dictionary<string, int>
            {
                { "0-10", 0 },
                { "11-30", 0 },
                { "31-50", 0 },
                { "50+", 0 }
            };

            foreach (var review in reviews)
            {
                if (review.BorrowedCount >= 0 && review.BorrowedCount <= 10) borrowedCountGroups["0-10"]++;
                else if (review.BorrowedCount >= 11 && review.BorrowedCount <= 30) borrowedCountGroups["11-30"]++;
                else if (review.BorrowedCount >= 31 && review.BorrowedCount <= 50) borrowedCountGroups["31-50"]++;
                else if (review.BorrowedCount >= 51) borrowedCountGroups["50+"]++;
            }

            return borrowedCountGroups.Select(kv => new BorrowedCountReview { BorrowedRange = kv.Key, ReviewCount = kv.Value }).ToList();
        }
    }
}
