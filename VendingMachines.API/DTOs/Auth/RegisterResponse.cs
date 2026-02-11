using VendingMachines.API.DTOs.Account;

namespace VendingMachines.API.DTOs.Auth
{
    public class RegisterResponse
    {
        public UserResponse User { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }
}
