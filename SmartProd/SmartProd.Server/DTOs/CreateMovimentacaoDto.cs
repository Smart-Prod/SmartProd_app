namespace SmartProd.API.Server.DTOs
{
    public class CreateMovimentacaoDto
    {
        public int ProductId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public double Quantity { get; set; }
        public int? OrderId { get; set; }
    }
}