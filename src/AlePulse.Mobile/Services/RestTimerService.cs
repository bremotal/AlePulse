namespace AlePulse.Mobile.Services;

public static class RestTimerService
{
    private static DateTime _endTimeUtc;
    private static bool _running;
    private static bool _tickerStarted;

    public static event Action<int>? Tick;
    public static event Action? Finished;

    public static bool IsRunning => _running;

    public static void Start(int seconds)
    {
        _endTimeUtc = DateTime.UtcNow.AddSeconds(seconds);
        _running = true;
        EnsureTicker();
        EmitTick();
    }

    public static void AddSeconds(int seconds)
    {
        if (!_running) return;
        _endTimeUtc = _endTimeUtc.AddSeconds(seconds);
        EmitTick();
    }

    public static void Skip()
    {
        if (!_running) return;
        _running = false;
        EmitFinished();
    }

    public static int GetRemainingSeconds()
    {
        if (!_running) return 0;
        double remaining = (_endTimeUtc - DateTime.UtcNow).TotalSeconds;
        return remaining <= 0 ? 0 : (int)Math.Ceiling(remaining);
    }

    private static void EnsureTicker()
    {
        if (_tickerStarted) return;
        _tickerStarted = true;

        Device.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            try
            {
                if (_running)
                {
                    if ((_endTimeUtc - DateTime.UtcNow).TotalSeconds <= 0)
                    {
                        _running = false;
                        EmitFinished();
                    }
                    else
                    {
                        EmitTick();
                    }
                }
            }
            catch
            {
            }

            return true;
        });
    }

    private static void EmitTick()
    {
        int remaining = GetRemainingSeconds();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                Tick?.Invoke(remaining);
            }
            catch
            {
            }
        });
    }

    private static void EmitFinished()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                PlayAlert();
            }
            catch
            {
            }

            try
            {
                Finished?.Invoke();
            }
            catch
            {
            }
        });
    }

    private static void PlayAlert()
    {
#if ANDROID
        try
        {
            var uri = Android.Media.RingtoneManager.GetDefaultUri(Android.Media.RingtoneType.Notification);
            var ringtone = Android.Media.RingtoneManager.GetRingtone(Android.App.Application.Context, uri);
            ringtone?.Play();
        }
        catch
        {
        }
#endif

        try
        {
            Vibration.Default.Vibrate(TimeSpan.FromSeconds(1));
        }
        catch
        {
        }
    }
}
