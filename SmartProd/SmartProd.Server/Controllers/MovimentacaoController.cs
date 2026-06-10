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
        private readonly ILogger<MovimentacaoController> _logger;
        public MovimentacaoController(MovimentoEstoqueService service, ILogger<MovimentacaoController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // Exemplo de rota: GET /api/movimentacao?search=azul&productId=1&type=ENTRADA&startDate=2024-01-01&endDate=2025-01-01
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

        [HttpPost("consumo")]
        public async Task<IActionResult> RegisterConsumption([FromBody] ConsumoRequestDto dto)
        {
            try
            {
                var (movement, product) = await _service.CreateConsumptionAsync(dto);
                return Ok(new
                {
                    message = "Consumo registrado com sucesso!",
                    movement = new
                    {
                        movement.Id,
                        movement.ProductId,
                        movement.OrderId,
                        tipo = movement.Tipo.ToString(),
                        movement.Quantity,
                        movement.CreatedAt
                    },
                    product = new
                    {
                        product.Id,
                        product.Code,
                        product.Name,
                        product.Tipo,
                        product.Unit,
                        product.EstoqueAtual
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar consumo");
                var msg = ex.InnerException != null
                    ? $"{ex.Message} -> {ex.InnerException.Message}"
                    : ex.Message;
                return BadRequest(new { error = msg });
            }
        }

        [HttpPost("finalizar")]
        public async Task<IActionResult> CompleteOrder([FromBody] ConsumoRequestDto dto)
        {
            try
            {
                var (movement, product) = await _service.CompleteOrderAsync(dto);
                return Ok(new
                {
                    message = "Produção finalizada com sucesso!",
                    movement = new
                    {
                        movement.Id,
                        movement.ProductId,
                        movement.OrderId,
                        tipo = movement.Tipo.ToString(),
                        movement.Quantity,
                        movement.CreatedAt
                    },
                    product = new
                    {
                        product.Id,
                        product.Code,
                        product.Name,
                        product.Tipo,
                        product.Unit,
                        product.EstoqueAtual
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao finalizar produção");
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
