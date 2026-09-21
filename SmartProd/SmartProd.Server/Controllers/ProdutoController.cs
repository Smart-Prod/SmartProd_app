using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using SmartProd.API.Server.DTOs;
using SmartProd.API.Server.Services;
using System.Security.Claims;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SmartProd.API.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProdutoController : ControllerBase
    {
        private readonly ProdutoService _produtoService;

        public ProdutoController(ProdutoService produtoService)
        {
            _produtoService = produtoService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ProdutoCreateDto dto)
        {
            try
            {
                var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (usuarioIdClaim == null) return Unauthorized();

                int usuarioId = int.Parse(usuarioIdClaim);
                var product = await _produtoService.CreateProductAsync(dto, usuarioId);
                return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var products = await _produtoService.GetAllProductsAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                var product = await _produtoService.GetProductByIdAsync(id);
                return Ok(product);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> GetProductByCode(string code)
        {
            try
            {
                var product = await _produtoService.GetProductByCodeAsync(code);
                if (product == null)
                    return NotFound(new { error = "Produto não encontrado." });
                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{code}/qrcode")]
        public async Task<IActionResult> GetProductQrCode(string code)
        {
            try
            {
                var product = await _produtoService.GetProductByCodeAsync(code);
                if (product == null)
                    return NotFound(new { error = "Produto não encontrado." });

                using var generator = new QRCodeGenerator();
                var qrData = generator.CreateQrCode(product.Code, QRCodeGenerator.ECCLevel.Q);
                var pngRenderer = new PngByteQRCode(qrData);
                                var pngBytes = pngRenderer.GetGraphic(20, System.Drawing.Color.Black, System.Drawing.Color.White);

                return File(pngBytes, "image/png", $"qrcode-{product.Code}.png");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("qrcodes/export")]
        public async Task<IActionResult> ExportQrCodesPdf([FromQuery] string? tipo = null)
        {
            try
            {
                var products = await _produtoService.GetAllProductsAsync();

                if (!string.IsNullOrWhiteSpace(tipo))
                    products = products.Where(p => p.Tipo.ToString() == tipo).ToList();

                if (products.Count == 0)
                    return NotFound(new { error = "Nenhum produto encontrado para exportação." });

                QuestPDF.Settings.License = LicenseType.Community;

                var pdf = Document.Create(container =>
                {
                    foreach (var product in products)
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(20);
                            page.DefaultTextStyle(x => x.FontSize(10));

                            page.Header().AlignCenter().Text("ETIQUETAS SMARTPROD")
                                .FontSize(16).Bold().FontColor("#FF8C00");

                            page.Content().PaddingTop(30).AlignCenter().Column(col =>
                            {
                                using var generator = new QRCodeGenerator();
                                var qrData = generator.CreateQrCode(product.Code, QRCodeGenerator.ECCLevel.Q);
                var pngRenderer = new PngByteQRCode(qrData);
                var pngBytes = pngRenderer.GetGraphic(20, System.Drawing.Color.Black, System.Drawing.Color.White);

                                col.Item().Width(200).Height(200).Image(pngBytes);
                                col.Item().PaddingTop(12).AlignCenter().Text(product.Code)
                                    .FontSize(18).Bold();
                                col.Item().PaddingTop(4).AlignCenter().Text(product.Name)
                                    .FontSize(12);
                                col.Item().PaddingTop(4).AlignCenter().Text($"Tipo: {product.Tipo} | Estoque: {product.EstoqueAtual} {product.Unit}")
                                    .FontSize(10).FontColor("#636E72");
                            });

                            page.Footer().AlignCenter().Text(text =>
                            {
                                text.Span("Gerado em ").FontSize(8);
                                text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(8);
                            });
                        });
                    }
                }).GeneratePdf();

                return File(pdf, "application/pdf", "etiquetas-smartprod.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
