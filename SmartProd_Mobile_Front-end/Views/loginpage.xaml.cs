using SmartProd.ViewModels;
using SmartProd_Mobile_Front_end.Services;
namespace SmartProd_Mobile_Front_end.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(ApiService apiService)
    {
        InitializeComponent();
        BindingContext = new Loginpage_ViewModel(apiService);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Shell.SetNavBarIsVisible(this, false);
    }
}
