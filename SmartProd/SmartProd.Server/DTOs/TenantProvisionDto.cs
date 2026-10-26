using System.ComponentModel.DataAnnotations;

namespace SmartProd.API.Server.DTOs;

public sealed class TenantProvisionDto
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [StringLength(80, MinimumLength = 2)]
    [RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    public string Slug { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string AdminName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string AdminEmail { get; init; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 12)]
    public string AdminPassword { get; init; } = string.Empty;
}
