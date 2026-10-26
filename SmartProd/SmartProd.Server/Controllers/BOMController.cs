using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartProd.API.Server.DTOs;
using SmartProd.API.Server.Services;

namespace SmartProd.API.Server.Controllers
{
    [ApiController]
    [Route("api/v1/BOM")]
    [Authorize]
    public class BOMController : ControllerBase
    {
        private readonly IBomService _service;

        public BOMController(IBomService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<List<BomResponseDto>>> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{id:int}")]
        public async Task<ActionResult<BomResponseDto>> GetById(int id)
        {
            var bom = await _service.GetByIdAsync(id);
            return bom == null ? NotFound() : Ok(bom);
        }

        [HttpPost]
        public async Task<ActionResult<BomResponseDto>> Create([FromBody] CreateBomDto dto)
        {
            try
            {
                var bom = await _service.CreateAsync(dto);
                return StatusCode(201, bom);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}