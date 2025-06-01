using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace LibrarySystem_Main
{
    public static class APIClient
    {
        private static readonly Lazy<HttpClient> _instance = new Lazy<HttpClient>(() =>
        {
            var client = new HttpClient();

            client.BaseAddress = new Uri("https://localhost:44343/");

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        });

        public static HttpClient Instance => _instance.Value;
    }
}