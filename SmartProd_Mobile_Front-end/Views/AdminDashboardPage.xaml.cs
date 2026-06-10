using SmartProd.ViewModels;
using SmartProd_Mobile_Front_end.Services;
namespace SmartProd_Mobile_Front_end.Views;

public partial class AdminDashboardPage : ContentPage
{
    private readonly AdminDashboardPage_ViewsModel _viewModel;

    public AdminDashboardPage(ApiService apiService)
    {
        InitializeComponent();
        _viewModel = new AdminDashboardPage_ViewsModel(apiService);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Shell.SetNavBarIsVisible(this, false);
        try
        {
            await _viewModel.LoadDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Dashboard] Erro: {ex.Message}");
        }
    }
}
