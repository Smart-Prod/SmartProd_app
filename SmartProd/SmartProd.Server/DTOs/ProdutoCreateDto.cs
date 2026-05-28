using SmartProd.API.Server.Enum;

namespace SmartProd.API.Server.DTOs
{
    public class ProdutoCreateDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public TipoProduto Tipo { get; set; }
        public string Unit { get; set; } = string.Empty;
        public double EstoqueAtual { get; set; }
        public double EstoqueReservado { get; set; }
        public double EstoqueMínimo { get; set; }
    }
}
