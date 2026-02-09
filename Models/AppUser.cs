using System.ComponentModel.DataAnnotations;

namespace trial.Models
{
    public class AppUser
    {
        public int UserId { get; set; }
        public required string Username { get; set; }
        public required byte[] PasswordHash { get; set; }
        public required byte[] PasswordSalt { get; set; }
        
        // NEW: Role Property
        public required string Role { get; set; } 
    }
}