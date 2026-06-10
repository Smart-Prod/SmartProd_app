using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SmartProd_Mobile_Front_end.Models;

namespace SmartProd_Mobile_Front_end.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<(LoginResponse? response, string? error)> LoginAsync(string email, string senha)
        {
            var payload = new { email, senha };
            var response = await _httpClient.PostAsJsonAsync("api/usuario/login", payload);

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
                if (loginResponse?.Token != null)
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", loginResponse.Token);
                }
                return (loginResponse, null);
            }

            var errorBody = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(_jsonOptions);
            return (null, errorBody?.Error ?? "Erro ao fazer login");
        }

        public async Task<(List<OrdemProducao>? ordens, string? error)> GetProductionOrdersAsync()
        {
            var response = await _httpClient.GetAsync("api/ProductionOrder");

            if (response.IsSuccessStatusCode)
            {
                var wrapper = await response.Content.ReadFromJsonAsync<OrderListResponse>(_jsonOptions);
                return (wrapper?.Orders, null);
            }

            var errorBody = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(_jsonOptions);
            return (null, errorBody?.Error ?? "Erro ao carregar ordens de produção");
        }

        public async Task<(List<Produto>? produtos, string? error)> GetProductsAsync()
        {
            var response = await _httpClient.GetAsync("api/Produto");

            if (response.IsSuccessStatusCode)
            {
                var produtos = await response.Content.ReadFromJsonAsync<List<Produto>>(_jsonOptions);
                return (produtos, null);
            }

            return (null, "Erro ao carregar produtos");
        }

        public async Task<(RegisterResponse? response, string? error)> RegisterAsync(string name, string company, string email, string senha)
        {
            var payload = new { name, company, email, senha };
            var response = await _httpClient.PostAsJsonAsync("api/Usuario/register", payload);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<RegisterResponse>(_jsonOptions);
                return (result, null);
            }

            var errorBody = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(_jsonOptions);
            return (null, errorBody?.Error ?? "Erro ao criar conta");
        }

        public async Task<(ConsumoResponse? response, string? error)> RegisterConsumptionAsync(string code, int? orderId, double quantity = 1)
        {
            var payload = new { code, orderId, quantity };
            var response = await _httpClient.PostAsJsonAsync("api/Movimentacao/consumo", payload);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ConsumoResponse>(_jsonOptions);
                return (result, null);
            }

            var errorBody = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(_jsonOptions);
            return (null, errorBody?.Error ?? "Erro ao registrar consumo");
        }

        public async Task<(ConsumoResponse? response, string? error)> CompleteOrderAsync(string code, int? orderId, double quantity = 1)
        {
            var payload = new { code, orderId, quantity };
            var response = await _httpClient.PostAsJsonAsync("api/Movimentacao/finalizar", payload);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ConsumoResponse>(_jsonOptions);
                return (result, null);
            }

            var errorBody = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(_jsonOptions);
            return (null, errorBody?.Error ?? "Erro ao finalizar produção");
        }

        public async Task<(Produto? product, string? error)> GetProductByCodeAsync(string code)
        {
            var response = await _httpClient.GetAsync($"api/Produto/{code}");

            if (response.IsSuccessStatusCode)
            {
                var product = await response.Content.ReadFromJsonAsync<Produto>(_jsonOptions);
                return (product, null);
            }

            var errorBody = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(_jsonOptions);
            return (null, errorBody?.Error ?? "Produto não encontrado");
        }

        public async Task<(byte[]? imageBytes, string? error)> GetProductQrCodeAsync(string code)
        {
            var response = await _httpClient.GetAsync($"api/Produto/{code}/qrcode");

            if (response.IsSuccessStatusCode)
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();
                return (bytes, null);
            }

            var errorBody = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(_jsonOptions);
            return (null, errorBody?.Error ?? "Erro ao gerar QR Code");
        }

        public async Task<(byte[]? pdfBytes, string? error)> ExportQrCodesPdfAsync(string? tipo = null)
        {
            var url = "api/Produto/qrcodes/export";
            if (!string.IsNullOrWhiteSpace(tipo))
                url += $"?tipo={tipo}";

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();
                return (bytes, null);
            }

            var errorBody = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(_jsonOptions);
            return (null, errorBody?.Error ?? "Erro ao exportar QR Codes");
        }

        public async Task<(DashboardResponse? dashboard, string? error)> GetDashboardAsync()
        {
            var response = await _httpClient.GetAsync("api/dashboard");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<DashboardResponse>(_jsonOptions);
                return (data, null);
            }

            var errorBody = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(_jsonOptions);
            return (null, errorBody?.Error ?? "Erro ao carregar dashboard");
        }
    }
}
