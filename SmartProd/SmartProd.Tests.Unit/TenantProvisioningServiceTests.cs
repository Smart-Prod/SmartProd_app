using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SmartProd.API.Server.Data;
using SmartProd.API.Server.DTOs;
using SmartProd.API.Server.Enum;
using SmartProd.API.Server.Services;
using SmartProd.API.Server.Utils;
using Xunit;

namespace SmartProd.Tests.Unit;

public sealed class TenantProvisioningServiceTests
{
    [Fact]
    public async Task CreateTenantAsync_AtomicallyCreatesTenantAdministratorAndAuditTrail()
    {
        await using var context = CreateContext();
        var service = new TenantProvisioningService(context);

        var tenant = await service.CreateTenantAsync(new TenantProvisionDto
        {
            Name = " Factory One ",
            Slug = "factory-one",
            AdminName = "Tenant Admin",
            AdminEmail = "admin@factory-one.test",
            AdminPassword = "a-strong-password-123"
        }, CancellationToken.None);

        var admin = await context.Usuarios.IgnoreQueryFilters().SingleAsync();
        Assert.Equal("Factory One", tenant.Name);
        Assert.Equal(tenant.Id, admin.TenantId);
        Assert.Equal(UserRole.Admin, admin.Role);
        Assert.True(BCrypt.Net.BCrypt.Verify("a-strong-password-123", admin.Senha));
        Assert.Contains(await context.AuditEvents.ToListAsync(), audit =>
            audit.TenantId == tenant.Id && audit.EntityType == "Tenant" && audit.Action == "Provisioned");
    }

    [Fact]
    public async Task CreateTenantAsync_RejectsDuplicateSlug()
    {
        await using var context = CreateContext();
        context.Tenants.Add(new()
        {
            Name = "Existing",
            Slug = "existing"
        });
        await context.SaveChangesAsync();
        var service = new TenantProvisioningService(context);

        await Assert.ThrowsAsync<ConflictException>(() => service.CreateTenantAsync(new TenantProvisionDto
        {
            Name = "Another",
            Slug = "existing",
            AdminName = "Admin",
            AdminEmail = "admin@another.test",
            AdminPassword = "a-strong-password-123"
        }, CancellationToken.None));

        Assert.Empty(await context.Usuarios.IgnoreQueryFilters().ToListAsync());
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"smartprod-tenant-provisioning-{Guid.NewGuid():N}")
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new AppDbContext(options);
    }
}
