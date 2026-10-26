using System.Security.Claims;

namespace SmartProd.API.Server.Services;

public sealed class HttpTenantContext(IHttpContextAccessor httpContextAccessor, IConfiguration configuration) : ITenantContext
{
    public int TenantId
    {
        get
        {
            var tenantClaim = httpContextAccessor.HttpContext?.User.FindFirstValue("tenant_id");
            if (int.TryParse(tenantClaim, out var tenantId) && tenantId > 0)
                return tenantId;

            return configuration.GetValue("Tenancy:DefaultTenantId", 1);
        }
    }
}
