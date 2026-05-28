using SmartProd.API.Server.Enum;

namespace SmartProd.API.Server.Models
{
    public class Movimentacao
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Produto Produto { get; set; } = null!;
        public int? OrderId { get; set; } // FK para OrdemProducao, se houver
        public TipoMovimentacao Tipo { get; set; } // ENUM: ENTRADA, SAIDA, PRODUCAO, CONSUMO...
        public double Quantity { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
