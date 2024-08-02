namespace TechnicalTaskAPI.Application.Identity.Models
{
    public class AuthResponseWithTokens : AuthResponse
    {
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpirationDate { get; set; }
        public DateTime? AccessTokenExpirationDate { get; set; }

    }
}
