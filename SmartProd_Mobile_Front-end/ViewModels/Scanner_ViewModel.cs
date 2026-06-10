using System.Windows.Input;
using SmartProd_Mobile_Front_end.Models;
using SmartProd_Mobile_Front_end.Services;

namespace SmartProd.ViewModels
{
    public class ScannerViewModel : BaseViewModel
    {
        private bool _isScanning = true;
        public bool IsScanning
        {
            get => _isScanning;
            set => SetProperty(ref _isScanning, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                SetProperty(ref _isBusy, value);
                OnPropertyChanged(nameof(CanRegister));
            }
        }

        private string _statusMessage = "Aponte a câmera para o QR Code";
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private string? _lastScannedCode;
        public string? LastScannedCode
        {
            get => _lastScannedCode;
            set
            {
                SetProperty(ref _lastScannedCode, value);
                OnPropertyChanged(nameof(CanRegister));
            }
        }

        private string _shiftProgress = "—";
        public string ShiftProgress
        {
            get => _shiftProgress;
            set => SetProperty(ref _shiftProgress, value);
        }

        private string _deviceStatus = "ONLINE";
        public string DeviceStatus
        {
            get => _deviceStatus;
            set => SetProperty(ref _deviceStatus, value);
        }

        private int? _orderId;
        public int? OrderId
        {
            get => _orderId;
            set => SetProperty(ref _orderId, value);
        }

        public bool CanRegister => !IsBusy && !string.IsNullOrWhiteSpace(LastScannedCode);

        public ICommand ScanResultCommand { get; }
        public ICommand ToggleFlashCommand { get; }
        public ICommand ManualEntryCommand { get; }

        private readonly ApiService _apiService;
        private string? _lastError;

        public ScannerViewModel(ApiService apiService)
        {
            _apiService = apiService;

            ScanResultCommand = new Command(async () =>
            {
                if (!string.IsNullOrWhiteSpace(LastScannedCode))
                    await OnCodeScannedAsync(LastScannedCode);
            });

            ManualEntryCommand = new Command(async () =>
            {
                var code = await Shell.Current.DisplayPromptAsync(
                    "Entrada Manual",
                    "Digite o código do insumo:",
                    placeholder: "Ex: SP-12345"
                );

                if (!string.IsNullOrWhiteSpace(code))
                    await OnCodeScannedAsync(code);
            });

            ToggleFlashCommand = new Command(() =>
            {
                StatusMessage = StatusMessage == "Flash ligado" ? "Flash desligado" : "Flash ligado";
            });
        }

        public void SetOrderId(int id)
        {
            OrderId = id;
        }

        public async void OnCodeDetected(string code)
        {
            await OnCodeScannedAsync(code);
        }

        private async Task OnCodeScannedAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code) || IsBusy) return;

            try
            {
                IsBusy = true;
                IsScanning = false;
                LastScannedCode = code;
                StatusMessage = "Processando código...";

                var success = await RegisterConsumptionAsync(code);

                if (success)
                {
                    try { Vibration.Default.Vibrate(); } catch { }
                    var isCompletion = StatusMessage.StartsWith("Produção finalizada");
                    await Shell.Current.DisplayAlertAsync(
                        "Sucesso",
                        isCompletion ? $"Produção finalizada com sucesso!" : $"Insumo {code} registrado com sucesso!",
                        "OK"
                    );
                    await Shell.Current.GoToAsync("//ProductionPage");
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync(
                        "Erro",
                        $"QR Code inválido: {_lastError}",
                        "Tentar Novamente"
                    );
                    IsScanning = true;
                    LastScannedCode = string.Empty;
                    _lastError = null;
                    StatusMessage = "Aponte a câmera para o QR Code";
                }
            }
            catch (HttpRequestException)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Sem conexão",
                    "Verifique sua conexão com a internet e tente novamente.",
                    "OK"
                );
                IsScanning = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ScannerVM] Erro: {ex}");
                await Shell.Current.DisplayAlertAsync(
                    "Falha no Scanner",
                    $"Ocorreu um erro ao processar a leitura.\n\n{ex.GetType().Name}: {ex.Message}",
                    "OK"
                );
                IsScanning = true;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task<bool> RegisterConsumptionAsync(string code)
        {
            try
            {
                var (dashboard, dashError) = await _apiService.GetDashboardAsync();
                if (dashError == null && dashboard != null)
                {
                    ShiftProgress = dashboard.ShiftProgress;
                    DeviceStatus = string.IsNullOrEmpty(dashboard.CurrentShift) ? "OFFLINE" : "ONLINE";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ScannerVM] Dashboard error (non-fatal): {ex.Message}");
            }

            var (product, lookupError) = await _apiService.GetProductByCodeAsync(code);
            if (lookupError != null || product == null)
            {
                _lastError = lookupError ?? "Produto não encontrado";
                return false;
            }

            if (product.Tipo == "PA")
            {
                var (result, error) = await _apiService.CompleteOrderAsync(code, OrderId);
                if (error != null || result == null)
                {
                    _lastError = error ?? "Resposta vazia do servidor";
                    System.Diagnostics.Debug.WriteLine($"[ScannerVM] Erro finalizar: {_lastError}");
                    return false;
                }

                StatusMessage = $"Produção finalizada! {result.Product?.Name} - Estoque: {result.Product?.EstoqueAtual} {result.Product?.Unit}";
                return true;
            }

            var (consResult, consError) = await _apiService.RegisterConsumptionAsync(code, OrderId);
            if (consError != null || consResult == null)
            {
                _lastError = consError ?? "Resposta vazia do servidor";
                System.Diagnostics.Debug.WriteLine($"[ScannerVM] Erro registro: {_lastError}");
                return false;
            }

            StatusMessage = $"{consResult.Product?.Name} - Estoque: {consResult.Product?.EstoqueAtual} {consResult.Product?.Unit}";
            return true;
        }
    }
}
