using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using VendingMachines.API.DTOs.Account;
using VendingMachines.API.DTOs.Auth;
using VendingMachines.API.DTOs.Monitoring;
using VendingMachines.Core.Models;

namespace VendingMachines.Desktop.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:7270/api/";

        public ApiService(string jwtToken)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl)
            };
            if (!string.IsNullOrEmpty(jwtToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            }
        }

        public async Task<NetworkStatusResponse?> GetNetworkStatusAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("monitoring/network-status");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<NetworkStatusResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<SummaryResponse?> GetSummaryAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("monitoring/summary");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<SummaryResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<SaleTrend>?> GetSalesTrendAsync(DateTime startDate, DateTime endDate, bool byAmount)
        {
            try
            {
                var query = $"monitoring/sales-trend?startDate={startDate:yyyy-MM-dd}" +
                    $"&endDate={endDate:yyyy-MM-dd}&byAmount={byAmount}";

                var response = await _httpClient.GetAsync(query);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<SaleTrend>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<Notification>?> GetNotificationsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("monitoring/notifications?limit=100");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Notification>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<UserRequest?> LoginAsync(string email, string password)
        {
            try
            {
                var loginRequest = new LoginRequest
                {
                    Email = email,
                    Password = password
                };

                var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("auth/login", content);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<UserRequest>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
