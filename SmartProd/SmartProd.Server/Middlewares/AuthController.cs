using Microsoft.AspNetCore.Mvc;
using SmartProd.API.Server.DTOs;
using SmartProd.API.Server.Models;

namespace SmartProd.API.Server.Middlewares
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto request)
        {
            // Aqui você faria a checagem real do usuário, por exemplo, no banco de dados.
            if (request.Email == "admin@example.com" && request.Senha == "1234")
            {
                return Ok(new { id = 1, email = request.Email, token = "token_exemplo" });
            }
            return Unauthorized();
        }
    }
}
