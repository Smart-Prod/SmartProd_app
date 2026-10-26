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

public sealed class ProdutoServiceTests
{
    [Fact]
    public async Task CreateProductAsync_ThrowsWhenCodeAlreadyExists()
    {
        await using var context = CreateContext();
        context.Produtos.Add(new Produto
        {
            Code = "PRD-001",
            Name = "Existing item",
            Unit = "un",
            UsuarioId = 1
        });
        await context.SaveChangesAsync();
        var service = new ProdutoService(context);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            service.CreateProductAsync(CreateProductDto("PRD-001"), usuarioId: 1));

        Assert.Equal("Código de produto já está em uso.", exception.Message);
    }

    [Fact]
    public async Task GetProductByIdAsync_ThrowsWhenProductDoesNotExist()
    {
        await using var context = CreateContext();
        var service = new ProdutoService(context);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetProductByIdAsync(id: 404));

        Assert.Equal("Produto não encontrado.", exception.Message);
    }

    [Fact]
    public async Task CreateProductAsync_SavesProductWithOwner()
    {
        await using var context = CreateContext();
        context.Usuarios.Add(new Usuario
        {
            Name = "Test user",
            Email = "unit-test@example.test",
            Senha = "not-a-real-password",
            Active = true,
            Role = UserRole.Admin
        });
        await context.SaveChangesAsync();
        var service = new ProdutoService(context);

        var product = await service.CreateProductAsync(CreateProductDto("PRD-002"), usuarioId: 1);

        Assert.Equal("PRD-002", product.Code);
        Assert.Equal("Test item", product.Name);
        Assert.Equal(1, product.UsuarioId);
        Assert.Equal(1, await context.Produtos.CountAsync());
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"smartprod-unit-{Guid.NewGuid():N}")
            .Options;
        return new AppDbContext(options);
    }

    private static ProdutoCreateDto CreateProductDto(string code) => new()
    {
        Code = code,
        Name = "Test item",
        Tipo = TipoProduto.PA,
        Unit = "un"
    };
}
