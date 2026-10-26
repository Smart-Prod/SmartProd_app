using System.ComponentModel.DataAnnotations;

namespace SmartProd.API.Server.DTOs;

public sealed class RefreshTokenDto
{
    [Required]
    [StringLength(256, MinimumLength = 32)]
    public string RefreshToken { get; set; } = string.Empty;
}
