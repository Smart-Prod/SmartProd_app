using System.Text.Json.Serialization;

namespace SmartProd_Mobile_Front_end.Models
{
    public class OrdemProducao
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [JsonPropertyName("produto")]
        public Produto? Produto { get; set; }

        [JsonPropertyName("quantity")]
        public double Quantity { get; set; }

        [JsonPropertyName("produced")]
        public double Produced { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("startedAt")]
        public DateTime? StartedAt { get; set; }

        [JsonPropertyName("finishedAt")]
        public DateTime? FinishedAt { get; set; }

        public string StatusColor => Status switch
        {
            "PLANEJADA" => "Blue",
            "EM_PRODUCAO" => "Orange",
            "PAUSADA" => "Gray",
            "CONCLUIDA" => "Green",
            "CANCELADA" => "Red",
            _ => "Black"
        };
    }
}
