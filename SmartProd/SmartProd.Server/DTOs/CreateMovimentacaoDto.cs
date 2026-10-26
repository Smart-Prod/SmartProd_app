using System.ComponentModel.DataAnnotations;

namespace SmartProd.API.Server.DTOs
{
    public class CreateMovimentacaoDto
    {
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 1)]
        public string Tipo { get; set; } = string.Empty;
        [Range(double.Epsilon, double.MaxValue)]
        public double Quantity { get; set; }
        [Range(1, int.MaxValue)]
        public int? OrderId { get; set; }
    }
}