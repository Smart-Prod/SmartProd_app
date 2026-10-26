using System.ComponentModel.DataAnnotations;
using SmartProd.API.Server.DTOs;
using SmartProd.API.Server.Enum;
using Microsoft.EntityFrameworkCore;
using SmartProd.API.Server.Models;
using SmartProd.API.Server.Data;
using SmartProd.API.Server.Utils;


namespace SmartProd.API.Server.Services
{
    public class MovimentoEstoqueService
    {
        private readonly AppDbContext _context;

        public MovimentoEstoqueService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(Movimentacao movement, Produto product)> CreateConsumptionAsync(ConsumoRequestDto dto)
        {
            if (dto.Quantity <= 0)
                throw new ValidationException("A quantidade precisa ser maior que zero.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            var product = await _context.Produtos.FirstOrDefaultAsync(p => p.Code == dto.Code)
                ?? throw new NotFoundException("Produto não encontrado.");
            if (product.Tipo != TipoProduto.MP)
                throw new ValidationException("Somente matéria-prima pode ser registrada como consumo.");
            if (product.EstoqueAtual < dto.Quantity)
                throw new ValidationException($"Estoque insuficiente para consumir {product.Name}.");
            if (dto.OrderId.HasValue && !await _context.OrdensProducao.AnyAsync(order => order.Id == dto.OrderId.Value))
                throw new NotFoundException("Ordem de produção não encontrada.");

            var movimentacao = new Movimentacao
            {
                ProductId = product.Id,
                Tipo = TipoMovimentacao.CONSUMO,
                Quantity = dto.Quantity,
                OrderId = dto.OrderId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Movimentacoes.Add(movimentacao);

            await ApplyStockDeltaAsync(product, -dto.Quantity);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (movimentacao, product);
        }

        public async Task<(Movimentacao movement, Produto product)> CompleteOrderAsync(ConsumoRequestDto dto)
        {
            if (dto.Quantity <= 0)
                throw new ValidationException("A quantidade precisa ser maior que zero.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            var product = await _context.Produtos.FirstOrDefaultAsync(p => p.Code == dto.Code)
                ?? throw new NotFoundException("Produto não encontrado.");
            if (product.Tipo != TipoProduto.PA)
                throw new ValidationException("Somente produto acabado pode ser apontado como produção.");
            if (dto.OrderId.HasValue)
            {
                var order = await _context.OrdensProducao
                    .FirstOrDefaultAsync(candidate => candidate.Id == dto.OrderId.Value)
                    ?? throw new NotFoundException("Ordem de produção não encontrada.");
                if (order.ProductId != product.Id)
                    throw new ValidationException("A ordem de produção não corresponde ao produto informado.");
            }

            var movimentacao = new Movimentacao
            {
                ProductId = product.Id,
                Tipo = TipoMovimentacao.PRODUCAO,
                Quantity = dto.Quantity,
                OrderId = dto.OrderId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Movimentacoes.Add(movimentacao);

            await ApplyStockDeltaAsync(product, dto.Quantity);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (movimentacao, product);
        }

        public async Task<Movimentacao> CreateMovementAsync(CreateMovimentacaoDto dto)
        {
            var produto = await _context.Produtos.FindAsync(dto.ProductId);
            if (produto == null)
                throw new NotFoundException("Produto não encontrado.");

            if (dto.Quantity <= 0)
                throw new ValidationException("A quantidade precisa ser maior que zero.");
            if (!System.Enum.TryParse<TipoMovimentacao>(dto.Tipo, true, out var tipo))
                throw new ValidationException($"Tipo de movimentação inválido: {dto.Tipo}");

            var stockDelta = tipo switch
            {
                TipoMovimentacao.ENTRADA or TipoMovimentacao.PRODUCAO => dto.Quantity,
                TipoMovimentacao.SAIDA or TipoMovimentacao.CONSUMO => -dto.Quantity,
                _ => throw new ValidationException($"Tipo de movimentação não suportado: {tipo}")
            };
            if (stockDelta < 0 && produto.EstoqueAtual < -stockDelta)
                throw new ValidationException($"Estoque insuficiente para o produto {produto.Name}.");
            if (dto.OrderId.HasValue)
            {
                var order = await _context.OrdensProducao
                    .FirstOrDefaultAsync(candidate => candidate.Id == dto.OrderId.Value)
                    ?? throw new NotFoundException("Ordem de produção não encontrada.");
                if (tipo == TipoMovimentacao.PRODUCAO && order.ProductId != produto.Id)
                    throw new ValidationException("A ordem de produção não corresponde ao produto informado.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            var movimentacao = new Movimentacao
            {
                ProductId = dto.ProductId,
                Tipo = tipo,
                Quantity = dto.Quantity,
                OrderId = dto.OrderId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Movimentacoes.Add(movimentacao);
            await ApplyStockDeltaAsync(produto, stockDelta);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return movimentacao;
        }

        private async Task ApplyStockDeltaAsync(Produto product, double delta)
        {
            var stock = await _context.Estoque.FirstOrDefaultAsync(entry => entry.ProductId == product.Id);
            var newBalance = product.EstoqueAtual + delta;
            if (newBalance < 0)
                throw new ValidationException("O saldo de estoque não pode ficar negativo.");
            if (stock is null)
            {
                _context.Estoque.Add(new Estoque
                {
                    ProductId = product.Id,
                    Quantity = newBalance,
                    Unit = product.Unit
                });
            }
            else
                stock.Quantity = newBalance;

            product.EstoqueAtual = newBalance;
            product.UpdatedAt = DateTime.UtcNow;
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
