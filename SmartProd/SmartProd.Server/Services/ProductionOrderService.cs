using Microsoft.EntityFrameworkCore;
using SmartProd.API.Server.Data;
using SmartProd.API.Server.DTOs;
using SmartProd.API.Server.Enum;
using SmartProd.API.Server.Models;

namespace SmartProd.API.Server.Services
{
    public class ProductionOrderService
    {
        private readonly AppDbContext _context;
        public ProductionOrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<object> CreateOrderAsync(OrdemProducaoCreateDto data)
        {
            var product = await _context.Produtos.FindAsync(data.ProductId);
            if (product == null)
                throw new System.Exception("Produto não encontrado.");

            var order = new OrdemProducao
            {
                ProductId = data.ProductId,
                UsuarioId = data.UsuarioId,
                Quantity = data.Quantity,
                Status = OrdemProducaoStatus.PLANEJADA,
                Produced = 0,
                CreatedAt = System.DateTime.UtcNow
            };

            _context.OrdensProducao.Add(order);
            await _context.SaveChangesAsync();

            var bom = await _context.Materiais
                .Include(b => b.Materials)
                    .ThenInclude(bm => bm.Produtos)
                .FirstOrDefaultAsync(b => b.ProdutoId == data.ProductId);

            if (bom == null)
            {
                return new
                {
                    order,
                    materialsNeeded = new List<object>()
                };
            }

            var materialsNeeded = bom.Materials.Select(item => new
            {
                materialId = item.MaterialId,
                materialName = item.Produtos?.Name ?? "Desconhecido",
                quantityPerUnit = item.Quantidade,
                totalQuantity = item.Quantidade * data.Quantity
            }).ToList();

            return new
            {
                order,
                materialsNeeded
            };
        }

        public async Task<List<OrdemProducao>> GetAllOrdersAsync()
        {
            return await _context.OrdensProducao
                .Include(o => o.Produto)
                .Include(o => o.Usuario)
                .ToListAsync();
        }

        public async Task<OrdemProducao> UpdateOrderStatusAsync(int id, OrdemProducaoStatus status, double? produced = null)
        {
            var order = await _context.OrdensProducao
                .Include(o => o.Produto)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                throw new Exception("Ordem de produção não encontrada.");

            order.Status = status;

            if (produced.HasValue)
                order.Produced = produced.Value;

            if (status == OrdemProducaoStatus.CONCLUIDA)
            {
                order.FinishedAt = DateTime.UtcNow;

                if (order.Produto != null)
                {
                    var movimentacao = new Movimentacao
                    {
                        ProductId = order.ProductId,
                        OrderId = order.Id,
                        Tipo = TipoMovimentacao.PRODUCAO,
                        Quantity = produced ?? order.Quantity,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Movimentacoes.Add(movimentacao);
                }
            }

            await _context.SaveChangesAsync();
            return order;
        }
    }
}
