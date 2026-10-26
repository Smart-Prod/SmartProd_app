using System.ComponentModel.DataAnnotations;
using SmartProd.API.Server.Enum;

namespace SmartProd.API.Server.DTOs;

public sealed class LotCreateDto
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; init; }

    [Required]
    [StringLength(120, MinimumLength = 1)]
    public string Number { get; init; } = string.Empty;

    public LotKind Kind { get; init; }

    [Range(typeof(decimal), "0.001", "999999999999999")]
    public decimal Quantity { get; init; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? UnitCost { get; init; }

    [StringLength(120)]
    public string? SupplierLotNumber { get; init; }

    public DateTime? ExpiresAt { get; init; }
    public int? ProductionOrderId { get; init; }
}

public sealed class LotConsumptionDto
{
    [Range(1, int.MaxValue)]
    public int InputLotId { get; init; }

    [Range(1, int.MaxValue)]
    public int OutputLotId { get; init; }

    [Range(1, int.MaxValue)]
    public int ProductionOrderId { get; init; }

    [Range(typeof(decimal), "0.001", "999999999999999")]
    public decimal Quantity { get; init; }
}
