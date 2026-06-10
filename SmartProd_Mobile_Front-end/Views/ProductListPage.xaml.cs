using SmartProd.ViewModels;
using SmartProd_Mobile_Front_end.Services;

namespace SmartProd_Mobile_Front_end.Views;

public partial class ProductListPage : ContentPage
{
    private readonly ProductListViewModel _viewModel;

    public ProductListPage(ApiService apiService)
    {
        InitializeComponent();
        _viewModel = new ProductListViewModel(apiService);
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Shell.SetNavBarIsVisible(this, false);
        _ = _viewModel.LoadProductsAsync();
    }
}
