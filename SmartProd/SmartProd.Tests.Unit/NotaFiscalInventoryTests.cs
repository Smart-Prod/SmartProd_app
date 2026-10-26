using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SmartProd.API.Server.Data;
using SmartProd.API.Server.DTOs;
using SmartProd.API.Server.Enum;
using SmartProd.API.Server.Models;
using SmartProd.API.Server.Services;
using Xunit;

namespace SmartProd.Tests.Unit;

public sealed class NotaFiscalInventoryTests
{
    [Fact]
    public async Task CreateInvoiceAsync_UpdatesProductSnapshotMovementAndInvoiceTogether()
    {
        await using var context = CreateContext();
        var product = new Produto
        {
            Code = "RAW-INV-001",
            Name = "Resin",
            Tipo = TipoProduto.MP,
            Unit = "KG",
            EstoqueAtual = 4,
            UsuarioId = 1
        };
        context.Produtos.Add(product);
        await context.SaveChangesAsync();
        context.Estoque.Add(new Estoque
        {
            ProductId = product.Id,
            Quantity = 4,
            Unit = product.Unit
        });
        await context.SaveChangesAsync();
        var service = new NotaFiscalService(context);

        var invoice = await service.CreateInvoiceAsync(new NotaFiscalCreateDto
        {
            Type = NotaFiscalTipo.ENTRADA,
            Number = "INV-STOCK-001",
            Date = DateTime.UtcNow,
            Items = [new NotaFiscalItemCreateDto { ProductId = product.Id, Quantity = 3, Value = 12.5 }]
        }, usuarioId: 1);

        Assert.Equal(7, (await context.Produtos.SingleAsync()).EstoqueAtual);
        Assert.Equal(7, (await context.Estoque.SingleAsync()).Quantity);
        var movement = await context.Movimentacoes.SingleAsync();
        Assert.Equal(TipoMovimentacao.ENTRADA, movement.Tipo);
        Assert.Equal(3, movement.Quantity);
        Assert.Equal("INV-STOCK-001", invoice.Number);
        Assert.Equal(1, await context.NotaFiscalItems.CountAsync());
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"smartprod-invoice-stock-unit-{Guid.NewGuid():N}")
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new AppDbContext(options);
    }
}
