using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
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
            await using var transaction = await _context.Database.BeginTransactionAsync();

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
                if (string.IsNullOrWhiteSpace(data.Number))
                    throw new ValidationException("O número da nota é obrigatório.");

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

        public async Task<NotaFiscal> ProcessXmlInvoiceAsync(Stream xmlStream, int usuarioId, string? fileName = null)
        {
            XDocument doc;
            try
            {
                doc = await XDocument.LoadAsync(xmlStream, LoadOptions.None, CancellationToken.None);
            }
            catch (Exception ex)
            {
                throw new ValidationException($"Arquivo XML inválido ou corrompido: {ex.Message}");
            }

            var root = doc.Root;
            if (root == null)
                throw new ValidationException("O arquivo XML da NF-e está vazio.");

            var infNFe = root.Descendants().FirstOrDefault(e => e.Name.LocalName == "infNFe");
            var ide = root.Descendants().FirstOrDefault(e => e.Name.LocalName == "ide");
            if (ide == null)
                throw new ValidationException("Estrutura de NF-e inválida: elemento <ide> não encontrado.");

            var chaveAcesso = infNFe?.Attribute("Id")?.Value?.Replace("NFe", "").Trim() ?? string.Empty;
            var nNF = ide.Elements().FirstOrDefault(e => e.Name.LocalName == "nNF")?.Value?.Trim();
            var tpNFStr = ide.Elements().FirstOrDefault(e => e.Name.LocalName == "tpNF")?.Value?.Trim();
            var dEmiStr = ide.Elements().FirstOrDefault(e => e.Name.LocalName == "dhEmi" || e.Name.LocalName == "dEmi")?.Value;

            if (string.IsNullOrWhiteSpace(nNF))
                throw new ValidationException("Número da nota fiscal (<nNF>) não encontrado no XML.");

            var type = tpNFStr == "0" ? NotaFiscalTipo.ENTRADA : NotaFiscalTipo.SAIDA;

            DateTime date = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(dEmiStr) && DateTime.TryParse(dEmiStr, out var parsedDate))
            {
                date = parsedDate.ToUniversalTime();
            }

            var emit = root.Descendants().FirstOrDefault(e => e.Name.LocalName == "emit");
            var supplier = emit?.Elements().FirstOrDefault(e => e.Name.LocalName == "xNome")?.Value?.Trim() ?? "Fornecedor / Emitente";

            var dest = root.Descendants().FirstOrDefault(e => e.Name.LocalName == "dest");
            var customer = dest?.Elements().FirstOrDefault(e => e.Name.LocalName == "xNome")?.Value?.Trim() ?? "Cliente / Destinatário";

            var detElements = root.Descendants().Where(e => e.Name.LocalName == "det").ToList();
            if (detElements.Count == 0)
                throw new ValidationException("A NF-e não contém nenhum item (<det>).");

            var itemsToProcess = new List<(string code, string name, string unit, double qty, double val)>();

            foreach (var det in detElements)
            {
                var prod = det.Elements().FirstOrDefault(e => e.Name.LocalName == "prod");
                if (prod == null) continue;

                var cProd = prod.Elements().FirstOrDefault(e => e.Name.LocalName == "cProd")?.Value?.Trim() ?? string.Empty;
                var xProd = prod.Elements().FirstOrDefault(e => e.Name.LocalName == "xProd")?.Value?.Trim() ?? "Item NF-e";
                var uCom = prod.Elements().FirstOrDefault(e => e.Name.LocalName == "uCom")?.Value?.Trim() ?? "UN";
                var qComStr = prod.Elements().FirstOrDefault(e => e.Name.LocalName == "qCom")?.Value?.Replace(',', '.');
                var vUnComStr = prod.Elements().FirstOrDefault(e => e.Name.LocalName == "vUnCom" || e.Name.LocalName == "vProd")?.Value?.Replace(',', '.');

                double.TryParse(qComStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var qty);
                double.TryParse(vUnComStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var val);

                if (qty <= 0) qty = 1.0;
                if (string.IsNullOrWhiteSpace(cProd)) cProd = $"PROD-{Math.Abs(xProd.GetHashCode() % 100000)}";

                itemsToProcess.Add((cProd, xProd, uCom, qty, val));
            }

            if (itemsToProcess.Count == 0)
                throw new ValidationException("Nenhum produto válido encontrado no XML da NF-e.");

            var notaNumber = !string.IsNullOrWhiteSpace(chaveAcesso) && chaveAcesso.Length >= 8
                ? $"{nNF} (Chave: {chaveAcesso[..8]}...)"
                : nNF;

            if (await _context.NotasFiscais.AnyAsync(n => n.Number == nNF || n.Number == notaNumber))
                throw new ValidationException($"A NF-e nº {nNF} já foi importada e processada anteriormente.");

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var notaFiscal = new NotaFiscal
                {
                    Type = type,
                    Number = notaNumber,
                    Date = date,
                    Supplier = supplier,
                    Customer = customer,
                    Status = NotaFiscalStatus.PROCESSADA,
                    UsuarioId = usuarioId,
                    Items = new List<NotaFiscalItem>()
                };

                foreach (var item in itemsToProcess)
                {
                    var product = await _context.Produtos
                        .FirstOrDefaultAsync(p => p.Code == item.code || p.Name == item.name);

                    if (product == null)
                    {
                        if (type == NotaFiscalTipo.ENTRADA)
                        {
                            product = new Produto
                            {
                                Code = item.code,
                                Name = item.name,
                                Tipo = TipoProduto.MP,
                                Unit = item.unit.Length <= 5 ? item.unit.ToUpperInvariant() : "UN",
                                EstoqueAtual = 0,
                                EstoqueMínimo = 0,
                                EstoqueReservado = 0,
                                UsuarioId = usuarioId,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };
                            _context.Produtos.Add(product);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            throw new ValidationException($"Produto '{item.name}' ({item.code}) não encontrado para saída fiscal.");
                        }
                    }

                    if (type == NotaFiscalTipo.ENTRADA)
                    {
                        product.EstoqueAtual += item.qty;
                        product.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        if (product.EstoqueAtual < item.qty)
                            throw new ValidationException($"Estoque insuficiente para faturar '{product.Name}'. Atual: {product.EstoqueAtual} | Requerido: {item.qty}");

                        product.EstoqueAtual -= item.qty;
                        product.UpdatedAt = DateTime.UtcNow;
                    }

                    _context.Movimentacoes.Add(new Movimentacao
                    {
                        ProductId = product.Id,
                        Tipo = type == NotaFiscalTipo.ENTRADA ? TipoMovimentacao.ENTRADA : TipoMovimentacao.SAIDA,
                        Quantity = item.qty,
                        CreatedAt = DateTime.UtcNow
                    });

                    var stock = await _context.Estoque.FirstOrDefaultAsync(e => e.ProductId == product.Id);
                    if (stock == null)
                    {
                        _context.Estoque.Add(new Estoque
                        {
                            ProductId = product.Id,
                            Quantity = product.EstoqueAtual,
                            Unit = product.Unit
                        });
                    }
                    else
                    {
                        stock.Quantity = product.EstoqueAtual;
                    }

                    notaFiscal.Items.Add(new NotaFiscalItem
                    {
                        ProductId = product.Id,
                        Quantity = item.qty,
                        Value = item.val
                    });
                }

                _context.NotasFiscais.Add(notaFiscal);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _context.Entry(notaFiscal).Collection(n => n.Items).LoadAsync();
                return notaFiscal;
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
                    .ThenInclude(i => i.Produto)
                .OrderByDescending(n => n.Date)
                .ToListAsync();
        }
    }
}
