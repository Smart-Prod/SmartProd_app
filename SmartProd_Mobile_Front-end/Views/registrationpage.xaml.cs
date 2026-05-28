using SmartProd.ViewModels;
using SmartProd_Mobile_Front_end.Services;

namespace SmartProd_Mobile_Front_end.Views;

public partial class RegistrationPage : ContentPage
{
	public RegistrationPage(ApiService apiService)
	{
		InitializeComponent();
		BindingContext = new RegistrationViewModel(apiService);
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Shell.SetNavBarIsVisible(this, false);
    }
}