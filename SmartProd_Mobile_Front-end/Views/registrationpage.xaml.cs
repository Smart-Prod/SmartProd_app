using SmartProd.ViewModels;
using SmartProd_Mobile_Front_end.Services;

namespace SmartProd_Mobile_Front_end.Views;

public partial class RegistrationPage : ContentPage
{
	public RegistrationPage(ApiService apiService)
	{
		InitializeComponent();
		BindingContext = new RegisterViewModel(apiService);
	}

    private async void OnSignInTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
