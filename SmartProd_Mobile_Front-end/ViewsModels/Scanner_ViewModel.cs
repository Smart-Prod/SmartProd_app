using System.Windows.Input;
using SmartProd_Mobile_Front_end.Services;

namespace SmartProd.ViewModels
{
    public class ScannerViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

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

        private string _shiftProgress = "84%";
        public string ShiftProgress
        {
            get => _shiftProgress;
            set => SetProperty(ref _shiftProgress, value);
        }

        public bool CanRegister => !IsBusy && !string.IsNullOrWhiteSpace(LastScannedCode);

        public ICommand ScanResultCommand { get; }
        public ICommand ToggleFlashCommand { get; }
        public ICommand ManualEntryCommand { get; }

        public ScannerViewModel(ApiService apiService)
        {
            _apiService = apiService;

            ScanResultCommand = new Command<string>(
                async (result) => await OnCodeScannedAsync(result),
                (result) => !IsBusy
            );

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
                StatusMessage = "Flash alternado";
            });
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

                bool isValid = await ProcessInsumoAsync(code);

                if (isValid)
                {
                    Vibration.Default.Vibrate();
                    await Shell.Current.DisplayAlert(
                        "Sucesso",
                        $"Insumo {code} registrado com sucesso!",
                        "OK"
                    );
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Shell.Current.DisplayAlert(
                        "Erro",
                        "QR Code inválido ou não reconhecido.",
                        "Tentar Novamente"
                    );
                    IsScanning = true;
                    LastScannedCode = string.Empty;
                    StatusMessage = "Aponte a câmera para o QR Code";
                }
            }
            catch (HttpRequestException)
            {
                await Shell.Current.DisplayAlert(
                    "Sem conexão",
                    "Verifique sua conexão com a internet e tente novamente.",
                    "OK"
                );
                IsScanning = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ScannerVM] Erro: {ex.Message}");
                await Shell.Current.DisplayAlert(
                    "Falha no Scanner",
                    "Ocorreu um erro ao processar a leitura.",
                    "OK"
                );
                IsScanning = true;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task<bool> ProcessInsumoAsync(string code)
        {
            var (produtos, error) = await _apiService.GetProductsAsync();
            if (error != null || produtos == null)
                return false;

            var produto = produtos.FirstOrDefault(p =>
                p.Code?.Equals(code, StringComparison.OrdinalIgnoreCase) == true ||
                p.Name?.Equals(code, StringComparison.OrdinalIgnoreCase) == true);

            if (produto == null)
                return false;

            var (success, movError) = await _apiService.CreateMovimentacaoAsync(
                produto.Id, "CONSUMO", 1);

            if (!success)
                return false;

            return true;
        }
    }
}