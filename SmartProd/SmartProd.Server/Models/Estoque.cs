namespace SmartProd.API.Server.Models
{
    public class Estoque : ITenantOwned
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public int ProductId { get; set; }
        public Produto Produto { get; set; } = null!;
        public double Quantity { get; set; }
        public string Unit { get; set; } = "UN";
    }
}
