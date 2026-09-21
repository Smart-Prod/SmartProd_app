using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartProd.API.Server.DTOs;
using SmartProd.API.Server.Services;
using System.Security.Claims;

namespace SmartProd.API.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotaFiscalController : ControllerBase
    {
        private readonly NotaFiscalService _service;
        public NotaFiscalController(NotaFiscalService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateInvoice([FromBody] NotaFiscalCreateDto dto)
        {
            try
            {
                var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (usuarioIdClaim == null) return Unauthorized();

                int usuarioId = int.Parse(usuarioIdClaim);

                var nota = await _service.CreateInvoiceAsync(dto, usuarioId);
                return CreatedAtAction(null, new { id = nota.Id }, nota);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllInvoices()
        {
            try
            {
                var notes = await _service.GetAllInvoicesAsync();
                return Ok(notes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
