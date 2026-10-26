using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartProd.API.Server.Services;

namespace SmartProd.API.Server.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class RelatorioController : ControllerBase
    {
        private readonly RelatorioService _relatorioService;
        private readonly ReportExportService _reportExportService;

        public RelatorioController(RelatorioService relatorioService, ReportExportService reportExportService)
        {
            _relatorioService = relatorioService;
            _reportExportService = reportExportService;
        }

        [HttpGet("produtos.csv")]
        public async Task<IActionResult> ExportProductsCsv(CancellationToken cancellationToken)
        {
            var stream = await _reportExportService.ExportProductsCsvAsync(cancellationToken);
            return File(stream, "text/csv; charset=utf-8", "smartprod-produtos.csv");
        }

        [HttpGet("movimentacoes.csv")]
        public async Task<IActionResult> ExportMovementsCsv(CancellationToken cancellationToken)
        {
            var stream = await _reportExportService.ExportMovementsCsvAsync(cancellationToken);
            return File(stream, "text/csv; charset=utf-8", "smartprod-movimentacoes.csv");
        }

        [HttpGet("estoque.pdf")]
        public async Task<IActionResult> ExportInventoryPdf(CancellationToken cancellationToken)
        {
            var pdf = await _reportExportService.CreateInventoryPdfAsync(cancellationToken);
            return File(pdf, "application/pdf", "smartprod-estoque.pdf");
        }

        [HttpGet("boms.pdf")]
        public async Task<IActionResult> ExportBomPdf(CancellationToken cancellationToken)
        {
            var pdf = await _reportExportService.CreateBomPdfAsync(cancellationToken);
            return File(pdf, "application/pdf", "smartprod-boms.pdf");
        }

        [HttpGet("producao")]
        public async Task<IActionResult> GetRelatorioProducao([FromQuery] DateTime dataInicial, [FromQuery] DateTime dataFinal)
        {
            try
            {
                var data = await _relatorioService.GetRelatorioProducao(dataInicial, dataFinal);
                return Ok(new { message = "Relatório carregado com sucesso", data });
            }
            catch (Exception e)
            {
                return BadRequest(new { error = e.Message });
            }
        }

        // Endpoint comentado - Método GetRelatorioEstoque foi comentado em RelatorioService
         [HttpGet("estoque")]
         public async Task<IActionResult> GetRelatorioEstoque()
         {
             try
             {
                 var data = await _relatorioService.GetRelatorioEstoque();
                 return Ok(new { message = "Relatório de estoque carregado", data });
             }
             catch (Exception e)
            {
                 return BadRequest(new { error = e.Message });
             }
         }

        [HttpGet("consumo-mp")]
        public async Task<IActionResult> GetRelatorioConsumoMP([FromQuery] DateTime dataInicial, [FromQuery] DateTime dataFinal)
        {
            try
            {
                var data = await _relatorioService.GetRelatorioConsumoMP(dataInicial, dataFinal);
                return Ok(new { message = "Relatório de consumo carregado", data });
            }
            catch (Exception e)
            {
                return BadRequest(new { error = e.Message });
            }
        }

        // Endpoint comentado - Método GetRelatorioVendas foi comentado em RelatorioService
         [HttpGet("vendas")]
         public async Task<IActionResult> GetRelatorioVendas([FromQuery] DateTime dataInicial, [FromQuery] DateTime dataFinal)
         {
             try
             {
                 var data = await _relatorioService.GetRelatorioVendas(dataInicial, dataFinal);
               return Ok(new { message = "Relatório de vendas carregado", data });
             }
             catch (Exception e)
             {
                 return BadRequest(new { error = e.Message });
             }
         }
    }
}
