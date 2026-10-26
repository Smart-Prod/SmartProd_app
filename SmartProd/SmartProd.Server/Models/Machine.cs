namespace SmartProd.API.Server.Models;

public sealed class Machine : ITenantOwned
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int WorkCenterId { get; set; }
    public WorkCenter WorkCenter { get; set; } = null!;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "Available";
    public bool IsActive { get; set; } = true;
}
