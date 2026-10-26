using SmartProd.API.Server.Enum;
using System.ComponentModel.DataAnnotations;

namespace SmartProd.API.Server.DTOs
{
    public class ProdutoCreateDto
    {
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Code { get; set; } = string.Empty;
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
        public TipoProduto Tipo { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 1)]
        public string Unit { get; set; } = string.Empty;
        [Range(0, double.MaxValue)]
        public double EstoqueAtual { get; set; }
        [Range(0, double.MaxValue)]
        public double EstoqueReservado { get; set; }
        [Range(0, double.MaxValue)]
        public double EstoqueMínimo { get; set; }
    }
}
