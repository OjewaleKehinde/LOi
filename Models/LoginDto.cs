using System.ComponentModel.DataAnnotations;

namespace LOi.Models
{
    public class LoginDto
    {
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}