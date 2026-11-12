using Android.App;
using Android.Runtime;
using AlePulse.Mobile.Services;

namespace AlePulse.Mobile;

[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override void OnCreate()
    {
        base.OnCreate();
        var defaultHandler = Java.Lang.Thread.DefaultUncaughtExceptionHandler;
        Java.Lang.Thread.DefaultUncaughtExceptionHandler = new CrashForwardHandler(defaultHandler);
    }

    private class CrashForwardHandler : Java.Lang.Object, Java.Lang.Thread.IUncaughtExceptionHandler
    {
        private readonly Java.Lang.Thread.IUncaughtExceptionHandler? _default;

        public CrashForwardHandler(Java.Lang.Thread.IUncaughtExceptionHandler? defaultHandler)
        {
            _default = defaultHandler;
        }

        public void UncaughtException(Java.Lang.Thread thread, Java.Lang.Throwable throwable)
        {
            var message = $"{thread?.Name}: {throwable?.Message}\n{throwable?.StackTrace}";
            CrashReportService.LogException("Java.UncaughtException", new Exception(message));
            _default?.UncaughtException(thread!, throwable!);
        }
    }
}
