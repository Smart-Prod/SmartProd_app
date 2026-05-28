using SmartProd_Mobile_Front_end.Models;
using SmartProd_Mobile_Front_end.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SmartProd.ViewModels
{
    public class AdminDashboardPage_ViewsModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        private int _totalOrders;
        public int TotalOrders
        {
            get => _totalOrders;
            set => SetProperty(ref _totalOrders, value);
        }

        private int _activeOrders;
        public int ActiveOrders
        {
            get => _activeOrders;
            set => SetProperty(ref _activeOrders, value);
        }

        private int _totalProducts;
        public int TotalProducts
        {
            get => _totalProducts;
            set => SetProperty(ref _totalProducts, value);
        }

        private int _mpCount;
        public int MPCount
        {
            get => _mpCount;
            set => SetProperty(ref _mpCount, value);
        }

        private int _paCount;
        public int PACount
        {
            get => _paCount;
            set => SetProperty(ref _paCount, value);
        }

        private int _totalMovements;
        public int TotalMovements
        {
            get => _totalMovements;
            set => SetProperty(ref _totalMovements, value);
        }

        private int _lowStockCount;
        public int LowStockCount
        {
            get => _lowStockCount;
            set => SetProperty(ref _lowStockCount, value);
        }

        private string _lowStockText = "Nenhum produto com estoque baixo";
        public string LowStockText
        {
            get => _lowStockText;
            set => SetProperty(ref _lowStockText, value);
        }

        private bool _hasLowStock;
        public bool HasLowStock
        {
            get => _hasLowStock;
            set
            {
                SetProperty(ref _hasLowStock, value);
                OnPropertyChanged(nameof(HasNormalStock));
            }
        }

        public bool HasNormalStock => !HasLowStock;

        private bool _isLoading = true;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private bool _hasData;
        public bool HasData
        {
            get => _hasData;
            set => SetProperty(ref _hasData, value);
        }

        private string _totalProduced = "0";
        public string TotalProduced
        {
            get => _totalProduced;
            set => SetProperty(ref _totalProduced, value);
        }

        private string _lastMovementInfo = "Nenhuma movimentação recente";
        public string LastMovementInfo
        {
            get => _lastMovementInfo;
            set => SetProperty(ref _lastMovementInfo, value);
        }

        public ObservableCollection<string> RecentMovements { get; } = new();

        public ICommand RefreshCommand { get; }

        public AdminDashboardPage_ViewsModel(ApiService apiService)
        {
            _apiService = apiService;
            RefreshCommand = new Command(async () => await LoadDashboardDataAsync());
            Task.Run(async () => await LoadDashboardDataAsync());
        }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                IsLoading = true;
                HasData = false;

                var produtosTask = _apiService.GetProductsAsync();
                var ordensTask = _apiService.GetProductionOrdersAsync();
                var movTask = _apiService.GetStockMovementsAsync();

                await Task.WhenAll(produtosTask, ordensTask, movTask);

                var (produtos, _) = await produtosTask;
                var (ordens, _) = await ordensTask;
                var (movResponse, _) = await movTask;

                if (produtos != null)
                {
                    TotalProducts = produtos.Count;
                    MPCount = produtos.Count(p => p.Tipo == "MP");
                    PACount = produtos.Count(p => p.Tipo == "PA");
                    LowStockCount = produtos.Count(p => p.EstoqueAtual <= p.EstoqueMinimo);

                    var lowStockItems = produtos.Where(p => p.EstoqueAtual <= p.EstoqueMinimo).ToList();
                    if (lowStockItems.Count > 0)
                    {
                        HasLowStock = true;
                        LowStockText = string.Join(", ", lowStockItems.Take(3).Select(p => $"{p.Name} ({p.EstoqueAtual}/{p.EstoqueMinimo})"));
                        if (lowStockItems.Count > 3)
                            LowStockText += $" e mais {lowStockItems.Count - 3}";
                    }
                    else
                    {
                        HasLowStock = false;
                        LowStockText = "Estoque normal";
                    }
                }

                if (ordens != null)
                {
                    TotalOrders = ordens.Count;
                    ActiveOrders = ordens.Count(o => o.Status == "EM_PRODUCAO" || o.Status == "PLANEJADA");
                    var producedTotal = ordens.Where(o => o.Status == "CONCLUIDA").Sum(o => o.Produced);
                    TotalProduced = producedTotal.ToString("N0");
                }

                if (movResponse != null)
                {
                    TotalMovements = movResponse.Total;
                    RecentMovements.Clear();
                    if (movResponse.Movements != null)
                    {
                        foreach (var m in movResponse.Movements.Take(5))
                        {
                            var tipo = m.Tipo switch
                            {
                                "ENTRADA" => "Entrada",
                                "SAIDA" => "Saída",
                                "PRODUCAO" => "Produção",
                                "CONSUMO" => "Consumo",
                                _ => m.Tipo ?? "Mov."
                            };
                            var produtoNome = m.Produto?.Name ?? $"Produto #{m.ProductId}";
                            RecentMovements.Add($"{tipo} - {produtoNome} ({m.Quantity:N2})");
                        }
                    }
                    if (movResponse.Movements != null && movResponse.Movements.Count > 0)
                    {
                        var last = movResponse.Movements[0];
                        var tipo = last.Tipo switch
                        {
                            "ENTRADA" => "Entrada",
                            "SAIDA" => "Saída",
                            "PRODUCAO" => "Produção",
                            "CONSUMO" => "Consumo",
                            _ => last.Tipo ?? "Mov."
                        };
                        var produtoNome = last.Produto?.Name ?? $"Produto #{last.ProductId}";
                        LastMovementInfo = $"{tipo} de {last.Quantity:N2} - {produtoNome}";
                    }
                }

                HasData = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DashboardVM] {ex.Message}");
                HasData = false;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
