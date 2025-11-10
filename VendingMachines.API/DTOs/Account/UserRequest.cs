namespace VendingMachines.API.DTOs.Account
{
    public class UserRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string RoleName { get; set; }
        public string Token { get; set; }
    }
}
