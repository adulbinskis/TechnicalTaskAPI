namespace TechnicalTaskAPI.Application.Identity.Models
{
    public class TokenResponse
    {
        public string? Token { get; set; }
        public DateTime? TokenExpirationDate { get; set; }
    }
}
