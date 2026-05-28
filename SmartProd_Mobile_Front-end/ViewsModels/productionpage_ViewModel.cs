using System.Collections.ObjectModel;
using System.Windows.Input;
using SmartProd_Mobile_Front_end.Models;
using SmartProd_Mobile_Front_end.Services;

namespace SmartProd.ViewModels
{
    public class ProductionPageViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

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

        public ICommand RefreshCommand { get; }
        public ICommand StartProductionCommand { get; }
        public ICommand SearchCommand { get; }

        public ProductionPageViewModel(ApiService apiService)
        {
            _apiService = apiService;
            RefreshCommand = new Command(async () => await LoadOrdersAsync());
            StartProductionCommand = new Command<ProductionOrder>(async (order) => await OnStartProductionAsync(order));
            SearchCommand = new Command<string>(async (query) => await OnSearchAsync(query));

            Task.Run(async () => await LoadOrdersAsync());
        }

        private async Task LoadOrdersAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                IsRefreshing = true;

                var (ordens, error) = await _apiService.GetProductionOrdersAsync();

                if (error != null)
                {
                    await Shell.Current.DisplayAlert("Erro", error, "OK");
                    return;
                }

                Orders.Clear();
                if (ordens != null)
                {
                    foreach (var ordem in ordens)
                    {
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
                            StartTime = ordem.CreatedAt.ToString("dd/MM - HH:mm"),
                            EndTime = ordem.FinishedAt?.ToString("dd/MM - HH:mm") ?? "-",
                            StatusColor = ordem.StatusColor
                        });
                    }
                }
            }
            catch (HttpRequestException)
            {
                await Shell.Current.DisplayAlert("Erro", "Não foi possível conectar ao servidor.", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await Shell.Current.DisplayAlert("Erro", "Falha ao carregar ordens de produção.", "OK");
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        private async Task OnStartProductionAsync(ProductionOrder order)
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
