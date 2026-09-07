using AlePulse.Mobile.Views;

namespace AlePulse.Mobile;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Captura erros que quebram o app em segundo plano
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new LoginPage());
    }

    // Mostra o erro na tela em vez de fechar o app silenciosamente
    private async void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        if (ex != null && MainPage != null)
        {
            await MainPage.DisplayAlertAsync("Erro Capturado (App)", ex.Message, "OK");
        }
    }

    // Mostra erros de tarefas assíncronas (como chamadas de API)
    private async void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        if (MainPage != null)
        {
            await MainPage.DisplayAlertAsync("Erro Capturado (Task)", e.Exception.Message, "OK");
        }
        e.SetObserved(); // Marca o erro como tratado para não fechar o app
    }
}