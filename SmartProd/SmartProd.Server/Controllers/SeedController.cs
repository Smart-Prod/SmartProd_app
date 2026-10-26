using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartProd.API.Server.Services;
using Microsoft.EntityFrameworkCore;
using SmartProd.API.Server.Data;

namespace SmartProd.API.Server.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(Roles = "Admin")]
    public class SeedController : ControllerBase
    {
        private readonly ISeedService _seedService;
        private readonly AppDbContext _context;

        public SeedController(ISeedService seedService, AppDbContext context)
        {
            _seedService = seedService;
            _context = context;
        }

        [HttpPost("kaixote")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SeedKaixote()
        {
            try
            {
                await _context.Database.MigrateAsync();
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
