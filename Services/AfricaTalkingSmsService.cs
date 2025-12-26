using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace ThikaResQNet.Services
{
    public class AfricaTalkingSmsService : ISmsService
    {
        private readonly string _username;
        private readonly string _apiKey;
        private readonly string _from;
        private readonly string _baseUrl;
        private readonly IHttpClientFactory _httpFactory;

        public AfricaTalkingSmsService(IConfiguration config, IHttpClientFactory httpFactory)
        {
            var section = config.GetSection("AfricaTalking");
            _username = section.GetValue<string>("Username");
            _apiKey = section.GetValue<string>("ApiKey");
            _from = section.GetValue<string>("From");
            _baseUrl = section.GetValue<string>("BaseUrl") ?? "https://api.africastalking.com";
            _httpFactory = httpFactory;
        }

        public async Task<bool> SendSmsAsync(IEnumerable<string> toPhoneNumbers, string message)
        {
            var client = _httpFactory.CreateClient();
            client.BaseAddress = new Uri(_baseUrl);
            client.DefaultRequestHeaders.Add("apiKey", _apiKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var to = string.Join(",", toPhoneNumbers);
            var payload = new Dictionary<string, string>
            {
                { "username", _username },
                { "to", to },
                { "message", message },
                { "from", _from }
            };

            var content = new FormUrlEncodedContent(payload);
            var resp = await client.PostAsync("/version1/messaging", content);
            if (!resp.IsSuccessStatusCode) return false;
            var body = await resp.Content.ReadAsStringAsync();
            // Optionally inspect response
            return true;
        }

        public Task HandleInboundSmsAsync(string from, string text)
        {
            // Basic inbound SMS handling: create an incident from SMS text or log
            // In production, validate and parse message format, lookup sender to user mapping, etc.
            // For now, just log or store a simple incident — repository/service injection would be needed.
            Console.WriteLine($"Inbound SMS from {from}: {text}");
            return Task.CompletedTask;
        }
    }
}