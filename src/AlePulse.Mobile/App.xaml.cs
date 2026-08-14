using AlePulse.Mobile.Services;
using AlePulse.Mobile.Views;
using Microsoft.Extensions.DependencyInjection;

namespace AlePulse.Mobile;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Verifica se o token está salvo no celular
        bool isLogged = ApiService.IsUserLoggedInAsync().Result;

        if (isLogged)
            return new Window(new HomePage());
        else
            return new Window(new LoginPage());
    }
}