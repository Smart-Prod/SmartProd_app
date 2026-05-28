using Microsoft.Extensions.Logging;
using SmartProd_Mobile_Front_end.Services;
using SmartProd_Mobile_Front_end.Views;

namespace SmartProd_Mobile_Front_end
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<HttpClient>(sp =>
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:5481");
                client.Timeout = TimeSpan.FromSeconds(30);
                return client;
            });

            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegistrationPage>();
            builder.Services.AddTransient<ProductionPage>();
            builder.Services.AddTransient<ScannerPage>();
            builder.Services.AddTransient<AdminDashboardPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
