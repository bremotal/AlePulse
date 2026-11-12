using AlePulse.Mobile.Services;
using AlePulse.Mobile.Views;

namespace AlePulse.Mobile;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        CrashReportService.AddBreadcrumb("App iniciado");
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new LoginPage());
    }

    private void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
    {
        CrashReportService.LogException("AppDomain.UnhandledException", e.ExceptionObject as Exception);
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        CrashReportService.LogException("TaskScheduler.UnobservedTaskException", e.Exception);
        e.SetObserved();
    }
}
