namespace SmartProd.API.Server.Models;

public sealed class AuditEvent : ITenantOwned
{
    public long Id { get; set; }
    public int TenantId { get; set; }
    public int? ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
    public string? Reason { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
