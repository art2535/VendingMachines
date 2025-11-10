using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using VendingMachines.API.DTOs.Account;
using VendingMachines.Web.Pages.Base;

namespace VendingMachines.Web.Pages.Account
{
    public class ProfileModel : AuthenticatedPageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? RoleName { get; set; }
        public string? ErrorMessage { get; set; }

        public ProfileModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("jwt_token");
            if (string.IsNullOrEmpty(token))
            {
                ErrorMessage = "Токен не найден. Попробуйте снова.";
                return RedirectToPage("/Authorization/Auth");
            }

            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.GetAsync("https://localhost:7270/api/auth/info");
            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = $"Ошибка: {response.StatusCode}";
                return Page();
            }

            var json = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<UserRequest>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (user == null)
            {
                ErrorMessage = "Данные профиля не найдены";
                return Page();
            }

            Email = user.Email;
            FullName = $"{user.LastName} {user.FirstName} {user.MiddleName}".Trim();
            RoleName = string.IsNullOrEmpty(user.RoleName) ? "-" : user.RoleName;

            return Page();
        }
    }
}
