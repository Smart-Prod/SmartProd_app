using SmartProd.API.Server.Enum;

namespace SmartProd.API.Server.Models;

public sealed class ProductionEvent : ITenantOwned
{
    public long Id { get; set; }
    public int TenantId { get; set; }
    public Guid EventId { get; set; } = Guid.NewGuid();
    public int OrderId { get; set; }
    public OrdemProducao Order { get; set; } = null!;
    public int? OperationId { get; set; }
    public ProductionOperation? Operation { get; set; }
    public int? MachineId { get; set; }
    public Machine? Machine { get; set; }
    public int? OperatorId { get; set; }
    public Usuario? Operator { get; set; }
    public ProductionEventType Type { get; set; }
    public decimal GoodQuantity { get; set; }
    public decimal ScrapQuantity { get; set; }
    public decimal ReworkQuantity { get; set; }
    public string? ReasonCode { get; set; }
    public string? DeviceId { get; set; }
    public long? DeviceSequence { get; set; }
    public DateTime OccurredAt { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
