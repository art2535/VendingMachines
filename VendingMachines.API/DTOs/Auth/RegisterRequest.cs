using System.ComponentModel.DataAnnotations;

namespace VendingMachines.API.DTOs.Auth
{
    public class RegisterRequest
    {
        public string LastName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Language { get; set; }
        public string Password { get; set; } = string.Empty;
    }
}
