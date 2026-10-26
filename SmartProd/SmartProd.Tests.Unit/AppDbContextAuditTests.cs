using Microsoft.EntityFrameworkCore;
using SmartProd.API.Server.Data;
using SmartProd.API.Server.Enum;
using SmartProd.API.Server.Models;
using Xunit;

namespace SmartProd.Tests.Unit;

public sealed class AppDbContextAuditTests
{
    [Fact]
    public async Task SaveChangesAsyncWithoutAcceptingChanges_PreservesUpdatedValuesAndModifiedState()
    {
        await using var context = CreateContext();
        var product = new Produto
        {
            Code = "AUD-001",
            Name = "Audited product",
            Tipo = TipoProduto.PA,
            Unit = "UN",
            EstoqueAtual = 10,
            UsuarioId = 1
        };
        context.Produtos.Add(product);
        await context.SaveChangesAsync();

        product.EstoqueAtual = 7;
        await context.SaveChangesAsync(acceptAllChangesOnSuccess: false);

        Assert.Equal(7, product.EstoqueAtual);
        Assert.Equal(EntityState.Modified, context.Entry(product).State);
        Assert.Equal(10, context.Entry(product).Property(item => item.EstoqueAtual).OriginalValue);
        Assert.Equal(2, await context.AuditEvents.CountAsync());
    }

    [Fact]
    public async Task SaveChangesAsync_DoesNotPersistSensitiveUserFieldsInAuditJson()
    {
        await using var context = CreateContext();
        context.Usuarios.Add(new Usuario
        {
            Name = "Audit user",
            Email = "audit@example.test",
            Senha = "sensitive-password",
            RefreshTokenHash = "sensitive-token",
            Active = true,
            Role = UserRole.Admin
        });

        await context.SaveChangesAsync();

        var audit = await context.AuditEvents.SingleAsync();
        Assert.DoesNotContain("sensitive-password", audit.AfterJson);
        Assert.DoesNotContain("sensitive-token", audit.AfterJson);
        Assert.DoesNotContain("Senha", audit.AfterJson);
        Assert.DoesNotContain("RefreshTokenHash", audit.AfterJson);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"smartprod-audit-unit-{Guid.NewGuid():N}")
            .Options;
        return new AppDbContext(options);
    }
}
