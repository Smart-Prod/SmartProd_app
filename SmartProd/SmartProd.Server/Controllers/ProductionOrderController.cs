using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartProd.API.Server.DTOs;
using SmartProd.API.Server.Enum;
using SmartProd.API.Server.Services;

namespace SmartProd.API.Server.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class ProductionOrderController : ControllerBase
    {
        private readonly ProductionOrderService _service;

        public ProductionOrderController(ProductionOrderService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrdemProducaoCreateDto dto)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(usuarioIdClaim, out var usuarioId))
            {
                dto.UsuarioId = usuarioId;
            }

            var result = await _service.CreateOrderAsync(dto);
            return StatusCode(201, new { message = "Ordem de produção criada com sucesso!", result });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _service.GetAllOrdersAsync();
            return Ok(new { message = "Ordens listadas com sucesso!", orders });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await _service.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound(new { error = "Ordem de produção não encontrada ou não pertence à sua organização." });

            return Ok(order);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto)
        {
            try
            {
                if (!System.Enum.TryParse<OrdemProducaoStatus>(dto.Status, true, out var status))
                    return BadRequest(new { error = $"Status inválido: {dto.Status}" });

                var order = await _service.UpdateOrderStatusAsync(id, status, dto.Produced);
                return Ok(new { message = "Status atualizado com sucesso!", order });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
