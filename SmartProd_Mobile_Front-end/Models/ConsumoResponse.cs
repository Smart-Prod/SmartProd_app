using System.Text.Json.Serialization;

namespace SmartProd_Mobile_Front_end.Models
{
    public class ConsumoResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("movement")]
        public ConsumoMovement? Movement { get; set; }

        [JsonPropertyName("product")]
        public ConsumoProduct? Product { get; set; }
    }

    public class ConsumoMovement
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [JsonPropertyName("orderId")]
        public int? OrderId { get; set; }

        [JsonPropertyName("tipo")]
        public string? Tipo { get; set; }

        [JsonPropertyName("quantity")]
        public double Quantity { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }

    public class ConsumoProduct
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("tipo")]
        public string? Tipo { get; set; }

        [JsonPropertyName("unit")]
        public string? Unit { get; set; }

        [JsonPropertyName("estoqueAtual")]
        public double EstoqueAtual { get; set; }
    }
}
