namespace SmartProd.API.Server.Models
{
    public class Estoque
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Produto Produto { get; set; } = null!;
        public double Quantity { get; set; }
        public string Unit { get; set; } = "UN";
    }
}
