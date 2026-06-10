using SmartProd.ViewModels;
using SmartProd_Mobile_Front_end.Services;

namespace SmartProd_Mobile_Front_end.Views;

public partial class ProductionPage : ContentPage
{
    private readonly ProductionPageViewModel _viewModel;

    public ProductionPage(ApiService apiService)
    {
        InitializeComponent();
        _viewModel = new ProductionPageViewModel(apiService);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Shell.SetNavBarIsVisible(this, false);
        try
        {
            await _viewModel.LoadOrdersAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductionPage] Erro: {ex.Message}");
        }
    }
}
