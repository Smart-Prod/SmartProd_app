using SmartProd.ViewModels;
using SmartProd_Mobile_Front_end.Services;

namespace SmartProd_Mobile_Front_end.Views;

public partial class ProductionPage : ContentPage
{
    public ProductionPage(ApiService apiService)
    {
        InitializeComponent();
        BindingContext = new ProductionPageViewModel(apiService);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Shell.SetNavBarIsVisible(this, false);
    }
}
