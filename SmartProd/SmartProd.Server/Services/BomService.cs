using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using SmartProd.API.Server.Data;
using SmartProd.API.Server.DTOs;
using SmartProd.API.Server.Models;
using SmartProd.API.Server.Utils;

namespace SmartProd.API.Server.Services;

public sealed class BomService : IBomService
{
    private readonly AppDbContext _context;
    private static readonly Expression<Func<Materiais, BomResponseDto>> ResponseProjection = bom => new()
    {
        Id = bom.Id,
        ProdutoId = bom.ProdutoId,
        CreatedAt = bom.CreatedAt,
        UpdatedAt = bom.UpdatedAt,
        Produto = new ProdutoSummaryDto
        {
            Id = bom.Produto.Id,
            Code = bom.Produto.Code,
            Name = bom.Produto.Name,
            Tipo = bom.Produto.Tipo.ToString(),
            Unit = bom.Produto.Unit,
            EstoqueAtual = bom.Produto.EstoqueAtual,
            EstoqueReservado = bom.Produto.EstoqueReservado,
            EstoqueMínimo = bom.Produto.EstoqueMínimo,
            UsuarioId = bom.Produto.UsuarioId,
            CreatedAt = bom.Produto.CreatedAt,
            UpdatedAt = bom.Produto.UpdatedAt
        },
        Materials = bom.Materials.Select(item => new BomItemResponseDto
        {
            Id = item.Id,
            MateriaisId = item.MateriaisId,
            ProdutosId = item.ProdutosId,
            Quantidade = item.Quantidade,
            Produtos = new ProdutoSummaryDto
            {
                Id = item.Produtos.Id,
                Code = item.Produtos.Code,
                Name = item.Produtos.Name,
                Tipo = item.Produtos.Tipo.ToString(),
                Unit = item.Produtos.Unit,
                EstoqueAtual = item.Produtos.EstoqueAtual,
                EstoqueReservado = item.Produtos.EstoqueReservado,
                EstoqueMínimo = item.Produtos.EstoqueMínimo,
                UsuarioId = item.Produtos.UsuarioId,
                CreatedAt = item.Produtos.CreatedAt,
                UpdatedAt = item.Produtos.UpdatedAt
            }
        }).ToList()
    };

    public BomService(AppDbContext context) => _context = context;

    public Task<List<BomResponseDto>> GetAllAsync() => _context.Materiais
        .Select(ResponseProjection)
        .ToListAsync();

    public Task<BomResponseDto?> GetByIdAsync(int id) => _context.Materiais
        .Where(bom => bom.Id == id)
        .Select(ResponseProjection)
        .FirstOrDefaultAsync();

    public async Task<BomResponseDto> CreateAsync(CreateBomDto dto)
    {
        if (!await _context.Produtos.AnyAsync(product => product.Id == dto.ProductId))
            throw new NotFoundException("Produto não encontrado.");

        var materialIds = dto.Materials.Select(item => item.MaterialId).Distinct().ToArray();
        if (materialIds.Length != dto.Materials.Count ||
            await _context.Produtos.CountAsync(product => materialIds.Contains(product.Id)) != materialIds.Length)
            throw new ValidationException("Um ou mais materiais não existem ou foram repetidos.");

        var bom = new Materiais
        {
            ProdutoId = dto.ProductId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Materials = dto.Materials.Select(item => new MateriaisItems
            {
                ProdutosId = item.MaterialId,
                Quantidade = item.Quantidade
            }).ToList()
        };

        _context.Materiais.Add(bom);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(bom.Id)
            ?? throw new NotFoundException("A estrutura criada não pôde ser carregada.");
    }

}
