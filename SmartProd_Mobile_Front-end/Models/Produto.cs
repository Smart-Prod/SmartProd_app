using System.Text.Json.Serialization;

namespace SmartProd_Mobile_Front_end.Models
{
    public class Produto
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

        [JsonPropertyName("estoqueReservado")]
        public double EstoqueReservado { get; set; }

        [JsonPropertyName("estoqueMínimo")]
        public double EstoqueMinimo { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}
