using System.ComponentModel.DataAnnotations;

namespace TechnicalTaskAPI.Application.Identity.Models
{
    public class RegistrationRequest
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

    }
}
