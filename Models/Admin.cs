using System.ComponentModel.DataAnnotations;

namespace LOi.Models
{
    public class Admin
    {
        [Key]
        public Guid ID { get; set; }

        [Required]
        public string Name { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime TokenCreated { get; set; }
        public DateTime TokenExpires { get; set; }
    }
}