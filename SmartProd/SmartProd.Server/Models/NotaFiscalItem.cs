namespace SmartProd.API.Server.Models
{
    public class NotaFiscalItem
    {
        public int Id { get; set; }
        public int NotaFiscalId { get; set; }
        public NotaFiscal NotaFiscal { get; set; } = null!;

        public int ProductId { get; set; }
        public Produto Produto { get; set; } = null!;
        public double Quantity { get; set; }
        public double Value { get; set; }
    }
}
