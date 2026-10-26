using System.ComponentModel.DataAnnotations;

namespace SmartProd.API.Server.DTOs;

public sealed class CreateBomDto
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }
    [Required]
    [MinLength(1)]
    public List<BomItemDto> Materials { get; set; } = [];
}

public sealed class BomItemDto
{
    [Range(1, int.MaxValue)]
    public int MaterialId { get; set; }
    [Range(double.Epsilon, double.MaxValue)]
    public double Quantidade { get; set; }
}
