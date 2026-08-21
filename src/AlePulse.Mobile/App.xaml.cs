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
        // Abre direto na LoginPage sem travar a tela
        return new Window(new LoginPage());
    }
}