using System.ComponentModel.DataAnnotations;

namespace SmartProd.API.Server.DTOs
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string? Email { get; set; }
        [Required]
        [StringLength(128, MinimumLength = 8)]
        public string? Senha { get; set; }
    }
}
