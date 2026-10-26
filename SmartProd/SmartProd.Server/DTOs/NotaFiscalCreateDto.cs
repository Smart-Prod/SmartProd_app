using SmartProd.API.Server.Enum;
using System.ComponentModel.DataAnnotations;

namespace SmartProd.API.Server.DTOs
{
    public class NotaFiscalCreateDto
    {
        [Required]
        public NotaFiscalTipo Type { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string? Number { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [StringLength(200)]
        public string? Supplier { get; set; }
        [StringLength(200)]
        public string? Customer { get; set; }
        [Required]
        public List<NotaFiscalItemCreateDto> Items { get; set; } = new();
    }
}
