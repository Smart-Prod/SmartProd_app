using Microsoft.AspNetCore.Mvc;
using SmartProd.API.Server.Data;
using SmartProd.API.Server.Services;

namespace SmartProd.API.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly SeedService _seedService;
        private readonly UsuarioService _usuarioService;
        private readonly AppDbContext _context;

        public SeedController(SeedService seedService, UsuarioService usuarioService, AppDbContext context)
        {
            _seedService = seedService;
            _usuarioService = usuarioService;
            _context = context;
        }

        [HttpPost("kaixote")]
        public async Task<IActionResult> SeedKaixote()
        {
            try
            {
                await _context.Database.EnsureDeletedAsync();
                await _context.Database.EnsureCreatedAsync();
                await _usuarioService.SeedUsersAsync();
                var message = await _seedService.SeedKaixoteAsync();
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message, inner = ex.InnerException?.Message });
            }
        }
    }
}
