namespace SmartProd.API.Server.Models;

public sealed class ProductionOperation : ITenantOwned
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int ProductId { get; set; }
    public Produto Product { get; set; } = null!;
    public int WorkCenterId { get; set; }
    public WorkCenter WorkCenter { get; set; } = null!;
    public int Sequence { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal IdealCycleTimeSeconds { get; set; }
    public bool IsActive { get; set; } = true;
}
