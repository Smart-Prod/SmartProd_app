using System.ComponentModel.DataAnnotations;

namespace SmartProd.API.Server.DTOs
{
    public class NotaFiscalItemCreateDto
    {
        [Required]
        public int ProductId { get; set; }
        [Required]
        public double Quantity { get; set; }
        [Required]
        public double Value { get; set; }
    }
}
