using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmartProd.API.Server.Data;
using SmartProd.API.Server.DTOs;
using SmartProd.API.Server.Enum;
using SmartProd.API.Server.Models;
using SmartProd.API.Server.Services;
using Xunit;

namespace SmartProd.Tests.Unit;

public sealed class UsuarioServiceRoleTests
{
    [Fact]
    public async Task UpdateUserAsync_PreventsTenantAdminFromGrantingPlatformRole()
    {
        await using var context = CreateContext();
        context.Usuarios.Add(new Usuario
        {
            Name = "Regular user",
            Email = "regular@example.test",
            Senha = "hashed",
            Active = true,
            Role = UserRole.Operator
        });
        await context.SaveChangesAsync();
        var service = new UsuarioService(context, new ConfigurationBuilder().Build());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.UpdateUserAsync(1, new UpdateUserDto { Role = UserRole.SuperAdmin }));

        Assert.Equal(UserRole.Operator, (await context.Usuarios.SingleAsync()).Role);
    }

    [Fact]
    public async Task UpdateUserAsync_PreventsTenantAdminFromChangingPlatformAccount()
    {
        await using var context = CreateContext();
        context.Usuarios.Add(new Usuario
        {
            Name = "Platform administrator",
            Email = "platform@example.test",
            Senha = "hashed",
            Active = true,
            Role = UserRole.SuperAdmin
        });
        await context.SaveChangesAsync();
        var service = new UsuarioService(context, new ConfigurationBuilder().Build());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.UpdateUserAsync(1, new UpdateUserDto { Active = false }));

        Assert.True((await context.Usuarios.SingleAsync()).Active);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"smartprod-user-role-{Guid.NewGuid():N}")
            .Options;
        return new AppDbContext(options);
    }
}
