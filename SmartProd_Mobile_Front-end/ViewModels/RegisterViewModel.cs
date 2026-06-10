using SmartProd_Mobile_Front_end.Services;

namespace SmartProd.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        private string _fullName = string.Empty;
        private string _company = string.Empty;
        private string _email = string.Empty;
        private string _senha = string.Empty;
        private bool _isBusy;

        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        public string Company
        {
            get => _company;
            set => SetProperty(ref _company, value);
        }

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

        public Command RegisterCommand { get; }

        public RegisterViewModel(ApiService apiService)
        {
            _apiService = apiService;
            RegisterCommand = new Command(async () => await ExecuteRegisterAsync());
        }

        private async Task ExecuteRegisterAsync()
        {
            if (IsBusy) return;

            if (string.IsNullOrWhiteSpace(FullName) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Senha))
            {
                await Application.Current.Windows[0].Page.DisplayAlert(
                    "Erro",
                    "Preencha todos os campos obrigatórios.",
                    "OK");
                return;
            }

            try
            {
                IsBusy = true;

                var (response, error) = await _apiService.RegisterAsync(FullName, Company, Email, Senha);

                if (error != null)
                {
                    await Application.Current.Windows[0].Page.DisplayAlert("Erro", error, "OK");
                    return;
                }

                await Application.Current.Windows[0].Page.DisplayAlert(
                    "Sucesso",
                    "Conta criada com sucesso! Faça o login.",
                    "OK");

                await Application.Current.Windows[0].Page.Navigation.PopAsync();
            }
            catch (HttpRequestException)
            {
                await Application.Current.Windows[0].Page.DisplayAlert(
                    "Erro de Conexão",
                    "Não foi possível conectar ao servidor.",
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
