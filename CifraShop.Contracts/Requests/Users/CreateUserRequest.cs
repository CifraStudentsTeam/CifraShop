using System.ComponentModel.DataAnnotations;

namespace CifraShop.Contracts.Requests.Users
{
    public class CreateUserRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        public string Branch { get; set; } = string.Empty;
    }
}