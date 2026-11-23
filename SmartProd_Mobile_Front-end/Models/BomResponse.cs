using System.Text.Json.Serialization;

namespace SmartProd_Mobile_Front_end.Models
{
    public class BomResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("produtoId")]
        public int ProdutoId { get; set; }

        [JsonPropertyName("produto")]
        public Produto? Produto { get; set; }

        [JsonPropertyName("materials")]
        public List<BomItemResponse> Materials { get; set; } = new();
    }

    public class BomItemResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("materiaisId")]
        public int MateriaisId { get; set; }

        [JsonPropertyName("produtosId")]
        public int ProdutosId { get; set; }

        [JsonPropertyName("quantidade")]
        public double Quantidade { get; set; }

        [JsonPropertyName("produtos")]
        public Produto? Produtos { get; set; }
    }
}
