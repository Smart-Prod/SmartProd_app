using Microsoft.Extensions.Logging;
using SmartProd_Mobile_Front_end.Services;
using SmartProd_Mobile_Front_end.Views;
using ZXing.Net.Maui.Controls;

namespace SmartProd_Mobile_Front_end
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseBarcodeReader()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddTransient<ApiEnvelopeHandler>();
            builder.Services.AddSingleton<HttpClient>(sp =>
            {
                var handler = sp.GetRequiredService<ApiEnvelopeHandler>();
                handler.InnerHandler = new HttpClientHandler();
                var client = new HttpClient(handler);
                client.BaseAddress = new Uri("http://localhost:5481");
                client.Timeout = TimeSpan.FromSeconds(30);
                return client;
            });

            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddSingleton<INfcService, NfcService>();

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
