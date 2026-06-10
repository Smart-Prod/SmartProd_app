using System.Collections.ObjectModel;
using System.Windows.Input;
using SmartProd_Mobile_Front_end.Models;
using SmartProd_Mobile_Front_end.Services;

namespace SmartProd.ViewModels
{
    public class ProductionPageViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private string _searchText = string.Empty;
        private string _lineEfficiency = "94.2%";
        private string _latency = "480ms";
        private string _traceability = "100%";

        public ObservableCollection<ProductionOrder> Orders { get; set; } = new();

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                if (string.IsNullOrWhiteSpace(value))
                {
                    _ = LoadOrdersAsync();
                }
            }
        }

        public string LineEfficiency
        {
            get => _lineEfficiency;
            set => SetProperty(ref _lineEfficiency, value);
        }

        public string Latency
        {
            get => _latency;
            set => SetProperty(ref _latency, value);
        }

        public string Traceability
        {
            get => _traceability;
            set => SetProperty(ref _traceability, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand StartProductionCommand { get; }
        public ICommand SearchCommand { get; }

        public ProductionPageViewModel(ApiService apiService)
        {
            _apiService = apiService;
            RefreshCommand = new Command(async () => await LoadOrdersAsync());
            StartProductionCommand = new Command<ProductionOrder>(async (order) => await OnStartProductionAsync(order));
            SearchCommand = new Command<string>(async (query) => await OnSearchAsync(query));
        }

        public async Task LoadOrdersAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                IsRefreshing = true;

                var (dashboard, dashError) = await _apiService.GetDashboardAsync();
                if (dashError == null && dashboard != null)
                {
                    LineEfficiency = dashboard.LineEfficiency;
                    Latency = dashboard.Latency;
                    Traceability = dashboard.Traceability;
                }

                var (ordens, error) = await _apiService.GetProductionOrdersAsync();

                if (error != null)
                {
                    await Shell.Current.DisplayAlertAsync("Erro", error, "OK");
                    return;
                }

                Orders.Clear();
                if (ordens != null)
                {
                    foreach (var ordem in ordens)
                    {
                        var fmt = "dd/MM/yyyy HH:mm";
                        Orders.Add(new ProductionOrder
                        {
                            Id = ordem.Id.ToString(),
                            Product = ordem.Produto?.Name ?? $"Produto #{ordem.ProductId}",
                            Status = ordem.Status switch
                            {
                                "PLANEJADA" => "Pendente",
                                "EM_PRODUCAO" => "Em Curso",
                                "CONCLUIDA" => "Concluída",
                                "CANCELADA" => "Cancelada",
                                "PAUSADA" => "Pausada",
                                _ => ordem.Status ?? "Desconhecido"
                            },
                            CreatedAt = ordem.CreatedAt.ToString(fmt),
                            StartTime = ordem.StartedAt?.ToString(fmt) ?? "-",
                            EndTime = ordem.FinishedAt?.ToString(fmt) ?? "-",
                            StatusColor = ordem.StatusColor
                        });
                    }
                }
            }
            catch (HttpRequestException)
            {
                await Shell.Current.DisplayAlertAsync("Erro", "Não foi possível conectar ao servidor.", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await Shell.Current.DisplayAlertAsync("Erro", "Falha ao carregar ordens de produção.", "OK");
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        private async Task OnStartProductionAsync(ProductionOrder? order)
        {
            if (order == null) return;
            await Shell.Current.GoToAsync($"//ScannerPage?opId={order.Id}");
        }

        private async Task OnSearchAsync(string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                await LoadOrdersAsync();
                return;
            }

            var filtered = Orders
                .Where(o => (o.Id?.Contains(query) == true) || (o.Product?.ToLower().Contains(query.ToLower()) == true))
                .ToList();
            Orders.Clear();
            foreach (var order in filtered) Orders.Add(order);
        }
    }
}
