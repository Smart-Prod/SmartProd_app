using SmartProd_Mobile_Front_end.Services;

namespace SmartProd.ViewModels
{
    public class RegistrationViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        private string _name = string.Empty;
        private string _email = string.Empty;
        private string _senha = string.Empty;
        private bool _isBusy;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
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

        public RegistrationViewModel(ApiService apiService)
        {
            _apiService = apiService;
            RegisterCommand = new Command(async () => await ExecuteRegisterAsync());
        }

        private async Task ExecuteRegisterAsync()
        {
            if (IsBusy)
                return;

            if (string.IsNullOrWhiteSpace(Name) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Senha))
            {
                await Shell.Current.DisplayAlert("Erro", "Preencha todos os campos.", "OK");
                return;
            }

            try
            {
                IsBusy = true;

                var (message, error) = await _apiService.RegisterAsync(Name, Email, Senha);

                if (error != null)
                {
                    await Shell.Current.DisplayAlert("Erro", error, "OK");
                    return;
                }

                await Shell.Current.DisplayAlert("Sucesso", message ?? "Conta criada!", "OK");
                await Shell.Current.GoToAsync("//LoginPage");
            }
            catch (HttpRequestException)
            {
                await Shell.Current.DisplayAlert("Erro de Conexão", "Não foi possível conectar ao servidor.", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await Shell.Current.DisplayAlert("Erro", "Ocorreu um erro inesperado.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}