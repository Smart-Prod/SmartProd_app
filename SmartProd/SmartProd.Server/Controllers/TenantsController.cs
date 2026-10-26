using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartProd.API.Server.DTOs;
using SmartProd.API.Server.Services;

namespace SmartProd.API.Server.Controllers;

[ApiController]
[Route("api/v1/tenants")]
[Authorize(Roles = "SuperAdmin")]
public sealed class TenantsController(TenantProvisioningService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] TenantProvisionDto dto,
        CancellationToken cancellationToken)
    {
        var tenant = await service.CreateTenantAsync(dto, cancellationToken);
        return Created(string.Empty, new
        {
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            tenant.IsActive,
            tenant.CreatedAt
        });
    }
}
