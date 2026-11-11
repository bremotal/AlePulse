using AlePulse.Mobile.Views;

namespace AlePulse.Mobile;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new LoginPage());
    }

    private async void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        if (ex != null && MainPage != null)
        {
            await MainPage.DisplayAlertAsync("Erro Capturado (App)", ex.Message, "OK");
        }
    }

    private async void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        if (MainPage != null)
        {
            await MainPage.DisplayAlertAsync("Erro Capturado (Task)", e.Exception.Message, "OK");
        }
        e.SetObserved();
    }
}