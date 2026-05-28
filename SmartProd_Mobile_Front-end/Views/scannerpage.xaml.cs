using SmartProd.ViewModels;
using SmartProd_Mobile_Front_end.Services;

namespace SmartProd_Mobile_Front_end.Views;

public partial class ScannerPage : ContentPage
{
	public ScannerPage(ApiService apiService)
	{
		InitializeComponent();
		BindingContext = new ScannerViewModel(apiService);
	}
}