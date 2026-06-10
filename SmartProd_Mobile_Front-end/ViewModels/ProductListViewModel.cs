using System.Collections.ObjectModel;
using System.Windows.Input;
using SmartProd_Mobile_Front_end.Models;
using SmartProd_Mobile_Front_end.Services;

namespace SmartProd.ViewModels
{
    public class ProductListViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private bool _isBusy;
        private bool _isRefreshing;

        public ObservableCollection<Produto> Products { get; set; } = new();

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand ShowQrCommand { get; }
        public ICommand ExportPdfCommand { get; }

        public ProductListViewModel(ApiService apiService)
        {
            _apiService = apiService;

            RefreshCommand = new Command(async () => await LoadProductsAsync());
            ShowQrCommand = new Command<Produto>(async (product) => await OnShowQrAsync(product));
            ExportPdfCommand = new Command(async () => await OnExportPdfAsync());
        }

        public async Task LoadProductsAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                IsRefreshing = true;

                var (produtos, error) = await _apiService.GetProductsAsync();

                if (error != null)
                {
                    await Shell.Current.DisplayAlertAsync("Erro", error, "OK");
                    return;
                }

                Products.Clear();
                if (produtos != null)
                {
                    foreach (var p in produtos.OrderBy(p => p.Code))
                        Products.Add(p);
                }
            }
            catch (HttpRequestException)
            {
                await Shell.Current.DisplayAlertAsync("Erro", "Não foi possível conectar ao servidor.", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await Shell.Current.DisplayAlertAsync("Erro", "Falha ao carregar produtos.", "OK");
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        private async Task OnShowQrAsync(Produto? product)
        {
            if (product == null || string.IsNullOrWhiteSpace(product.Code)) return;

            try
            {
                StatusMessage = "Gerando QR Code...";

                var (imageBytes, error) = await _apiService.GetProductQrCodeAsync(product.Code);

                if (error != null || imageBytes == null)
                {
                    await Shell.Current.DisplayAlertAsync("Erro", error ?? "Falha ao gerar QR Code", "OK");
                    return;
                }

                var imageSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));

                var popupContent = new VerticalStackLayout
                {
                    Spacing = 16,
                    Padding = new Thickness(24),
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    BackgroundColor = Colors.White
                };

                var qrImage = new Image
                {
                    Source = imageSource,
                    WidthRequest = 300,
                    HeightRequest = 300,
                    HorizontalOptions = LayoutOptions.Center
                };

                var productInfo = new VerticalStackLayout
                {
                    Spacing = 4,
                    HorizontalOptions = LayoutOptions.Center
                };
                productInfo.Children.Add(new Label
                {
                    Text = product.Code,
                    FontSize = 20,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalTextAlignment = TextAlignment.Center,
                    TextColor = Color.FromArgb("#333333")
                });
                productInfo.Children.Add(new Label
                {
                    Text = product.Name,
                    FontSize = 14,
                    HorizontalTextAlignment = TextAlignment.Center,
                    TextColor = Color.FromArgb("#636E72")
                });

                var shareButton = new Button
                {
                    Text = "COMPARTILHAR",
                    BackgroundColor = Color.FromArgb("#FF8C00"),
                    TextColor = Colors.White,
                    FontAttributes = FontAttributes.Bold,
                    CornerRadius = 8,
                    HeightRequest = 48
                };
                shareButton.Clicked += async (s, e) =>
                {
                    var filePath = Path.Combine(FileSystem.CacheDirectory, $"qrcode-{product.Code}.png");
                    await File.WriteAllBytesAsync(filePath, imageBytes);

                    await Share.Default.RequestAsync(new ShareFileRequest
                    {
                        Title = $"QR Code - {product.Code}",
                        File = new ShareFile(filePath)
                    });
                };

                popupContent.Children.Add(qrImage);
                popupContent.Children.Add(productInfo);
                popupContent.Children.Add(shareButton);

                var closeButton = new Button
                {
                    Text = "FECHAR",
                    BackgroundColor = Color.FromArgb("#636E72"),
                    TextColor = Colors.White,
                    FontAttributes = FontAttributes.Bold,
                    CornerRadius = 8,
                    HeightRequest = 44
                };
                closeButton.Clicked += async (s, e) =>
                {
                    await Shell.Current.Navigation.PopModalAsync();
                };
                popupContent.Children.Add(closeButton);

                var page = new ContentPage
                {
                    BackgroundColor = Colors.Black.WithAlpha(0.6f),
                    Content = new Border
                    {
                        StrokeThickness = 0,
                        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
                        Padding = new Thickness(0),
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        BackgroundColor = Colors.White,
                        Content = popupContent
                    }
                };

                await Shell.Current.Navigation.PushModalAsync(page);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductList] Erro QR: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("Erro", "Falha ao gerar QR Code.", "OK");
            }
            finally
            {
                StatusMessage = string.Empty;
            }
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private async Task OnExportPdfAsync()
        {
            try
            {
                StatusMessage = "Gerando PDF...";

                var (pdfBytes, error) = await _apiService.ExportQrCodesPdfAsync("MP");

                if (error != null || pdfBytes == null)
                {
                    await Shell.Current.DisplayAlertAsync("Erro", error ?? "Falha ao exportar PDF", "OK");
                    return;
                }

                var filePath = Path.Combine(FileSystem.CacheDirectory, "etiquetas-smartprod.pdf");
                await File.WriteAllBytesAsync(filePath, pdfBytes);

                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Etiquetas SmartProd",
                    File = new ShareFile(filePath)
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductList] Erro PDF: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("Erro", "Falha ao exportar PDF.", "OK");
            }
            finally
            {
                StatusMessage = string.Empty;
            }
        }
    }
}
