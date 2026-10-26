using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartProd.API.Server.DTOs;
using SmartProd.API.Server.Services;

namespace SmartProd.API.Server.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Authorize]
public sealed class AuthController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public AuthController(IUsuarioService usuarioService) => _usuarioService = usuarioService;

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
    {
        var result = await _usuarioService.RefreshTokenAsync(dto.RefreshToken);
        if (result is null)
            return Unauthorized(new { error = "Refresh token inválido ou expirado." });

        return Ok(new
        {
            user = result.Value.user,
            token = result.Value.token,
            refreshToken = result.Value.refreshToken
        });
    }
}
