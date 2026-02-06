namespace trial.Models
{
    public class AppUser
    {
        public int UserId { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}