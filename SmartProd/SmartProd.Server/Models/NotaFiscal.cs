using SmartProd.API.Server.Enum;

namespace SmartProd.API.Server.Models
{
    public class NotaFiscal
    {
        public int Id { get; set; } // Normalmente é int. Se usar outro tipo, troque
        public NotaFiscalTipo Type { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? Supplier { get; set; }
        public string? Customer { get; set; }
        public NotaFiscalStatus Status { get; set; }
        public List<NotaFiscalItem> Items { get; set; } = new();
    }
}

