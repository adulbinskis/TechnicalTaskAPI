namespace TechnicalTaskAPI.Application.Identity.Models
{
    public class AuthResponse
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
    }
}
