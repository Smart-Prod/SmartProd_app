using System.ComponentModel.DataAnnotations;

namespace SmartProd.API.Server.DTOs
{
    public class RegisterDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string? Name { get; set; }
        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string? Email { get; set; }
        [Required]
        [StringLength(128, MinimumLength = 8)]
        public string? Senha { get; set; }
        [StringLength(100)]
        public string? Department { get; set; }
    }
}
