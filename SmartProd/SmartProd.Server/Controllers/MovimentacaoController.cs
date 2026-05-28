using Microsoft.AspNetCore.Mvc;
using SmartProd.API.Server.DTOs;
using SmartProd.API.Server.Services;

namespace SmartProd.API.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovimentacaoController : ControllerBase
    {
        private readonly MovimentoEstoqueService _service;
        public MovimentacaoController(MovimentoEstoqueService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMovement([FromBody] CreateMovimentacaoDto dto)
        {
            try
            {
                var movement = await _service.CreateMovementAsync(dto);
                return StatusCode(201, movement);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMovements([FromQuery] MovimentacaoFilterDto filters)
        {
            try
            {
                var result = await _service.GetAllMovementsAsync(filters);
                return Ok(new
                {
                    summary = result.Resumo,
                    total = result.Total,
                    saldoLiquido = result.SaldoLiquido,
                    movements = result.Movements
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
