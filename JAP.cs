using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Jap
{
    public class JAPClient
    {
        private readonly string _key;
        private readonly string _endpoint;

        public JAPClient(string key)
        {
            _key = key;
            _endpoint = "https://justanotherpanel.com/api/v2";
        }

        public async Task<List<Service>> ListServicesAsync()
        {
            return await PostAsync<List<Service>>("key", _key, "action", "services");
        }

        public async Task<string> AddOrderAsync(string service, string link, int quantity, int? runs = null, int? interval = null)
        {
            return await PostAsync<string>("key", _key, "action", "add",
                "service", service,
                "link", link,
                "quantity", quantity,
                "runs", runs,
                "interval", interval);
        }

        public async Task<OrderStatusResponse> GetOrderStatusAsync(string orderID)
        {
            return await PostAsync<OrderStatusResponse>(("key", _key), "action", "status", "order", orderID);
        }

        public async Task<UserBalanceResponse> GetUserBalanceAsync()
        {
            return await PostAsync<UserBalanceResponse>("key", _key, "action", "balance");
        }

        public async Task<string> RedditUpvoteAsync(string link, int quantity)
        {
            return await AddOrderAsync("6228", link, quantity);
        }

        private async Task<T> PostAsync<T>(params (string, string)[] data)
        {
            var content = new FormUrlEncodedContent(data);
            var response = await PostAsync<T>(content);
            return response;
        }

        private async Task<T> PostAsync<T>(HttpContent content)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_endpoint),
                Content = content
            };

            using var client = new HttpClient();
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStreamAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = await JsonSerializer.DeserializeAsync<T>(responseContent, options);

            return result;
        }
    }

    public class Service
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public string Rate { get; set; }
        public string Min { get; set; }
        public string Max { get; set; }
        public bool Refill { get; set; }
        public bool Cancel { get; set; }
    }

    public class OrderStatusResponse
    {
        public Dictionary<string, OrderStatus> OrderStatus { get; set; }
    }

    public class OrderStatus
    {
        public string Charge { get; set; }
        public string StartCount { get; set; }
        public string Status { get; set; }
        public string Remains { get; set; }
        public string Currency { get; set; }
        public string Error { get; set; }
    }

    public class UserBalanceResponse
    {
        public string Balance { get; set; }
        public string Currency { get; set; }
    }
}