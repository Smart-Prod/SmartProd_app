using System.ComponentModel.DataAnnotations;

namespace SmartProd.API.Server.DTOs
{
    public class ConsumoRequestDto
    {
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Code { get; set; } = string.Empty;
        [Range(1, int.MaxValue)]
        public int? OrderId { get; set; }
        [Range(double.Epsilon, double.MaxValue)]
        public double Quantity { get; set; } = 1;
    }
}
