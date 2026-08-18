using AlePulse.Mobile.Views;

namespace AlePulse.Mobile;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Abre direto na LoginPage.
        // A LoginPage vai verificar em background se já existe token salvo.
        return new Window(new LoginPage());
    }
}