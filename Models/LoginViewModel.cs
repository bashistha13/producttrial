using System.ComponentModel.DataAnnotations;

namespace trial.Models
{
      public class LoginViewModel
    {
        [Required]
        public required string Username { get; set; }
        
        [Required]
        public  required string Password { get; set; }
    }
}
