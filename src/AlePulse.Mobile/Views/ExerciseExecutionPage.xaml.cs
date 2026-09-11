using AlePulse.Mobile.Models;
using AlePulse.Mobile.Services;
using System.Globalization;

namespace AlePulse.Mobile.Views;

public partial class ExerciseExecutionPage : ContentPage
{
    private readonly Guid _workoutId;
    private List<WorkoutExerciseDto> _allExercises = new();
    private int _currentIndex = 0;
    private Guid? _editingSetId = null;
    private readonly bool _isSingleExercise;

    private int _remainingSeconds;
    private bool _isTimerRunning;
    private DateTime _workoutStartTime;
    private DateTime _restEndTime;

    private string? _currentGifUrl;

    public ExerciseExecutionPage(Guid workoutId, List<WorkoutExerciseDto> exercises, bool isSingleExercise = false)
    {
        InitializeComponent();
        _workoutId = workoutId;
        _allExercises = exercises;
        _isSingleExercise = isSingleExercise;

        ExercisePicker.ItemsSource = _allExercises;
        ChangeExercise(0);

        _workoutStartTime = DateTime.Now;
    }

    private void ChangeExercise(int newIndex)
    {
        if (_allExercises.Count == 0) return;
        _currentIndex = Math.Clamp(newIndex, 0, _allExercises.Count - 1);

        var exercise = _allExercises[_currentIndex];
        ExercisePicker.SelectedIndex = _currentIndex;

        TitleLabel.Text = exercise.Exercise?.Name ?? "Exercício";
        WeightEntry.Text = exercise.Weight.ToString();
        RepsEntry.Text = exercise.Repetitions.ToString();

        if (exercise.Exercise?.Medias != null && exercise.Exercise.Medias.Count > 0)
        {
            _currentGifUrl = exercise.Exercise.Medias[0].Url;
        }
        else
        {
            _currentGifUrl = null;
        }

        _editingSetId = null;
        SaveBtn.Text = "REGISTRAR SÉRIE";

        LoadHistory();

        if (_isSingleExercise)
        {
            NextFinishBtn.Text = "FINALIZAR";
        }
        else if (_currentIndex == _allExercises.Count - 1)
        {
            NextFinishBtn.Text = "FINALIZAR TREINO";
        }
        else
        {
            NextFinishBtn.Text = "PRÓXIMO EXERCÍCIO";
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isTimerRunning)
        {
            var remaining = _restEndTime - DateTime.Now;
            if (remaining.TotalSeconds <= 0)
            {
                TimerFinished();
            }
            else
            {
                _remainingSeconds = (int)Math.Ceiling(remaining.TotalSeconds);
                UpdateTimerLabel();
            }
        }

        await LoadHistory();
    }

    private async Task LoadHistory()
    {
        try
        {
            if (_allExercises.Count == 0) return;
            var currentExerciseId = _allExercises[_currentIndex].ExerciseId != Guid.Empty
                ? _allExercises[_currentIndex].ExerciseId
                : (_allExercises[_currentIndex].Exercise?.Id ?? Guid.Empty);

            var history = await ApiService.GetHistoryAsync(currentExerciseId);

            var grouped = history
                .GroupBy(e => e.CompletedAt.Date)
                .OrderByDescending(g => g.Key)
                .Select(g => new GroupedExerciseSet(
                    g.Key.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                    g.OrderByDescending(x => x.CompletedAt).ToList()
                ))
                .ToList();

            HistoryList.ItemsSource = grouped;

            if (!_editingSetId.HasValue)
            {
                // FIX: Conta apenas as séries feitas HOJE
                var todaySets = history.Where(x => x.CompletedAt.Date == DateTime.Today).ToList();
                if (todaySets.Count > 0)
                {
                    SetEntry.Text = (todaySets.Max(x => x.SetNumber) + 1).ToString();
                }
                else
                {
                    SetEntry.Text = "1";
                }
            }
        }
        catch { }
    }

    private void OnExerciseChanged(object sender, EventArgs e)
    {
        if (ExercisePicker.SelectedIndex >= 0 && ExercisePicker.SelectedIndex != _currentIndex)
        {
            ChangeExercise(ExercisePicker.SelectedIndex);
        }
    }

    private void OnPrevClicked(object sender, EventArgs e) => ChangeExercise(_currentIndex - 1);
    private void OnNextClicked(object sender, EventArgs e) => ChangeExercise(_currentIndex + 1);

    private async void OnSaveSetClicked(object sender, EventArgs e)
    {
        SaveBtn.IsEnabled = false;

        if (!int.TryParse(SetEntry.Text, out int setNum) ||
            !decimal.TryParse(WeightEntry.Text, out decimal weight) ||
            !int.TryParse(RepsEntry.Text, out int reps))
        {
            await DisplayAlertAsync("Erro", "Preencha os valores corretamente.", "OK");
            SaveBtn.IsEnabled = true;
            return;
        }

        var currentExercise = _allExercises[_currentIndex];
        var currentExerciseId = currentExercise.ExerciseId != Guid.Empty
            ? currentExercise.ExerciseId
            : (currentExercise.Exercise?.Id ?? Guid.Empty);

        bool success = false;

        if (_editingSetId.HasValue)
        {
            success = await ApiService.UpdateSetAsync(_editingSetId.Value, setNum, weight, reps);
            if (success)
            {
                await DisplayAlertAsync("Sucesso", "Série atualizada!", "OK");
                _editingSetId = null;
                SaveBtn.Text = "REGISTRAR SÉRIE";
            }
            else { await DisplayAlertAsync("Erro API", $"Não foi possível atualizar.\n{ApiService.LastError}", "OK"); }
        }
        else
        {
            int restTime = currentExercise.RestSeconds > 0 ? currentExercise.RestSeconds : 90;
            StartRestTimer(restTime);

            success = await ApiService.LogSetAsync(_workoutId, currentExerciseId, setNum, weight, reps);
            if (success)
            {
                SetEntry.Text = (setNum + 1).ToString();
            }
            else { await DisplayAlertAsync("Erro API", $"Não foi possível registrar.\n{ApiService.LastError}", "OK"); }
        }

        if (success) await LoadHistory();
        SaveBtn.IsEnabled = true;
    }

    private void OnExitClicked(object sender, EventArgs e)
    {
        // FIX: Não para o timer ao sair. Ele continua em background e vibrará quando zerar.
        Application.Current!.MainPage = new WorkoutDetailPage(_workoutId);
    }

    private void OnNextOrFinishClicked(object sender, EventArgs e)
    {
        if (_isSingleExercise || _currentIndex == _allExercises.Count - 1)
        {
            if (_isSingleExercise)
            {
                Application.Current!.MainPage = new WorkoutDetailPage(_workoutId);
            }
            else
            {
                TimeSpan duration = DateTime.Now - _workoutStartTime;
                Application.Current!.MainPage = new WorkoutSummaryPage(duration, _allExercises, _workoutId);
            }
        }
        else
        {
            ChangeExercise(_currentIndex + 1);
        }
    }

    private void OnEditSetClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is ExerciseSetDto set)
        {
            _editingSetId = set.Id;
            SetEntry.Text = set.SetNumber.ToString();
            WeightEntry.Text = set.Weight.ToString();
            RepsEntry.Text = set.Repetitions.ToString();
            SaveBtn.Text = "ATUALIZAR SÉRIE";
        }
    }

    private async void OnDeleteSetClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is ExerciseSetDto set)
        {
            bool confirm = await DisplayAlertAsync("Excluir", "Excluir esta série?", "Sim", "Não");
            if (confirm)
            {
                bool success = await ApiService.DeleteSetAsync(set.Id);
                if (success) await LoadHistory();
            }
        }
    }

    private void StartRestTimer(int seconds)
    {
        _isTimerRunning = true;
        _restEndTime = DateTime.Now.AddSeconds(seconds);
        RestTimerBorder.IsVisible = true;

        Device.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            if (!_isTimerRunning) return false;

            var remaining = _restEndTime - DateTime.Now;

            if (remaining.TotalSeconds <= 0)
            {
                TimerFinished();
                return false;
            }

            _remainingSeconds = (int)Math.Ceiling(remaining.TotalSeconds);
            UpdateTimerLabel();
            return true;
        });
    }

    private void UpdateTimerLabel()
    {
        var time = TimeSpan.FromSeconds(_remainingSeconds);
        TimerLabel.Text = time.ToString(@"mm\:ss");
    }

    private void TimerFinished()
    {
        _isTimerRunning = false;
        RestTimerBorder.IsVisible = false;
        TimerLabel.Text = "00:00";

        try
        {
#if ANDROID
            var uri = Android.Media.RingtoneManager.GetDefaultUri(Android.Media.RingtoneType.Notification);
            var ringtone = Android.Media.RingtoneManager.GetRingtone(Android.App.Application.Context, uri);
            ringtone?.Play();
#endif

            Vibration.Default.Vibrate(TimeSpan.FromSeconds(1));
        }
        catch { }
    }

    private void OnMinus15Clicked(object sender, EventArgs e)
    {
        _restEndTime = _restEndTime.AddSeconds(-15);
        _remainingSeconds = Math.Max(0, (int)Math.Ceiling((_restEndTime - DateTime.Now).TotalSeconds));
        UpdateTimerLabel();
    }

    private void OnPlus15Clicked(object sender, EventArgs e)
    {
        _restEndTime = _restEndTime.AddSeconds(15);
        _remainingSeconds = (int)Math.Ceiling((_restEndTime - DateTime.Now).TotalSeconds);
        UpdateTimerLabel();
    }

    private void OnSkipRestClicked(object sender, EventArgs e)
    {
        _remainingSeconds = 0;
        TimerFinished();
    }

    // O Título agora abre o GIF
    private void OnTitleTapped(object sender, TappedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_currentGifUrl))
        {
            PopupGif.Source = ImageSource.FromUri(new Uri(ApiService.GetAbsoluteUrl(_currentGifUrl)));
            GifPopupOverlay.IsVisible = true;
        }
        else
        {
            DisplayAlertAsync("Sem Imagem", "Nenhuma imagem cadastrada para este exercício.", "OK");
        }
    }

    private void OnClosePopupClicked(object sender, EventArgs e)
    {
        GifPopupOverlay.IsVisible = false;
    }
}