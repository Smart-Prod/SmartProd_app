using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SmartProd.API.Server.DTOs;
using SmartProd.API.Server.Enum;
using SmartProd.API.Server.Models;
using SmartProd.API.Server.Data;
using SmartProd.API.Server.Utils;

namespace SmartProd.API.Server.Services
{
    public class NotaFiscalService
    {
        private readonly AppDbContext _context;
        public NotaFiscalService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<NotaFiscal> CreateInvoiceAsync(NotaFiscalCreateDto data, int usuarioId)
        {
            if (data.Items == null || data.Items.Count == 0)
                throw new ValidationException("A nota precisa ter ao menos 1 item.");
            if (data.Type is not NotaFiscalTipo.ENTRADA and not NotaFiscalTipo.SAIDA)
                throw new ValidationException("Tipo de nota fiscal inválido.");
            if (data.Items.Any(item =>
                    item.Quantity <= 0 || !double.IsFinite(item.Quantity) ||
                    item.Value < 0 || !double.IsFinite(item.Value)))
                throw new ValidationException("Quantidade ou valor de item inválido.");

            // 1️⃣ Buscar produtos usados na nota
            var productIds = data.Items.Select(x => x.ProductId).Distinct().ToList();

            var products = await _context.Produtos
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            if (products.Count != productIds.Count)
                throw new NotFoundException("Algum produto informado não existe.");

            // 2️⃣ Validar regras MP/PA
            foreach (var item in data.Items)
            {
                var prod = products.First(p => p.Id == item.ProductId);

                if (data.Type == NotaFiscalTipo.ENTRADA && prod.Tipo != TipoProduto.MP)
                    throw new ValidationException($"Produto {prod.Name} não é MP. Somente MP aparece em nota de entrada.");

                if (data.Type == NotaFiscalTipo.SAIDA && prod.Tipo != TipoProduto.PA)
                    throw new ValidationException($"Produto {prod.Name} não é PA. Somente PA aparece em nota de saída.");
            }

            // 3️⃣ Iniciar transação ANTES de alterar estoque (garante atomicidade)
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 4️⃣ Atualizar estoque dentro da transação
                foreach (var item in data.Items)
                {
                    var prod = products.First(p => p.Id == item.ProductId);
                    if (data.Type == NotaFiscalTipo.ENTRADA)
                        prod.EstoqueAtual += item.Quantity;
                    else if (data.Type == NotaFiscalTipo.SAIDA)
                    {
                        if (prod.EstoqueAtual < item.Quantity)
                            throw new ValidationException(
                                $"Estoque insuficiente do produto {prod.Name}. Atual: {prod.EstoqueAtual} | Necessário: {item.Quantity}");
                        prod.EstoqueAtual -= item.Quantity;
                    }

                    _context.Produtos.Update(prod);

                    var stock = await _context.Estoque
                        .FirstOrDefaultAsync(entry => entry.ProductId == prod.Id);
                    if (stock is null)
                    {
                        _context.Estoque.Add(new Estoque
                        {
                            ProductId = prod.Id,
                            Quantity = prod.EstoqueAtual,
                            Unit = prod.Unit
                        });
                    }
                    else
                    {
                        stock.Quantity = prod.EstoqueAtual;
                    }

                    _context.Movimentacoes.Add(new Movimentacao
                    {
                        ProductId = prod.Id,
                        Tipo = data.Type == NotaFiscalTipo.ENTRADA
                            ? TipoMovimentacao.ENTRADA
                            : TipoMovimentacao.SAIDA,
                        Quantity = item.Quantity,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                // 5️⃣ Criar a nota fiscal + items
                // Validar campos obrigatórios
                if (string.IsNullOrWhiteSpace(data.Number))
                    throw new ValidationException("O número da nota é obrigatório.");

                // Verifica unicidade do número da nota
                if (await _context.NotasFiscais.AnyAsync(x => x.Number == data.Number))
                    throw new ValidationException("O número da nota já existe.");

                var notaFiscal = new NotaFiscal
                {
                    Type = data.Type,
                    Number = data.Number,
                    Date = data.Date,
                    UsuarioId = usuarioId,
                    Supplier = data.Supplier,
                    Customer = data.Customer,
                    Status = NotaFiscalStatus.PROCESSADA,
                    Items = new List<NotaFiscalItem>()
                };

                foreach (var item in data.Items)
                {
                    notaFiscal.Items.Add(new NotaFiscalItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Value = item.Value
                    });
                }

                _context.NotasFiscais.Add(notaFiscal);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Inclui Items ao retornar
                await _context.Entry(notaFiscal).Collection(n => n.Items).LoadAsync();

                return notaFiscal;
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true)
            {
                await transaction.RollbackAsync();
                throw new ValidationException("O número da nota já existe.");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<NotaFiscal>> GetAllInvoicesAsync()
        {
            return await _context.NotasFiscais
                .Include(n => n.Items)
                .ToListAsync();
        }
    }
}
