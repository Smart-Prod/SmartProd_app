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
            var janela = new Window(new AppShell());
            janela.Width = 350;
            janela.Height = 700;
            return janela;
        }
    }
}
