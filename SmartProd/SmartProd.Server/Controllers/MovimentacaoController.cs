using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartProd.API.Server.DTOs;
using SmartProd.API.Server.Services;

namespace SmartProd.API.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class MovimentacaoController : ControllerBase
{
    private readonly MovimentoEstoqueService _service;
    private readonly ILogger<MovimentacaoController> _logger;

    public MovimentacaoController(MovimentoEstoqueService service, ILogger<MovimentacaoController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllMovements([FromQuery] MovimentacaoFilterDto filters)
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

    [HttpPost]
    public async Task<IActionResult> CreateMovement([FromBody] CreateMovimentacaoDto dto)
    {
        try
        {
            return Ok(await _service.CreateMovementAsync(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar movimentação");
            return BadRequest(new { error = ex.Message });
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
            return BadRequest(new { error = ex.Message });
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
