using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SmartProd.API.Server.Data;
using SmartProd.API.Server.DTOs;
using SmartProd.API.Server.Enum;
using SmartProd.API.Server.Hubs;
using SmartProd.API.Server.Models;
using SmartProd.API.Server.Utils;

namespace SmartProd.API.Server.Services
{
    public class ProductionOrderService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ProductionHub>? _hubContext;

        public ProductionOrderService(AppDbContext context, IHubContext<ProductionHub>? hubContext = null)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<object> CreateOrderAsync(OrdemProducaoCreateDto data)
        {
            var product = await _context.Produtos.FindAsync(data.ProductId);
            if (product == null)
                throw new NotFoundException("Produto não encontrado.");

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
                materialId = item.ProdutosId,
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

        public async Task<OrdemProducao?> GetOrderByIdAsync(int id)
        {
            return await _context.OrdensProducao
                .Include(o => o.Produto)
                .Include(o => o.Usuario)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<OrdemProducao> UpdateOrderStatusAsync(int id, OrdemProducaoStatus status, double? produced = null)
        {
            var order = await _context.OrdensProducao
                .Include(o => o.Produto)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                throw new NotFoundException("Ordem de produção não encontrada.");

            order.Status = status;

            if (produced.HasValue)
                order.Produced = produced.Value;

            if (status == OrdemProducaoStatus.CONCLUIDA)
            {
                order.FinishedAt = DateTime.UtcNow;

                if (order.Produto != null)
                {
                    var producedQty = produced ?? order.Quantity;
                    var movimentacao = new Movimentacao
                    {
                        ProductId = order.ProductId,
                        OrderId = order.Id,
                        Tipo = TipoMovimentacao.PRODUCAO,
                        Quantity = producedQty,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Movimentacoes.Add(movimentacao);

                    order.Produto.EstoqueAtual += producedQty;
                    order.Produto.UpdatedAt = DateTime.UtcNow;

                    var stock = await _context.Estoque.FirstOrDefaultAsync(entry => entry.ProductId == order.ProductId);
                    if (stock is null)
                    {
                        _context.Estoque.Add(new Estoque
                        {
                            ProductId = order.ProductId,
                            Quantity = order.Produto.EstoqueAtual,
                            Unit = order.Produto.Unit
                        });
                    }
                    else
                    {
                        stock.Quantity = order.Produto.EstoqueAtual;
                    }

                    // Abate automático dos insumos pela Ficha Técnica (BOM)
                    var bom = await _context.Materiais
                        .Include(b => b.Materials)
                            .ThenInclude(bm => bm.Produtos)
                        .FirstOrDefaultAsync(b => b.ProdutoId == order.ProductId);

                    if (bom?.Materials != null && bom.Materials.Count > 0)
                    {
                        foreach (var item in bom.Materials)
                        {
                            if (item.Produtos != null)
                            {
                                var consumedQty = item.Quantidade * producedQty;
                                item.Produtos.EstoqueAtual = Math.Max(0, item.Produtos.EstoqueAtual - consumedQty);
                                item.Produtos.UpdatedAt = DateTime.UtcNow;

                                _context.Movimentacoes.Add(new Movimentacao
                                {
                                    ProductId = item.ProdutosId,
                                    OrderId = order.Id,
                                    Tipo = TipoMovimentacao.CONSUMO,
                                    Quantity = consumedQty,
                                    CreatedAt = DateTime.UtcNow
                                });

                                var matStock = await _context.Estoque.FirstOrDefaultAsync(entry => entry.ProductId == item.ProdutosId);
                                if (matStock != null)
                                {
                                    matStock.Quantity = item.Produtos.EstoqueAtual;
                                }
                            }
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();

            if (_hubContext != null)
            {
                try
                {
                    await _hubContext.Clients.All.SendAsync("OrderUpdated", new
                    {
                        orderId = order.Id,
                        productId = order.ProductId,
                        status = order.Status.ToString(),
                        produced = order.Produced,
                        finishedAt = order.FinishedAt
                    });
                }
                catch
                {
                    // Falha em telemetria em tempo real não impede persistência transacional
                }
            }

            return order;
        }
    }
}
