namespace SmartProd.API.Server.Models
{
    public class Vendas
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Produto Produto { get; set; } = null!;
        public double Quantity { get; set; }
        public double TotalValue { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
