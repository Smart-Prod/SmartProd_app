using System.Text.Json.Serialization;

namespace SmartProd_Mobile_Front_end.Models
{
    public class LoginResponse
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("user")]
        public UsuarioResponse? User { get; set; }

        [JsonPropertyName("token")]
        public string? Token { get; set; }
    }
}
