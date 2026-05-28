using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartProd.API.Server.Data;
using SmartProd.API.Server.DTOs;
using SmartProd.API.Server.Enum;
using SmartProd.API.Server.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartProd.API.Server.Services
{
    public class UsuarioService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public UsuarioService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<UsuarioResponseDto> CreateUserAsync(RegisterDto dto)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Email == dto.Email))
                throw new Exception("E-mail já está em uso.");

            var hash = BCrypt.Net.BCrypt.HashPassword(dto.Senha);

            var user = new Usuario
            {
                Name = dto.Name ?? string.Empty,
                Email = dto.Email ?? string.Empty,
                Senha = hash,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                Role = UserRole.Operator
            };

            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync();

            return new UsuarioResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<(UsuarioResponseDto user, string token)> LoginUserAsync(LoginDto dto)
        {
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                throw new Exception("Usuário não encontrado.");

            if (!BCrypt.Net.BCrypt.Verify(dto.Senha, user.Senha))
                throw new Exception("Senha incorreta.");

            var token = BuildToken(user);
            var userResponse = new UsuarioResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt
            };

            return (userResponse, token);
        }

        public async Task<List<UsuarioResponseDto>> GetAllUsersAsync()
        {
            return await _context.Usuarios
                .Select(user => new UsuarioResponseDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    CreatedAt = user.CreatedAt
                }).ToListAsync();
        }

        public async Task<UsuarioResponseDto> UpdateUserAsync(int id, UpdateUserDto dto)
        {
            var user = await _context.Usuarios.FindAsync(id);
            if (user == null)
                throw new Exception("Usuário não encontrado.");

            if (!string.IsNullOrWhiteSpace(dto.Name))
                user.Name = dto.Name;
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                if (await _context.Usuarios.AnyAsync(u => u.Email == dto.Email && u.Id != id))
                    throw new Exception("E-mail já está em uso.");
                user.Email = dto.Email;
            }
            if (dto.Role.HasValue)
                user.Role = dto.Role.Value;
            if (dto.Active.HasValue)
                user.Active = dto.Active.Value;

            await _context.SaveChangesAsync();

            return new UsuarioResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt
            };
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _context.Usuarios.FindAsync(id);
            if (user == null)
                throw new Exception("Usuário não encontrado.");

            _context.Usuarios.Remove(user);
            await _context.SaveChangesAsync();
        }

        // Gera JWT usando settings do appsettings.json
        private string BuildToken(Usuario user)
        {
            var jwtSecret = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret não configurado");
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer não configurado");

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddDays(
                double.TryParse(_configuration["Jwt:ExpiresInDays"], out var days) ? days : 1);

            var token = new JwtSecurityToken(
                jwtIssuer,
                jwtIssuer,
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
