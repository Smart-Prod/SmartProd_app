using SmartProd.API.Server.Enum;

namespace SmartProd.API.Server.Models
{
    public class NotaFiscal
    {
        public int Id { get; set; }
        public NotaFiscalTipo Type { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? Supplier { get; set; }
        public string? Customer { get; set; }
        public NotaFiscalStatus Status { get; set; }

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public List<NotaFiscalItem> Items { get; set; } = new();
    }
}

