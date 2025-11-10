using System.ComponentModel.DataAnnotations;

namespace VendingMachines.API.DTOs.Auth
{
    public class ExitRequest
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Token { get; set; }
    }
}
