using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SmartProd.API.Server.Data;
using SmartProd.API.Server.DTOs;
using SmartProd.API.Server.Enum;
using SmartProd.API.Server.Models;
using SmartProd.API.Server.Services;
using SmartProd.API.Server.Utils;
using Xunit;

namespace SmartProd.Tests.Unit;

public sealed class BomServiceTests
{
    [Fact]
    public async Task CreateAsync_ThrowsWhenProductDoesNotExist()
    {
        await using var context = CreateContext();
        var service = new BomService(context);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CreateAsync(CreateBomDto(productId: 404, materialId: 1)));

        Assert.Equal("Produto não encontrado.", exception.Message);
        Assert.Empty(await context.Materiais.ToListAsync());
    }

    [Fact]
    public async Task CreateAsync_ThrowsWhenMaterialDoesNotExist()
    {
        await using var context = CreateContext();
        context.Produtos.Add(CreateProduct(id: 1, code: "FIN-001"));
        await context.SaveChangesAsync();
        var service = new BomService(context);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            service.CreateAsync(CreateBomDto(productId: 1, materialId: 404)));

        Assert.Equal("Um ou mais materiais não existem ou foram repetidos.", exception.Message);
        Assert.Empty(await context.Materiais.ToListAsync());
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"smartprod-bom-unit-{Guid.NewGuid():N}")
            .Options;
        return new AppDbContext(options);
    }

    private static Produto CreateProduct(int id, string code) => new()
    {
        Id = id,
        Code = code,
        Name = "Test product",
        Tipo = TipoProduto.PA,
        Unit = "un",
        UsuarioId = 1
    };

    private static CreateBomDto CreateBomDto(int productId, int materialId) => new()
    {
        ProductId = productId,
        Materials = [new BomItemDto { MaterialId = materialId, Quantidade = 1 }]
    };
}

public sealed class NotaFiscalServiceTests
{
    [Fact]
    public async Task CreateInvoiceAsync_ThrowsWhenItemListIsEmpty()
    {
        await using var context = CreateContext();
        var service = new NotaFiscalService(context);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            service.CreateInvoiceAsync(new NotaFiscalCreateDto
            {
                Type = NotaFiscalTipo.ENTRADA,
                Number = "INV-001",
                Date = DateTime.UtcNow,
                Items = []
            }, usuarioId: 1));

        Assert.Equal("A nota precisa ter ao menos 1 item.", exception.Message);
        Assert.Empty(await context.NotasFiscais.ToListAsync());
    }

    [Fact]
    public async Task CreateInvoiceAsync_ThrowsWhenItemReferencesMissingProduct()
    {
        await using var context = CreateContext();
        var service = new NotaFiscalService(context);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CreateInvoiceAsync(new NotaFiscalCreateDto
            {
                Type = NotaFiscalTipo.ENTRADA,
                Number = "INV-002",
                Date = DateTime.UtcNow,
                Items = [new NotaFiscalItemCreateDto { ProductId = 404, Quantity = 1, Value = 10 }]
            }, usuarioId: 1));

        Assert.Equal("Algum produto informado não existe.", exception.Message);
        Assert.Empty(await context.NotasFiscais.ToListAsync());
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"smartprod-invoice-unit-{Guid.NewGuid():N}")
            .Options;
        return new AppDbContext(options);
    }
}
