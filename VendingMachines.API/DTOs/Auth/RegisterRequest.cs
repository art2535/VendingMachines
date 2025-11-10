using System.ComponentModel.DataAnnotations;

namespace VendingMachines.API.DTOs.Auth
{
    public class RegisterRequest
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string RepeatPassword { get; set; } = string.Empty;
    }
}
