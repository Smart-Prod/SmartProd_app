using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartProd.API.Server.DTOs;
using SmartProd.API.Server.Services;

namespace SmartProd.API.Server.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
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
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (usuarioIdClaim == null || !int.TryParse(usuarioIdClaim, out var usuarioId))
                return Unauthorized();

            var nota = await _service.CreateInvoiceAsync(dto, usuarioId);
            return CreatedAtAction(null, new { id = nota.Id }, nota);
        }

        [HttpPost("upload-xml")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadXmlInvoice(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "Arquivo XML da NF-e é obrigatório." });

            if (!file.FileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { error = "O arquivo enviado deve possuir a extensão .xml." });

            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (usuarioIdClaim == null || !int.TryParse(usuarioIdClaim, out var usuarioId))
                return Unauthorized();

            await using var stream = file.OpenReadStream();
            var nota = await _service.ProcessXmlInvoiceAsync(stream, usuarioId, file.FileName);
            return StatusCode(201, new
            {
                message = "Nota Fiscal XML processada com sucesso!",
                notaFiscal = nota
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllInvoices()
        {
            var notes = await _service.GetAllInvoicesAsync();
            return Ok(notes);
        }
    }
}
