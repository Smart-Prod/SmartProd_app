using SmartProd.API.Server.Enum;

namespace SmartProd.API.Server.Models
{

    public class OrdemProducao
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Produto Produto { get; set; } = null!;

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public double Quantity { get; set; } // planejado
        public double Produced { get; set; } // realizado
        public OrdemProducaoStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? FinishedAt { get; set; }
    }
}
