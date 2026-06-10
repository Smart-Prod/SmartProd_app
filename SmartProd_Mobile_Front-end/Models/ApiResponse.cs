using System.Text.Json.Serialization;

namespace SmartProd_Mobile_Front_end.Models
{
    public class ApiErrorResponse
    {
        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }

    public class OrderListResponse
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("orders")]
        public List<OrdemProducao>? Orders { get; set; }
    }

    public class RegisterResponse
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("user")]
        public UsuarioResponse? User { get; set; }
    }
}
