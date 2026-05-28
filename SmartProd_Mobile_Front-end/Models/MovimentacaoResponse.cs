using System.Text.Json.Serialization;

namespace SmartProd_Mobile_Front_end.Models
{
    public class MovimentacaoResponse
    {
        [JsonPropertyName("summary")]
        public MovimentacaoSummary? Summary { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("saldoLiquido")]
        public int SaldoLiquido { get; set; }

        [JsonPropertyName("movements")]
        public List<MovimentacaoItem>? Movements { get; set; }
    }

    public class MovimentacaoSummary
    {
        [JsonPropertyName("entradas")]
        public int Entradas { get; set; }

        [JsonPropertyName("saidas")]
        public int Saidas { get; set; }

        [JsonPropertyName("producao")]
        public int Producao { get; set; }

        [JsonPropertyName("consumo")]
        public int Consumo { get; set; }
    }

    public class MovimentacaoItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [JsonPropertyName("produto")]
        public MovProduto? Produto { get; set; }

        [JsonPropertyName("orderId")]
        public int? OrderId { get; set; }

        [JsonPropertyName("tipo")]
        public string? Tipo { get; set; }

        [JsonPropertyName("quantity")]
        public double Quantity { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }

    public class MovProduto
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
    }
}
