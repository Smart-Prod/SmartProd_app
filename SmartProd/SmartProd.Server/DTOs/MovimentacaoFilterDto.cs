namespace SmartProd.API.Server.DTOs
{
    public class MovimentacaoFilterDto
    {
        public string? Search { get; set; }
        public int? ProductId { get; set; }
        public string? Type { get; set; } // Pode ser mapeado para Enum depois
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
