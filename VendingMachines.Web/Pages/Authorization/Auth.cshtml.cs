using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text;
using VendingMachines.API.DTOs.Auth;
using VendingMachines.API.DTOs.Account;

namespace VendingMachines.Web.Pages.Authorization
{
    public class AuthModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AuthModel> _logger;

        [BindProperty]
        [Required(ErrorMessage = "Email не может быть пустым")]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Пароль не может быть пустым")]
        public string Password { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public AuthModel(IHttpClientFactory httpClientFactory, ILogger<AuthModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> OnPostLoginAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var httpClient = _httpClientFactory.CreateClient();

            var loginRequest = new LoginRequest
            {
                Email = Email,
                Password = Password
            };

            try
            {
                var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("https://localhost:7270/api/auth/login", content);

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = "Неверный email или пароль.";
                    return Page();
                }

                var json = await response.Content.ReadAsStringAsync();
                var userResponse = JsonSerializer.Deserialize<UserRequest>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (userResponse == null || string.IsNullOrEmpty(userResponse.Token))
                {
                    ErrorMessage = "Ошибка: токен не получен.";
                    return Page();
                }

                HttpContext.Session.SetString("jwt_token", userResponse.Token);
                HttpContext.Session.SetString("user_email", userResponse.Email);
                HttpContext.Session.SetString("user_name", $"{userResponse.LastName} {userResponse.FirstName}");
                HttpContext.Session.SetString("user_role", userResponse.RoleName ?? "");

                if (userResponse.RoleName == "Administrator")
                {
                    return RedirectToPage("/Account/Admin/VendingMachines");
                }
                return RedirectToPage("/Account/Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при авторизации");
                ErrorMessage = "Ощшибка авторизации в системе";
                return Page();
            }
        }

        public IActionResult OnPostRegister()
        {
            return RedirectToPage("/Authorization/Register");
        }
    }
}
