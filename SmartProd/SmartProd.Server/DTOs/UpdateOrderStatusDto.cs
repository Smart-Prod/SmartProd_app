namespace SmartProd.API.Server.DTOs
{
    public class UpdateOrderStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public double? Produced { get; set; }
    }
}