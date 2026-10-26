using Microsoft.EntityFrameworkCore;
using SmartProd.API.Server.Data;
using SmartProd.API.Server.DTOs;
using SmartProd.API.Server.Enum;

namespace SmartProd.API.Server.Services
{
    public class DashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardResponseDto> GetDashboardAsync(CancellationToken cancellationToken = default)
        {
            var orders = _context.OrdensProducao.AsNoTracking();
            var products = _context.Produtos.AsNoTracking();
            var totalOrders = await orders.CountAsync(cancellationToken);
            var activeOrders = await orders.CountAsync(
                order => order.Status == OrdemProducaoStatus.EM_PRODUCAO,
                cancellationToken);
            var completedOrders = await orders.CountAsync(
                order => order.Status == OrdemProducaoStatus.CONCLUIDA,
                cancellationToken);
            var totalMovements = await _context.Movimentacoes.CountAsync(cancellationToken);
            var totalProducts = await products.CountAsync(cancellationToken);
            var criticalStockProducts = await products
                .Where(product => product.EstoqueAtual <= product.EstoqueMínimo)
                .OrderBy(product => product.EstoqueAtual - product.EstoqueMínimo)
                .Select(product => new CriticalStockProductDto(
                    product.Id,
                    product.Code,
                    product.Name,
                    product.EstoqueAtual,
                    product.EstoqueMínimo))
                .ToListAsync(cancellationToken);

            var plannedQuantity = await orders.SumAsync(order => (double?)order.Quantity, cancellationToken) ?? 0;
            var producedQuantity = await orders.SumAsync(order => (double?)order.Produced, cancellationToken) ?? 0;
            var efficiency = plannedQuantity > 0
                ? Math.Round(producedQuantity / plannedQuantity * 100, 1)
                : 0;
            var revenue = Convert.ToDecimal(
                await _context.Vendas.AsNoTracking().SumAsync(sale => (double?)sale.TotalValue, cancellationToken) ?? 0);

            var recentMovements = await _context.Movimentacoes
                .AsNoTracking()
                .OrderByDescending(movement => movement.CreatedAt)
                .Take(10)
                .Select(movement => new RecentMovementDto(
                    movement.Id,
                    movement.ProductId,
                    movement.Produto.Name,
                    movement.Tipo.ToString(),
                    movement.Quantity,
                    movement.CreatedAt))
                .ToListAsync(cancellationToken);

            var history = await orders
                .Where(o => o.FinishedAt != null)
                .OrderByDescending(o => o.FinishedAt)
                .Take(10)
                .Select(order => new CompletedOrderHistoryDto(
                    order.Id,
                    order.FinishedAt!.Value,
                    order.Quantity,
                    order.Produced,
                    order.Produced >= order.Quantity ? "CONCLUIDA" : "PARCIAL"))
                .ToListAsync(cancellationToken);

            return new DashboardResponseDto
            {
                TotalOrders = totalOrders,
                ActiveOrders = activeOrders,
                CompletedOrders = completedOrders,
                TotalMovements = totalMovements,
                TotalProducts = totalProducts,
                CriticalStockCount = criticalStockProducts.Count,
                PlannedQuantity = plannedQuantity,
                ProducedQuantity = producedQuantity,
                ProductionEfficiencyPercent = efficiency,
                Revenue = revenue,
                CriticalStockProducts = criticalStockProducts,
                RecentMovements = recentMovements,
                CompletedOrderHistory = history
            };
        }
    }
}
