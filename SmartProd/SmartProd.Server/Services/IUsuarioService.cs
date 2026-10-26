using SmartProd.API.Server.DTOs;

namespace SmartProd.API.Server.Services;

public interface IUsuarioService
{
    Task<UsuarioResponseDto> CreateUserAsync(RegisterDto dto);
    Task<(UsuarioResponseDto user, string token, string refreshToken)> LoginUserAsync(LoginDto dto);
    Task<(UsuarioResponseDto user, string token, string refreshToken)?> RefreshTokenAsync(string refreshToken);
    Task<List<UsuarioResponseDto>> GetAllUsersAsync();
    Task<UsuarioResponseDto> UpdateUserAsync(int id, UpdateUserDto dto);
    Task DeleteUserAsync(int id);
}
