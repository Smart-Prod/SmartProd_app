using SmartProd.API.Server.Enum;
using System.ComponentModel.DataAnnotations;

namespace SmartProd.API.Server.DTOs
{
    public class NotaFiscalCreateDto
    {
        [Required]
        public NotaFiscalTipo Type { get; set; }
        [Required]
        public string? Number { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public string? Supplier { get; set; }
        public string? Customer { get; set; }
        [Required]
        public List<NotaFiscalItemCreateDto> Items { get; set; } = new();
    }
}
