using System.ComponentModel.DataAnnotations;

namespace VendingMachines.API.DTOs.Auth
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Фамилия обязательна")]
        public string LastName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Имя обязательно")]
        public string FirstName { get; set; } = string.Empty;
        
        public string? MiddleName { get; set; }
        
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Неверный формат email")]
        public string Email { get; set; } = string.Empty;
        
        public string? Phone { get; set; }
        
        public int? RoleId { get; set; }
        
        public int? CompanyId { get; set; }
        
        public string? Language { get; set; }

        [Required(ErrorMessage = "Пароль обязателен")]
        [MinLength(6, ErrorMessage = "Пароль должен содержать минимум 6 символов")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Повтор пароля обязателен")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string RepeatPassword { get; set; } = string.Empty;
    }
}
