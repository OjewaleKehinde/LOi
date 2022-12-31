using System;
using System.ComponentModel.DataAnnotations;
using LOi.Models;


namespace LOi.Models
{
    public class User
    {
        [Key]
        public Guid ID { get; set; }
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string PhoneNumber { get; set; } 
        public string Email { get; set; }

        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime TokenCreated { get; set; }
        public DateTime TokenExpires { get; set; }
    }
}