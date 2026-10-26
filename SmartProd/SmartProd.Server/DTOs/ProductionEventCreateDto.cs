using System.ComponentModel.DataAnnotations;
using SmartProd.API.Server.Enum;

namespace SmartProd.API.Server.DTOs;

public sealed class ProductionEventCreateDto
{
    [Required]
    public Guid EventId { get; init; }

    [Range(1, int.MaxValue)]
    public int OrderId { get; init; }

    public int? OperationId { get; init; }
    public int? MachineId { get; init; }
    public ProductionEventType Type { get; init; }

    [Range(typeof(decimal), "0", "999999999999999")]
    public decimal GoodQuantity { get; init; }

    [Range(typeof(decimal), "0", "999999999999999")]
    public decimal ScrapQuantity { get; init; }

    [Range(typeof(decimal), "0", "999999999999999")]
    public decimal ReworkQuantity { get; init; }

    [StringLength(100)]
    public string? ReasonCode { get; init; }

    [StringLength(120)]
    public string? DeviceId { get; init; }

    [Range(0, long.MaxValue)]
    public long? DeviceSequence { get; init; }

    public DateTime OccurredAt { get; init; }
}
