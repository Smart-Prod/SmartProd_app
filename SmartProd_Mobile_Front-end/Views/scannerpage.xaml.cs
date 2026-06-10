using SmartProd.ViewModels;
using SmartProd_Mobile_Front_end.Services;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace SmartProd_Mobile_Front_end.Views;

[QueryProperty(nameof(OpId), "opId")]
public partial class ScannerPage : ContentPage
{
    private readonly ScannerViewModel _viewModel;
    private CameraBarcodeReaderView? _cameraReader;

	public ScannerPage(ApiService apiService)
	{
		InitializeComponent();
		_viewModel = new ScannerViewModel(apiService);
		BindingContext = _viewModel;
	}

    public string? OpId
    {
        set
        {
            if (int.TryParse(value, out var id))
                _viewModel.SetOrderId(id);
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        SetupCamera();
        _viewModel.IsScanning = true;
        _viewModel.LastScannedCode = string.Empty;
        _viewModel.StatusMessage = "Aponte a câmera para o QR Code";
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.IsScanning = false;
        TeardownCamera();
    }

    private void SetupCamera()
    {
        if (_cameraReader != null)
            TeardownCamera();

        _cameraReader = new CameraBarcodeReaderView
        {
            IsDetecting = true,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            WidthRequest = 400,
            HeightRequest = 600
        };
        _cameraReader.BarcodesDetected += OnBarcodesDetected;
        Grid.SetRow(_cameraReader, 0);
        Grid.SetRowSpan(_cameraReader, 2);

        var grid = (Grid)Content;
        grid.Children.Insert(0, _cameraReader);
    }

    private void TeardownCamera()
    {
        if (_cameraReader == null) return;

        _cameraReader.IsDetecting = false;
        _cameraReader.BarcodesDetected -= OnBarcodesDetected;

        var grid = (Grid)Content;
        grid.Children.Remove(_cameraReader);

        _cameraReader = null;
    }

    private void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        if (e.Results?.Length > 0)
        {
            var code = e.Results[0].Value;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ((ScannerViewModel)BindingContext)?.OnCodeDetected(code);
            });
        }
    }
}
