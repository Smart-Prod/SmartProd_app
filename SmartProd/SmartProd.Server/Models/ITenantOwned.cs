namespace SmartProd.API.Server.Models;

public interface ITenantOwned
{
    int TenantId { get; set; }
}
