namespace SmartProd.API.Server.Models;

public sealed class Lot : ITenantOwned
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int ProductId { get; set; }
    public Produto Product { get; set; } = null!;
    public int? ProductionOrderId { get; set; }
    public OrdemProducao? ProductionOrder { get; set; }
    public string Number { get; set; } = string.Empty;
    public string? SupplierLotNumber { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal QuantityRemaining { get; set; }
    public decimal? UnitCost { get; set; }
    public DateTime? ManufacturedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
