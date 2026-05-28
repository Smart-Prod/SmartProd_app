using SmartProd.API.Server.Enum;

namespace SmartProd.API.Server.Models
{
    public class Produto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public TipoProduto Tipo { get; set; }
        public string Unit { get; set; } = string.Empty;
        public double EstoqueAtual { get; set; }
        public double EstoqueReservado { get; set; }
        public double EstoqueMínimo { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        // Ficha Técnica (BOM)
        public Materiais? Bom { get; set; }
        public List<MateriaisItems> MaterialsUsed { get; set; } = new();

        // Relacionamentos de Movimentação
        public List<OrdemProducao> ProductionOrders { get; set; } = new();
        public List<NotaFiscalItem> InvoiceItems { get; set; } = new();
        public List<Movimentacao> StockMovements { get; set; } = new();
    }

}

