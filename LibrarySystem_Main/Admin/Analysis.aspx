<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Analysis.aspx.cs" 
    Inherits="LibrarySystem_Main.Admin.Analysis" MasterPageFile="~/Site.Master" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <h2 class="mb-4">Review Analysis</h2>

        <div class="card shadow-sm mb-4 rounded-lg">
            <div class="card-header bg-primary text-white rounded-t-lg">
                <h4 class="mb-0">Overall Review Sentiment</h4>
            </div>
            <div class="card-body p-4">
                <div id="sentimentSummary" class="text-center">
                    <p class="h5">Loading sentiment data...</p>
                </div>
                <div class="row mt-3 text-center">
                    <div class="col-md-3 col-sm-6 mb-2">
                        <div class="p-2 border rounded-lg bg-success-light shadow-sm">
                            <strong>Positive:</strong> <span id="positiveCount">0</span>
                        </div>
                    </div>
                    <div class="col-md-3 col-sm-6 mb-2">
                        <div class="p-2 border rounded-lg bg-danger-light shadow-sm">
                            <strong>Negative:</strong> <span id="negativeCount">0</span>
                        </div>
                    </div>
                    <div class="col-md-3 col-sm-6 mb-2">
                        <div class="p-2 border rounded-lg bg-warning-light shadow-sm">
                            <strong>Mixed:</strong> <span id="mixedCount">0</span>
                        </div>
                    </div>
                    <div class="col-md-3 col-sm-6 mb-2">
                        <div class="p-2 border rounded-lg bg-secondary-light shadow-sm">
                            <strong>Unknown:</strong> <span id="unknownCount">0</span>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="row">
            <div class="col-md-6 mb-4">
                <div class="card shadow-sm rounded-lg h-100">
                    <div class="card-header bg-info text-white rounded-t-lg">
                        <h4 class="mb-0">Reviews by Age Group</h4>
                    </div>
                    <div class="card-body p-4 d-flex align-items-center justify-content-center position-relative">
                        <canvas id="ageGroupChart"></canvas>
                        <p id="ageChartMessage" class="chart-message text-center text-muted hidden">No age group data available.</p>
                    </div>
                </div>
            </div>

            <div class="col-md-6 mb-4">
                <div class="card shadow-sm rounded-lg h-100">
                    <div class="card-header bg-success text-white rounded-t-lg">
                        <h4 class="mb-0">Reviews by Borrowed Count</h4>
                    </div>
                    <div class="card-body p-4 d-flex align-items-center justify-content-center position-relative">
                        <canvas id="borrowedCountChart"></canvas>
                        <p id="borrowedChartMessage" class="chart-message text-center text-muted hidden">No borrowed count data available.</p>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.1/dist/chart.umd.min.js"></script>

    <script>
        var sentimentSummaryData = <%= SentimentSummaryJson %>;
        var ageGroupData = <%= AgeGroupDataJson %>;
        var borrowedCountData = <%= BorrowedCountDataJson %>;

        function showMessage(canvasId, messageId, showMessage) {
            const canvas = document.getElementById(canvasId);
            const message = document.getElementById(messageId);
            if (canvas && message) {
                canvas.style.display = showMessage ? 'none' : 'block';
                message.classList.toggle('hidden', !showMessage);
            }
        }

        document.addEventListener('DOMContentLoaded', function () {
            const sentimentSummaryElement = document.getElementById('sentimentSummary');
            if (sentimentSummaryData && sentimentSummaryData.OverallSentiment) {
                sentimentSummaryElement.innerHTML = `<p class="h5">Overall, reviews are <strong class="text-primary">${sentimentSummaryData.OverallSentiment}</strong>.</p>`;
                document.getElementById('positiveCount').innerText = sentimentSummaryData.TotalPositive;
                document.getElementById('negativeCount').innerText = sentimentSummaryData.TotalNegative;
                document.getElementById('mixedCount').innerText = sentimentSummaryData.TotalMixed;
                document.getElementById('unknownCount').innerText = sentimentSummaryData.TotalUnknown;
            } else {
                sentimentSummaryElement.innerHTML = `<p class="h5 text-danger">Failed to load sentiment summary.</p>`;
            }

            const ageGroupChartCanvas = document.getElementById('ageGroupChart');
            const ageChartMessage = document.getElementById('ageChartMessage');
            const hasAgeData = ageGroupData && ageGroupData.length > 0 && ageGroupData.some(d => d.ReviewCount > 0);

            if (hasAgeData) {
                showMessage('ageGroupChart', 'ageChartMessage', false);
                const ageCtx = ageGroupChartCanvas.getContext('2d');
                new Chart(ageCtx, {
                    type: 'bar', 
                    data: {
                        labels: ageGroupData.map(data => data.AgeGroup),
                        datasets: [{
                            label: 'Number of Reviews',
                            data: ageGroupData.map(data => data.ReviewCount), 
                            backgroundColor: [
                                'rgba(255, 99, 132, 0.7)',
                                'rgba(54, 162, 235, 0.7)',
                                'rgba(255, 206, 86, 0.7)',
                                'rgba(75, 192, 192, 0.7)',
                                'rgba(153, 102, 255, 0.7)'
                            ],
                            borderColor: [
                                'rgba(255, 99, 132, 1)',
                                'rgba(54, 162, 235, 1)',
                                'rgba(255, 206, 86, 1)',
                                'rgba(75, 192, 192, 1)',
                                'rgba(153, 102, 255, 1)'
                            ],
                            borderWidth: 1
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: {
                                display: false
                            },
                            title: {
                                display: true,
                                text: 'Review Counts by Age Group'
                            }
                        },
                        scales: {
                            y: {
                                beginAtZero: true,
                                title: {
                                    display: true,
                                    text: 'Number of Reviews'
                                },
                                ticks: {
                                    stepSize: 1 // Ensure Y-axis ticks are whole numbers for counts
                                }
                            },
                            x: {
                                title: {
                                    display: true,
                                    text: 'Age Group' // X-axis label
                                }
                            }
                        }
                    }
                });
            } else {
                showMessage('ageGroupChart', 'ageChartMessage', true); 
            }

            const borrowedCountChartCanvas = document.getElementById('borrowedCountChart');
            const borrowedChartMessage = document.getElementById('borrowedChartMessage');
            const hasBorrowedData = borrowedCountData && borrowedCountData.length > 0 && borrowedCountData.some(d => d.ReviewCount > 0);

            if (hasBorrowedData) {
                showMessage('borrowedCountChart', 'borrowedChartMessage', false);
                const borrowedCtx = borrowedCountChartCanvas.getContext('2d');
                new Chart(borrowedCtx, {
                    type: 'pie', 
                    data: {
                        labels: borrowedCountData.map(data => data.BorrowedRange), 
                        datasets: [{
                            label: 'Number of Reviews',
                            data: borrowedCountData.map(data => data.ReviewCount), 
                            backgroundColor: [
                                'rgba(255, 159, 64, 0.7)',
                                'rgba(255, 99, 132, 0.7)',
                                'rgba(75, 192, 192, 0.7)', 
                                'rgba(153, 102, 255, 0.7)'
                            ],
                            borderColor: [
                                'rgba(255, 159, 64, 1)',
                                'rgba(255, 99, 132, 1)',
                                'rgba(75, 192, 192, 1)',
                                'rgba(153, 102, 255, 1)'
                            ],
                            borderWidth: 1
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false, 
                        plugins: {
                            legend: {
                                position: 'top', 
                            },
                            title: {
                                display: true,
                                text: 'Review Counts by Borrowed Book Range' 
                            }
                        }
                    }
                });
            } else {
                showMessage('borrowedCountChart', 'borrowedChartMessage', true);
            }
        });
    </script>
</asp:Content>

