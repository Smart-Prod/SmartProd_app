using SmartProd_Mobile_Front_end.Models;
using SmartProd_Mobile_Front_end.Services;

namespace SmartProd.ViewModels
{
    public class Loginpage_ViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        private string _email = string.Empty;
        private string _senha = string.Empty;
        private bool _isBusy;

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Senha
        {
            get => _senha;
            set => SetProperty(ref _senha, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public Command LoginCommand { get; }

        public Loginpage_ViewModel(ApiService apiService)
        {
            _apiService = apiService;
            LoginCommand = new Command(async () => await ExecuteLoginAsync());
        }

        private async Task ExecuteLoginAsync()
        {
            if (IsBusy)
                return;

            if (string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Senha))
            {
                await Application.Current.Windows[0].Page.DisplayAlert(
                    "Erro",
                    "Preencha e-mail e senha.",
                    "OK");
                return;
            }

            try
            {
                IsBusy = true;

                var (loginResponse, error) = await _apiService.LoginAsync(Email, Senha);

                if (error != null || loginResponse == null)
                {
                    await Application.Current.Windows[0].Page.DisplayAlert(
                        "Erro",
                        error ?? "Falha no login. Verifique suas credenciais.",
                        "OK");
                    return;
                }

                if (loginResponse.User != null)
                {
                    Application.Current.Windows[0].Page = new SmartProd_Mobile_Front_end.AppShell();
                }
            }
            catch (HttpRequestException)
            {
                await Application.Current.Windows[0].Page.DisplayAlert(
                    "Erro de Conexão",
                    "Não foi possível conectar ao servidor. Verifique se o backend está rodando em http://localhost:5481.",
                    "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await Application.Current.Windows[0].Page.DisplayAlert(
                    "Erro",
                    "Ocorreu um erro inesperado.",
                    "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
