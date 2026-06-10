using Microsoft.EntityFrameworkCore;
using SmartProd.API.Server.Data;
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

        public async Task<object> GetDashboardAsync()
        {
            var totalOrdens = await _context.OrdensProducao.CountAsync();
            var activeOps = await _context.OrdensProducao.CountAsync(o => o.Status == OrdemProducaoStatus.EM_PRODUCAO);
            var completedOps = await _context.OrdensProducao.CountAsync(o => o.Status == OrdemProducaoStatus.CONCLUIDA);
            var totalMovimentacoes = await _context.Movimentacoes.CountAsync();
            var totalProdutos = await _context.Produtos.CountAsync();
            var lowStock = await _context.Produtos.CountAsync(p => p.EstoqueAtual <= p.EstoqueMínimo);

            var totalPlanned = await _context.OrdensProducao.SumAsync(o => (double?)o.Quantity) ?? 0;
            var totalProduced = await _context.OrdensProducao.SumAsync(o => (double?)o.Produced) ?? 0;
            var efficiency = totalPlanned > 0 ? Math.Round((totalProduced / totalPlanned) * 100, 1) : 0;

            var history = await _context.OrdensProducao
                .Where(o => o.FinishedAt != null)
                .OrderByDescending(o => o.FinishedAt)
                .Take(10)
                .Select(o => new
                {
                    date = o.FinishedAt!.Value.ToString("dd/MM"),
                    throughput = o.Produced.ToString("F0"),
                    status = o.Produced >= o.Quantity ? "CONCLUÍDO" : "PARCIAL"
                })
                .ToListAsync();

            return new
            {
                activeOps,
                totalOps = totalOrdens,
                efficiency = $"{efficiency}%",
                efficiencyTrend = efficiency >= 80 ? "+2.5%" : "-1.2%",
                throughput = $"{totalProduced:F0} unid.",
                oeeTarget = 85.0,
                alertsCount = lowStock,
                alertMessage = lowStock > 0 ? $"{lowStock} produto(s) com estoque baixo" : "Nenhum alerta",
                currentShift = "Turno A",
                shiftProgress = "67%",
                lineEfficiency = $"{efficiency}%",
                latency = "320ms",
                traceability = "100%",
                oeeMeta = "85%",
                oeeGap = $"{85 - efficiency:F1}%",
                history
            };
        }
    }
}
