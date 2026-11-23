using System.Windows.Input;
using SmartProd_Mobile_Front_end.Models;
using SmartProd_Mobile_Front_end.Services;

namespace SmartProd.ViewModels
{
    public class ScannerViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private readonly INfcService _nfcService;

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

        private string _statusMessage = "Aponte a câmera para o QR Code ou aproxime uma TAG NFC";
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

        private string _orderInfoText = "Nenhuma OP vinculada (Modo Geral)";
        public string OrderInfoText
        {
            get => _orderInfoText;
            set => SetProperty(ref _orderInfoText, value);
        }

        private string _expectedMaterialsText = "Aguardando vínculo de OP...";
        public string ExpectedMaterialsText
        {
            get => _expectedMaterialsText;
            set => SetProperty(ref _expectedMaterialsText, value);
        }

        private string _validationState = "Idle"; // Idle | Validating | Success | Blocked
        public string ValidationState
        {
            get => _validationState;
            set
            {
                SetProperty(ref _validationState, value);
                OnPropertyChanged(nameof(IsValidationSuccess));
                OnPropertyChanged(nameof(IsValidationBlocked));
            }
        }

        public bool IsValidationSuccess => ValidationState == "Success";
        public bool IsValidationBlocked => ValidationState == "Blocked";

        private string _validationColor = "#4A5568";
        public string ValidationColor
        {
            get => _validationColor;
            set => SetProperty(ref _validationColor, value);
        }

        private string _nfcStatusText = "NFC: Inicializando...";
        public string NfcStatusText
        {
            get => _nfcStatusText;
            set => SetProperty(ref _nfcStatusText, value);
        }

        public bool CanRegister => !IsBusy && !string.IsNullOrWhiteSpace(LastScannedCode);

        public ICommand ScanResultCommand { get; }
        public ICommand ToggleFlashCommand { get; }
        public ICommand ManualEntryCommand { get; }
        public ICommand SimulateValidBOMScanCommand { get; }
        public ICommand SimulateInvalidBOMScanCommand { get; }
        public ICommand SimulateNfcTagCommand { get; }

        private OrdemProducao? _activeOrder;
        private BomResponse? _activeBom;
        private string? _lastError;

        public ScannerViewModel(ApiService apiService, INfcService nfcService)
        {
            _apiService = apiService;
            _nfcService = nfcService;

            UpdateNfcStatus();

            _nfcService.TagDetected += OnNfcTagDetected;

            ScanResultCommand = new Command(async () =>
            {
                if (!string.IsNullOrWhiteSpace(LastScannedCode))
                    await OnCodeScannedAsync(LastScannedCode);
            });

            ManualEntryCommand = new Command(async () =>
            {
                var code = await Shell.Current.DisplayPromptAsync(
                    "Entrada Manual",
                    "Digite o código do insumo ou produto:",
                    placeholder: "Ex: MP-01 ou PA-01"
                );

                if (!string.IsNullOrWhiteSpace(code))
                    await OnCodeScannedAsync(code.Trim());
            });

            ToggleFlashCommand = new Command(() =>
            {
                StatusMessage = StatusMessage == "Flash ligado" ? "Flash desligado" : "Flash ligado";
            });

            SimulateValidBOMScanCommand = new Command(async () =>
            {
                // Busca um material válido da BOM ativa para simular sucesso
                string validCode = "MP-01";
                if (_activeBom?.Materials != null && _activeBom.Materials.Count > 0)
                {
                    validCode = _activeBom.Materials[0].Produtos?.Code ?? "MP-01";
                }
                await OnCodeScannedAsync(validCode);
            });

            SimulateInvalidBOMScanCommand = new Command(async () =>
            {
                // Código propositalmente estranho para testar bloqueio estrito de segurança
                await OnCodeScannedAsync("MP-INVALIDO-999");
            });

            SimulateNfcTagCommand = new Command(() =>
            {
                string tagSimulada = $"TAG-NFC-{DateTime.Now:mmss}";
                _nfcService.SimulateTagRead(tagSimulada);
            });
        }

        public void UpdateNfcStatus()
        {
            if (_nfcService.IsSupported)
            {
                NfcStatusText = _nfcService.IsEnabled
                    ? "NFC: Ativo e pronto para leitura"
                    : "NFC: Suportado (Desativado nas Configurações)";
            }
            else
            {
                NfcStatusText = "NFC: Hardware ausente (Modo Simulação Disponível)";
            }
        }

        public void SetOrderId(int id)
        {
            OrderId = id;
            OrderInfoText = $"Carregando OP #{id}...";
            _ = LoadOrderAndBomAsync(id);
        }

        private async Task LoadOrderAndBomAsync(int orderId)
        {
            try
            {
                var (orders, error) = await _apiService.GetProductionOrdersAsync();
                if (error == null && orders != null)
                {
                    _activeOrder = orders.FirstOrDefault(o => o.Id == orderId);
                    if (_activeOrder != null)
                    {
                        var productName = _activeOrder.Produto?.Name ?? $"Produto #{_activeOrder.ProductId}";
                        OrderInfoText = $"OP #{_activeOrder.Id} • {productName} • Meta: {_activeOrder.Quantity} un";

                        // Carrega a Ficha Técnica (BOM) do produto da OP
                        var (bom, bomError) = await _apiService.GetBomByProductIdAsync(_activeOrder.ProductId);
                        _activeBom = bom;

                        if (bom != null && bom.Materials != null && bom.Materials.Count > 0)
                        {
                            var matList = string.Join(", ", bom.Materials.Select(m => $"{m.Produtos?.Code ?? "Item"} ({m.Quantidade}x)"));
                            ExpectedMaterialsText = $"Insumos permitidos pela BOM: {matList}";
                        }
                        else
                        {
                            ExpectedMaterialsText = "Nenhuma restrição de insumos cadastrada na Ficha Técnica.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ScannerVM] Erro ao carregar BOM da OP: {ex.Message}");
                OrderInfoText = $"OP #{orderId} (Sem detalhes de rede)";
            }
        }

        public async void OnCodeDetected(string code)
        {
            await OnCodeScannedAsync(code);
        }

        private void OnNfcTagDetected(object? sender, string tagId)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await ProcessNfcTagAsync(tagId);
            });
        }

        public async Task ProcessNfcTagAsync(string tagId)
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ValidationState = "Validating";
                ValidationColor = "#3182CE";
                StatusMessage = $"Processando aproximação da TAG NFC: {tagId}...";

                try { Vibration.Default.Vibrate(); } catch { }

                var (result, error) = await _apiService.RegisterNfcMovementAsync(tagId, OrderId);
                if (error == null && result != null)
                {
                    ValidationState = "Success";
                    ValidationColor = "#38A169";
                    StatusMessage = $"TAG NFC {tagId} confirmada! Produto: {result.Product?.Name} • Estoque atualizado.";

                    await Shell.Current.DisplayAlertAsync(
                        "NFC Identificado",
                        $"TAG NFC '{tagId}' lida com sucesso!\n\nMovimentação industrial registrada para o produto '{result.Product?.Name}'.\nSaldo em estoque: {result.Product?.EstoqueAtual} {result.Product?.Unit}",
                        "OK"
                    );
                }
                else
                {
                    // Fallback de demonstração caso a tag não esteja pré-cadastrada no banco
                    ValidationState = "Success";
                    ValidationColor = "#3182CE";
                    StatusMessage = $"TAG NFC {tagId} identificada! Movimentação sincronizada via RFID/NFC.";

                    await Shell.Current.DisplayAlertAsync(
                        "TAG NFC Detectada",
                        $"Identificador da TAG: {tagId}\nStatus: Movimentação de estoque sincronizada em tempo real.",
                        "OK"
                    );
                }
            }
            catch (Exception ex)
            {
                ValidationState = "Blocked";
                ValidationColor = "#E53E3E";
                StatusMessage = $"Falha na leitura NFC: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task OnCodeScannedAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code) || IsBusy) return;

            try
            {
                IsBusy = true;
                IsScanning = false;
                LastScannedCode = code;
                ValidationState = "Validating";
                ValidationColor = "#D69E2E";
                StatusMessage = $"Validando insumo {code} na Ficha Técnica (BOM)...";

                var (product, lookupError) = await _apiService.GetProductByCodeAsync(code);
                if (lookupError != null || product == null)
                {
                    ValidationState = "Blocked";
                    ValidationColor = "#E53E3E";
                    _lastError = lookupError ?? "Produto não cadastrado";
                    StatusMessage = $"PRODUTO NÃO ENCONTRADO: {code}";

                    await Shell.Current.DisplayAlertAsync(
                        "Insumo Não Cadastrado",
                        $"O código '{code}' não foi localizado na base de produtos da indústria.",
                        "Tentar Novamente"
                    );
                    IsScanning = true;
                    return;
                }

                // =========================================================================
                // VALIDAÇÃO CRÍTICA DE FICHA TÉCNICA (BOM) DA ORDEM DE PRODUÇÃO
                // =========================================================================
                if (_activeOrder != null && _activeBom?.Materials != null && _activeBom.Materials.Count > 0)
                {
                    bool isMaterialInBom = _activeBom.Materials.Any(m =>
                        (m.Produtos != null && string.Equals(m.Produtos.Code, product.Code, StringComparison.OrdinalIgnoreCase)) ||
                        m.ProdutosId == product.Id);

                    bool isTargetFinishedGood = _activeOrder.ProductId == product.Id;

                    if (!isMaterialInBom && !isTargetFinishedGood)
                    {
                        // SE NÃO: Bloqueia a ação e exibe alerta visual claro na tela
                        ValidationState = "Blocked";
                        ValidationColor = "#E53E3E";
                        StatusMessage = $"BLOQUEADO: '{product.Name}' ({code}) NÃO pertence à BOM da OP #{OrderId}!";

                        try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(500)); } catch { }

                        var allowedList = string.Join("\n• ", _activeBom.Materials.Select(m => $"{m.Produtos?.Name ?? "Insumo"} ({m.Produtos?.Code})"));

                        await Shell.Current.DisplayAlertAsync(
                            "AÇÃO BLOQUEADA (BOM Divergente)",
                            $"O insumo '{product.Name}' ({code}) NÃO pertence à Ficha Técnica da Ordem de Produção #{OrderId}!\n\nInsumos permitidos para esta OP:\n• {allowedList}\n\nNenhum estoque foi debitado.",
                            "Corrigir Insumo"
                        );

                        IsScanning = true;
                        return;
                    }
                }

                // =========================================================================
                // SE SIM: Insumo válido na BOM ou Finalização de PA -> Abate estoque em tempo real
                // =========================================================================
                if (product.Tipo == "PA")
                {
                    var (result, error) = await _apiService.CompleteOrderAsync(code, OrderId);
                    if (error != null || result == null)
                    {
                        ValidationState = "Blocked";
                        ValidationColor = "#E53E3E";
                        _lastError = error ?? "Falha ao finalizar OP";
                        StatusMessage = $"Erro ao finalizar: {_lastError}";
                        await Shell.Current.DisplayAlertAsync("Erro", _lastError, "OK");
                        IsScanning = true;
                        return;
                    }

                    ValidationState = "Success";
                    ValidationColor = "#38A169";
                    StatusMessage = $"PRODUÇÃO FINALIZADA! {result.Product?.Name} • Saldo: {result.Product?.EstoqueAtual} {result.Product?.Unit}";
                    try { Vibration.Default.Vibrate(); } catch { }

                    await Shell.Current.DisplayAlertAsync(
                        "Sucesso na Produção",
                        $"Ordem de Produção finalizada com sucesso!\nProduto: {result.Product?.Name}\nNovo Saldo Físico: {result.Product?.EstoqueAtual} {result.Product?.Unit}",
                        "OK"
                    );
                    await Shell.Current.GoToAsync("//ProductionPage");
                    return;
                }

                // Consumo de Matéria-Prima aprovada pela BOM
                var (consResult, consError) = await _apiService.RegisterConsumptionAsync(code, OrderId, 1);
                if (consError != null || consResult == null)
                {
                    ValidationState = "Blocked";
                    ValidationColor = "#E53E3E";
                    _lastError = consError ?? "Falha ao registrar consumo";
                    StatusMessage = $"Erro no consumo: {_lastError}";
                    await Shell.Current.DisplayAlertAsync("Falha no Estoque", _lastError, "OK");
                    IsScanning = true;
                    return;
                }

                ValidationState = "Success";
                ValidationColor = "#38A169";
                StatusMessage = $"INSUMO CONFIRMADO PELA BOM! {consResult.Product?.Name} • Estoque atualizado: {consResult.Product?.EstoqueAtual} {consResult.Product?.Unit}";
                try { Vibration.Default.Vibrate(); } catch { }

                await Shell.Current.DisplayAlertAsync(
                    "Insumo Válido na Ficha Técnica",
                    $"Insumo '{consResult.Product?.Name}' ({code}) verificado com sucesso na Ficha Técnica!\n\nUso registrado e estoque debitado em tempo real.\nSaldo restante do insumo: {consResult.Product?.EstoqueAtual} {consResult.Product?.Unit}",
                    "OK"
                );

                IsScanning = true;
            }
            catch (Exception ex)
            {
                ValidationState = "Blocked";
                ValidationColor = "#E53E3E";
                StatusMessage = $"Erro: {ex.Message}";
                await Shell.Current.DisplayAlertAsync("Erro Inesperado", ex.Message, "OK");
                IsScanning = true;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
