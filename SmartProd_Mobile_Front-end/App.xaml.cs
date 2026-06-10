using SmartProd_Mobile_Front_end.Views;
using Microsoft.Extensions.DependencyInjection;

namespace SmartProd_Mobile_Front_end
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var loginPage = IPlatformApplication.Current!.Services.GetRequiredService<LoginPage>();
            var navPage = new NavigationPage(loginPage)
            {
                BarBackgroundColor = Colors.White,
                BarTextColor = Color.FromArgb("#FF8C00")
            };
            NavigationPage.SetHasNavigationBar(loginPage, false);

            var janela = new Window(navPage);

            janela.Width = 350;
            janela.Height = 700;

            return janela;
        }
    }
}