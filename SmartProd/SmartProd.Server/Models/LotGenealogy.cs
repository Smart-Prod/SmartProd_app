namespace SmartProd.API.Server.Models;

public sealed class LotGenealogy : ITenantOwned
{
    public long Id { get; set; }
    public int TenantId { get; set; }
    public int InputLotId { get; set; }
    public Lot InputLot { get; set; } = null!;
    public int OutputLotId { get; set; }
    public Lot OutputLot { get; set; } = null!;
    public int ProductionOrderId { get; set; }
    public OrdemProducao ProductionOrder { get; set; } = null!;
    public decimal QuantityConsumed { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}
