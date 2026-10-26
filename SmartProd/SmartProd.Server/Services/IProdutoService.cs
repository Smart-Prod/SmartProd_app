using SmartProd.API.Server.DTOs;
using SmartProd.API.Server.Models;

namespace SmartProd.API.Server.Services;

public interface IProdutoService
{
    Task<Produto> CreateProductAsync(ProdutoCreateDto dto, int usuarioId);
    Task<List<Produto>> GetAllProductsAsync();
    Task<Produto> GetProductByIdAsync(int id);
    Task<Produto> UpdateProductAsync(int id, ProdutoCreateDto dto);
    Task<Produto?> GetProductByCodeAsync(string code);
    Task DeleteProductAsync(int id);
}
