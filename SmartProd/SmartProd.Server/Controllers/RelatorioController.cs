using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartProd.API.Server.Services;

namespace SmartProd.API.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RelatorioController : ControllerBase
    {
        private readonly RelatorioService _relatorioService;
        public RelatorioController(RelatorioService relatorioService)
        {
            _relatorioService = relatorioService;
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
