using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using VendingMachines.API.DTOs.Auth;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;

namespace VendingMachines.Web.Pages.Authorization
{
    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty]
        [Required(ErrorMessage = "Email не может быть пустым")]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Пароль не может быть пустым")]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Повторите пароль")]
        public string RepeatPassword { get; set; } = string.Empty;

        public string? ErrorMessage { get; private set; } = string.Empty;

        public RegisterModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnPostRegister()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (Password != RepeatPassword)
            {
                ErrorMessage = "Пароли не совпадают";
                return Page();
            }

            try
            {
                using (var httpClient = _httpClientFactory.CreateClient())
                {
                    var registerRequest = new RegisterRequest
                    {
                        Email = Email,
                        Password = Password,
                        RepeatPassword = RepeatPassword
                    };

                    var content = new StringContent(JsonSerializer.Serialize(registerRequest),
                        Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync("https://localhost:7270/api/auth/register", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Ошибка сервера - {(int)response.StatusCode}");
                    }

                    return RedirectToPage("/Account/Main");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return Page();
            }
        }

        public IActionResult OnPostLogin()
        {
            return RedirectToPage("/Authorization/Auth");
        }
    }
}
