using LibrarySystem_Shared.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace LibrarySystem_Main.Admin
{
    public partial class Analysis : BasePage
    {
        public string SentimentSummaryJson { get; set; } = "null";
        public string AgeGroupDataJson { get; set; } = "[]";
        public string BorrowedCountDataJson { get; set; } = "[]";

        protected async void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {
                await LoadAnalysisData();
            }
        }

        private async Task LoadAnalysisData()
        {
            try
            {
                var sentimentResponse = await APIClient.Instance.GetAsync("api/analysis/sentiment-summary");
                if (sentimentResponse.IsSuccessStatusCode)
                {
                    var summary = JsonConvert.DeserializeObject<ReviewSummary>(await sentimentResponse.Content.ReadAsStringAsync());
                    SentimentSummaryJson = JsonConvert.SerializeObject(summary);
                }
                else
                {
                    Console.WriteLine($"Error fetching sentiment summary: {sentimentResponse.ReasonPhrase} - {await sentimentResponse.Content.ReadAsStringAsync()}");
                    SentimentSummaryJson = JsonConvert.SerializeObject(new ReviewSummary { OverallSentiment = "Error loading data.", MostCommonSentiment = "N/A" });
                }

                var ageResponse = await APIClient.Instance.GetAsync("api/analysis/reviews-by-age");
                if (ageResponse.IsSuccessStatusCode)
                {
                    var ageData = JsonConvert.DeserializeObject<List<AgeGroupReview>>(await ageResponse.Content.ReadAsStringAsync());
                    AgeGroupDataJson = JsonConvert.SerializeObject(ageData);
                }
                else
                {
                    Console.WriteLine($"Error fetching age group data: {ageResponse.ReasonPhrase} - {await ageResponse.Content.ReadAsStringAsync()}");
                    AgeGroupDataJson = "[]";
                }

                var borrowedResponse = await APIClient.Instance.GetAsync("api/analysis/reviews-by-borrowed-count");
                if (borrowedResponse.IsSuccessStatusCode)
                {
                    var borrowedData = JsonConvert.DeserializeObject<List<BorrowedCountReview>>(await borrowedResponse.Content.ReadAsStringAsync());
                    BorrowedCountDataJson = JsonConvert.SerializeObject(borrowedData);
                }
                else
                {
                    Console.WriteLine($"Error fetching borrowed count data: {borrowedResponse.ReasonPhrase} - {await borrowedResponse.Content.ReadAsStringAsync()}");
                    BorrowedCountDataJson = "[]";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unhandled error occurred while loading analysis data: {ex.Message}");
                SentimentSummaryJson = JsonConvert.SerializeObject(new ReviewSummary { OverallSentiment = "Failed to load data.", MostCommonSentiment = "N/A" });
                AgeGroupDataJson = "[]";
                BorrowedCountDataJson = "[]";
            }
        }
    }
}
