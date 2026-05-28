using SmartProd.API.Server.DTOs;
using SmartProd.API.Server.Enum;
using Microsoft.EntityFrameworkCore;
using SmartProd.API.Server.Models;
using SmartProd.API.Server.Data;


namespace SmartProd.API.Server.Services
{
    public class MovimentoEstoqueService
    {
        private readonly AppDbContext _context;

        public MovimentoEstoqueService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Movimentacao> CreateMovementAsync(CreateMovimentacaoDto dto)
        {
            var produto = await _context.Produtos.FindAsync(dto.ProductId);
            if (produto == null)
                throw new Exception("Produto não encontrado.");

            if (!System.Enum.TryParse<TipoMovimentacao>(dto.Tipo, true, out var tipo))
                throw new Exception($"Tipo de movimentação inválido: {dto.Tipo}");

            var movimentacao = new Movimentacao
            {
                ProductId = dto.ProductId,
                Tipo = tipo,
                Quantity = dto.Quantity,
                OrderId = dto.OrderId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Movimentacoes.Add(movimentacao);
            await _context.SaveChangesAsync();

            return movimentacao;
        }

        public async Task<RespostaMovimentoEstoque> GetAllMovementsAsync(MovimentacaoFilterDto filters)
        {
            var query = _context.Movimentacoes
                .Include(m => m.Produto)
                .AsQueryable();

            // Filtro por produto
            if (filters.ProductId.HasValue)
                query = query.Where(m => m.ProductId == filters.ProductId);

            // Filtro por tipo
            if (!string.IsNullOrEmpty(filters.Type) && System.Enum.TryParse<TipoMovimentacao>(filters.Type, true, out var tipo))
                query = query.Where(m => m.Tipo == tipo);

            // Filtro datas
            if (filters.StartDate.HasValue)
                query = query.Where(m => m.CreatedAt >= filters.StartDate.Value);
            if (filters.EndDate.HasValue)
                query = query.Where(m => m.CreatedAt <= filters.EndDate.Value);

            // Filtro texto (nome/código)
            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                var search = filters.Search.Trim();
                query = query.Where(m =>
                    m.Produto.Name.Contains(search) ||
                    m.Produto.Code.Contains(search)
                );
            }

            var ordered = await query
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            // Totais agregados por tipo
            var summary = new ResumoMovimentacaoEstoque();
            var grouped = await query
                .GroupBy(m => m.Tipo)
                .Select(g => new { Tipo = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var g in grouped)
            {
                switch (g.Tipo)
                {
                    case TipoMovimentacao.ENTRADA:
                        summary.Entradas = g.Count;
                        break;
                    case TipoMovimentacao.SAIDA:
                        summary.Saidas = g.Count;
                        break;
                    case TipoMovimentacao.PRODUCAO:
                        summary.Producao = g.Count;
                        break;
                    case TipoMovimentacao.CONSUMO:
                        summary.Consumo = g.Count;
                        break;
                }
            }

            // Saldo Líquido
            int saldoLiquido = (summary.Entradas + summary.Producao) - (summary.Saidas + summary.Consumo);

            return new RespostaMovimentoEstoque
            {
                Resumo = summary,
                Total = ordered.Count,
                SaldoLiquido = saldoLiquido,
                Movements = ordered
            };
        }
    }
}
