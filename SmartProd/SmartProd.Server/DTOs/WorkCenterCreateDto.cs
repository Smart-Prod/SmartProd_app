using System.ComponentModel.DataAnnotations;

namespace SmartProd.API.Server.DTOs;

public sealed class WorkCenterCreateDto
{
    [Required]
    [StringLength(80, MinimumLength = 1)]
    public string Code { get; init; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; init; } = string.Empty;
}

public sealed class MachineCreateDto
{
    [Range(1, int.MaxValue)]
    public int WorkCenterId { get; init; }

    [Required]
    [StringLength(80, MinimumLength = 1)]
    public string Code { get; init; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; init; } = string.Empty;
}

public sealed class ProductionOperationCreateDto
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; init; }

    [Range(1, int.MaxValue)]
    public int WorkCenterId { get; init; }

    [Range(1, int.MaxValue)]
    public int Sequence { get; init; }

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; init; } = string.Empty;

    [Range(typeof(decimal), "0.001", "999999999")]
    public decimal IdealCycleTimeSeconds { get; init; }
}
