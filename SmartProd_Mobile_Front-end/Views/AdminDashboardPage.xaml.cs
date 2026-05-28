using SmartProd.ViewModels;
using SmartProd_Mobile_Front_end.Services;
namespace SmartProd_Mobile_Front_end.Views;

public partial class AdminDashboardPage : ContentPage
{
	public AdminDashboardPage(ApiService apiService)
	{
		InitializeComponent();
		BindingContext = new AdminDashboardPage_ViewsModel(apiService);
	}
    protected override void OnAppearing()
    {
        base.OnAppearing();
        Shell.SetNavBarIsVisible(this, false);
    }
}