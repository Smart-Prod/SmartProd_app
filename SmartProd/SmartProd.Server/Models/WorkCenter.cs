namespace SmartProd.API.Server.Models;

public sealed class WorkCenter : ITenantOwned
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
