using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SmartProd.API.Server.Data;
using SmartProd.API.Server.DTOs;
using SmartProd.API.Server.Enum;
using SmartProd.API.Server.Models;
using SmartProd.API.Server.Services;
using Xunit;

namespace SmartProd.Tests.Unit;

public sealed class MovimentoEstoqueServiceTests
{
    [Fact]
    public async Task CreateConsumptionAsync_UpdatesProductAndStockSnapshotAtomically()
    {
        await using var context = CreateContext();
        var product = CreateRawMaterial();
        context.Produtos.Add(product);
        await context.SaveChangesAsync();
        context.Estoque.Add(new Estoque { ProductId = product.Id, Quantity = 10, Unit = product.Unit });
        await context.SaveChangesAsync();
        var service = new MovimentoEstoqueService(context);

        var (movement, updatedProduct) = await service.CreateConsumptionAsync(new ConsumoRequestDto
        {
            Code = product.Code,
            Quantity = 3
        });

        Assert.Equal(TipoMovimentacao.CONSUMO, movement.Tipo);
        Assert.Equal(7, updatedProduct.EstoqueAtual);
        Assert.Equal(7, await context.Estoque.Where(stock => stock.ProductId == product.Id)
            .Select(stock => stock.Quantity).SingleAsync());
        Assert.Single(await context.Movimentacoes.ToListAsync());
    }

    [Fact]
    public async Task CreateConsumptionAsync_RejectsInsufficientStockWithoutWritingMovement()
    {
        await using var context = CreateContext();
        var product = CreateRawMaterial(stock: 2);
        context.Produtos.Add(product);
        await context.SaveChangesAsync();
        context.Estoque.Add(new Estoque { ProductId = product.Id, Quantity = 2, Unit = product.Unit });
        await context.SaveChangesAsync();
        var service = new MovimentoEstoqueService(context);

        await Assert.ThrowsAsync<ValidationException>(() => service.CreateConsumptionAsync(new ConsumoRequestDto
        {
            Code = product.Code,
            Quantity = 3
        }));

        Assert.Equal(2, (await context.Produtos.SingleAsync()).EstoqueAtual);
        Assert.Equal(2, (await context.Estoque.SingleAsync()).Quantity);
        Assert.Empty(await context.Movimentacoes.ToListAsync());
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"smartprod-stock-unit-{Guid.NewGuid():N}")
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new AppDbContext(options);
    }

    private static Produto CreateRawMaterial(double stock = 10) => new()
    {
        Code = "RAW-001",
        Name = "Raw material",
        Tipo = TipoProduto.MP,
        Unit = "KG",
        EstoqueAtual = stock,
        UsuarioId = 1
    };
}
