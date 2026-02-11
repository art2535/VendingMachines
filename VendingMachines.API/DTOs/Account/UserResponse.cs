using VendingMachines.API.DTOs.Auth;
using VendingMachines.API.DTOs.Company;
using VendingMachines.API.DTOs.Role;

namespace VendingMachines.API.DTOs.Account
{
    public class UserResponse
    {
        public string LastName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public RoleResponse Role { get; set; } = new();
        public CompanyResponse Company { get; set; } = new();
        public string Token { get; set;} = string.Empty;
    }
}
