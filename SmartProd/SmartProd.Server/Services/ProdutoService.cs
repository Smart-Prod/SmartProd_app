using Microsoft.EntityFrameworkCore;
using SmartProd.API.Server.Data;
using SmartProd.API.Server.DTOs;
using SmartProd.API.Server.Models;

namespace SmartProd.API.Server.Services
{
    public class ProdutoService
    {
        private readonly AppDbContext _context;

        public ProdutoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Produto> CreateProductAsync(ProdutoCreateDto dto, int usuarioId)
        {
            if (await _context.Produtos.AnyAsync(p => p.Code == dto.Code))
                throw new Exception("Código de produto já está em uso.");

            var produto = new Produto
            {
                Code = dto.Code,
                Name = dto.Name,
                Tipo = dto.Tipo,
                Unit = dto.Unit,
                EstoqueAtual = dto.EstoqueAtual,
                EstoqueReservado = dto.EstoqueReservado,
                EstoqueMínimo = dto.EstoqueMínimo,
                UsuarioId = usuarioId
            };

            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();
            return produto;
        }

        public async Task<List<Produto>> GetAllProductsAsync()
        {
            return await _context.Produtos
                .Include(p => p.Bom)
                .ToListAsync();
        }

        public async Task<Produto> GetProductByIdAsync(int id)
        {
            var produto = await _context.Produtos
                .Include(p => p.Bom)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (produto == null)
                throw new Exception("Produto não encontrado.");
            return produto;
        }

        public async Task<Produto> UpdateProductAsync(int id, ProdutoCreateDto dto)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null)
                throw new Exception("Produto não encontrado.");

            if (!string.IsNullOrWhiteSpace(dto.Code))
                produto.Code = dto.Code;
            if (!string.IsNullOrWhiteSpace(dto.Name))
                produto.Name = dto.Name;
            produto.Tipo = dto.Tipo;
            if (!string.IsNullOrWhiteSpace(dto.Unit))
                produto.Unit = dto.Unit;
            produto.EstoqueAtual = dto.EstoqueAtual;
            produto.EstoqueReservado = dto.EstoqueReservado;
            produto.EstoqueMínimo = dto.EstoqueMínimo;
            produto.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return produto;
        }

        public async Task DeleteProductAsync(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null)
                throw new Exception("Produto não encontrado.");

            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();
        }
    }
}
