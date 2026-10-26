using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartProd.API.Server.Data;

namespace SmartProd.API.Server.Services;

public sealed class ReportExportService(AppDbContext context)
{
    public async Task<MemoryStream> ExportProductsCsvAsync(CancellationToken cancellationToken = default)
    {
        var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream, new UTF8Encoding(true), 1024, leaveOpen: true);
        await writer.WriteLineAsync("Id,Código,Produto,Tipo,Unidade,Estoque atual,Estoque reservado,Estoque mínimo");

        await foreach (var product in context.Produtos
                           .AsNoTracking()
                           .OrderBy(product => product.Name)
                           .Select(product => new
                           {
                               product.Id,
                               product.Code,
                               product.Name,
                               product.Tipo,
                               product.Unit,
                               product.EstoqueAtual,
                               product.EstoqueReservado,
                               product.EstoqueMínimo
                           })
                           .AsAsyncEnumerable()
                           .WithCancellation(cancellationToken))
        {
            await writer.WriteLineAsync(string.Join(',',
                product.Id.ToString(CultureInfo.InvariantCulture),
                EscapeCsv(product.Code),
                EscapeCsv(product.Name),
                EscapeCsv(product.Tipo.ToString()),
                EscapeCsv(product.Unit),
                product.EstoqueAtual.ToString("G", CultureInfo.InvariantCulture),
                product.EstoqueReservado.ToString("G", CultureInfo.InvariantCulture),
                product.EstoqueMínimo.ToString("G", CultureInfo.InvariantCulture)));
        }

        await writer.FlushAsync(cancellationToken);
        stream.Position = 0;
        return stream;
    }

    public async Task<MemoryStream> ExportMovementsCsvAsync(CancellationToken cancellationToken = default)
    {
        var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream, new UTF8Encoding(true), 1024, leaveOpen: true);
        await writer.WriteLineAsync("Id,Produto,Nome do produto,Tipo,Quantidade,Data");

        await foreach (var movement in context.Movimentacoes
                           .AsNoTracking()
                           .OrderByDescending(movement => movement.CreatedAt)
                           .Select(movement => new
                           {
                               movement.Id,
                               movement.Produto.Code,
                               ProductName = movement.Produto.Name,
                               movement.Tipo,
                               movement.Quantity,
                               movement.CreatedAt
                           })
                           .AsAsyncEnumerable()
                           .WithCancellation(cancellationToken))
        {
            await writer.WriteLineAsync(string.Join(',',
                movement.Id.ToString(CultureInfo.InvariantCulture),
                EscapeCsv(movement.Code),
                EscapeCsv(movement.ProductName),
                EscapeCsv(movement.Tipo.ToString()),
                movement.Quantity.ToString("G", CultureInfo.InvariantCulture),
                EscapeCsv(movement.CreatedAt.ToString("O", CultureInfo.InvariantCulture))));
        }

        await writer.FlushAsync(cancellationToken);
        stream.Position = 0;
        return stream;
    }

    public async Task<byte[]> CreateInventoryPdfAsync(CancellationToken cancellationToken = default)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var products = await context.Produtos
            .AsNoTracking()
            .OrderBy(product => product.Name)
            .Select(product => new
            {
                product.Code,
                product.Name,
                Type = product.Tipo.ToString(),
                product.Unit,
                product.EstoqueAtual,
                product.EstoqueMínimo
            })
            .ToListAsync(cancellationToken);

        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Margin(32);
                page.Header().Column(header =>
                {
                    header.Item().Text("Relatório de estoque").FontSize(22).Bold().FontColor("#FF8C00");
                    header.Item().Text($"Gerado em {DateTimeOffset.UtcNow:dd/MM/yyyy HH:mm} UTC").FontSize(9).FontColor(Colors.Grey.Darken1);
                });
                page.Content().PaddingVertical(18).Column(content =>
                {
                    content.Spacing(7);
                    content.Item().Text($"Produtos cadastrados: {products.Count}").Bold();
                    foreach (var product in products)
                    {
                        var critical = product.EstoqueAtual <= product.EstoqueMínimo ? " - ESTOQUE CRÍTICO" : string.Empty;
                        content.Item()
                            .BorderBottom(1)
                            .BorderColor(Colors.Grey.Lighten2)
                            .PaddingVertical(5)
                            .Text($"{product.Code} | {product.Name} ({product.Type}) | {product.EstoqueAtual:G} {product.Unit} / mínimo {product.EstoqueMínimo:G}{critical}");
                    }
                });
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("SmartProd • Página ").FontSize(9).FontColor(Colors.Grey.Darken1);
                    text.CurrentPageNumber().FontSize(9).FontColor(Colors.Grey.Darken1);
                });
            });
        }).GeneratePdf();
    }

    public async Task<byte[]> CreateBomPdfAsync(CancellationToken cancellationToken = default)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var boms = await context.Materiais
            .AsNoTracking()
            .Include(bom => bom.Produto)
            .Include(bom => bom.Materials)
                .ThenInclude(item => item.Produtos)
            .OrderBy(bom => bom.Produto.Name)
            .ToListAsync(cancellationToken);

        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Margin(32);
                page.Header().Column(header =>
                {
                    header.Item().Text("Estruturas de produtos (BOM)").FontSize(22).Bold().FontColor("#FF8C00");
                    header.Item().Text($"Gerado em {DateTimeOffset.UtcNow:dd/MM/yyyy HH:mm} UTC").FontSize(9).FontColor(Colors.Grey.Darken1);
                });
                page.Content().PaddingVertical(18).Column(content =>
                {
                    content.Spacing(12);
                    foreach (var bom in boms)
                    {
                        content.Item().Column(recipe =>
                        {
                            recipe.Item().Text($"{bom.Produto.Code} | {bom.Produto.Name}").Bold().FontSize(13);
                            if (bom.Materials.Count == 0)
                            {
                                recipe.Item().PaddingLeft(12).Text("Nenhum componente cadastrado").FontColor(Colors.Grey.Darken1);
                            }
                            foreach (var item in bom.Materials.OrderBy(item => item.Produtos.Name))
                            {
                                recipe.Item().PaddingLeft(12).Text(
                                    $"{item.Produtos.Code} | {item.Produtos.Name}: {item.Quantidade.ToString("G", CultureInfo.InvariantCulture)} {item.Produtos.Unit}");
                            }
                        });
                    }
                });
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("SmartProd • Página ").FontSize(9).FontColor(Colors.Grey.Darken1);
                    text.CurrentPageNumber().FontSize(9).FontColor(Colors.Grey.Darken1);
                });
            });
        }).GeneratePdf();
    }

    private static string EscapeCsv(string value) =>
        $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
