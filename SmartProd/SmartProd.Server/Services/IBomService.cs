using SmartProd.API.Server.DTOs;

namespace SmartProd.API.Server.Services;

public interface IBomService
{
    Task<List<BomResponseDto>> GetAllAsync();
    Task<BomResponseDto?> GetByIdAsync(int id);
    Task<BomResponseDto> CreateAsync(CreateBomDto dto);
}
