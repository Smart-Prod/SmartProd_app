using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartProd.API.Server.Data;
using SmartProd.API.Server.DTOs;
using SmartProd.API.Server.Enum;
using SmartProd.API.Server.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SmartProd.API.Server.Services
{
    public class UsuarioService : IUsuarioService
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
                Active = user.Active,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<(UsuarioResponseDto user, string token, string refreshToken)> LoginUserAsync(LoginDto dto)
        {
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !user.Active)
                throw new UnauthorizedAccessException("Credenciais inválidas.");

            if (!BCrypt.Net.BCrypt.Verify(dto.Senha, user.Senha))
                throw new UnauthorizedAccessException("Credenciais inválidas.");

            var token = BuildToken(user);
            var refreshToken = CreateRefreshToken();
            user.RefreshTokenHash = HashRefreshToken(refreshToken);
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(30);
            await _context.SaveChangesAsync();

            var userResponse = new UsuarioResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString(),
                Active = user.Active,
                CreatedAt = user.CreatedAt
            };

            return (userResponse, token, refreshToken);
        }

        public async Task<(UsuarioResponseDto user, string token, string refreshToken)?> RefreshTokenAsync(string refreshToken)
        {
            var tokenHash = HashRefreshToken(refreshToken);
            var user = await _context.Usuarios.FirstOrDefaultAsync(candidate =>
                candidate.RefreshTokenHash == tokenHash &&
                candidate.RefreshTokenExpiresAt > DateTime.UtcNow &&
                candidate.Active);

            if (user == null)
                return null;

            var rotatedRefreshToken = CreateRefreshToken();
            user.RefreshTokenHash = HashRefreshToken(rotatedRefreshToken);
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(30);
            await _context.SaveChangesAsync();

            return (new UsuarioResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString(),
                Active = user.Active,
                CreatedAt = user.CreatedAt
            }, BuildToken(user), rotatedRefreshToken);
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
                    Active = user.Active,
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
                Active = user.Active,
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

        private string BuildToken(Usuario user)
        {
            var jwtSecret = _configuration["JWT_SECRET"]
                ?? _configuration["SMARTPROD_JWT_SECRET"]
                ?? _configuration["Jwt:Secret"]
                ?? throw new InvalidOperationException("JWT_SECRET não configurado");
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
                double.TryParse(_configuration["Jwt:ExpiresInDays"], out var days) ? days : 7);

            var token = new JwtSecurityToken(
                jwtIssuer,
                jwtIssuer,
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string CreateRefreshToken() =>
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        private static string HashRefreshToken(string refreshToken) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
    }
}
