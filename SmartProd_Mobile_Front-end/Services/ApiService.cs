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

        public async Task<(string? message, string? error)> RegisterAsync(string name, string email, string senha)
        {
            var payload = new { name, email, senha };
            var response = await _httpClient.PostAsJsonAsync("api/usuario/register", payload);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<RegisterResponse>(_jsonOptions);
                return (result?.Message ?? "Conta criada com sucesso!", null);
            }

            var errorBody = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(_jsonOptions);
            return (null, errorBody?.Error ?? "Erro ao criar conta");
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

        public async Task<(MovimentacaoResponse? response, string? error)> GetStockMovementsAsync()
        {
            var response = await _httpClient.GetAsync("api/movimentacao");

            if (response.IsSuccessStatusCode)
            {
                var movements = await response.Content.ReadFromJsonAsync<MovimentacaoResponse>(_jsonOptions);
                return (movements, null);
            }

            return (null, "Erro ao carregar movimentações");
        }

        public async Task<(bool success, string? error)> CreateMovimentacaoAsync(int productId, string tipo, double quantity, int? orderId = null)
        {
            var payload = new { productId, tipo, quantity, orderId };
            var response = await _httpClient.PostAsJsonAsync("api/movimentacao", payload);

            if (response.IsSuccessStatusCode)
                return (true, null);

            var errorBody = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(_jsonOptions);
            return (false, errorBody?.Error ?? "Erro ao registrar movimentação");
        }
    }
}
