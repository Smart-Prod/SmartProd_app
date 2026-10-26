using System.ComponentModel.DataAnnotations;

namespace SmartProd.API.Server.DTOs
{
    public class NotaFiscalItemCreateDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }
        [Required]
        [Range(double.Epsilon, double.MaxValue)]
        public double Quantity { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public double Value { get; set; }
    }
}
