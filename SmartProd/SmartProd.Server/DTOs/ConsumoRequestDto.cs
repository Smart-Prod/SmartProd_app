namespace SmartProd.API.Server.DTOs
{
    public class ConsumoRequestDto
    {
        public string Code { get; set; } = string.Empty;
        public int? OrderId { get; set; }
        public double Quantity { get; set; } = 1;
    }
}
