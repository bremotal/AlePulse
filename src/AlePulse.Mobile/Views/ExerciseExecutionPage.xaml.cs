using AlePulse.Mobile.Models;
using AlePulse.Mobile.Services;
using System.Globalization;
using System.Runtime.InteropServices;

namespace AlePulse.Mobile.Views;

public partial class ExerciseExecutionPage : ContentPage
{
    private readonly Guid _workoutId;
    private List<WorkoutExerciseDto> _allExercises = new();
    private int _currentIndex = 0;
    private Guid? _editingSetId = null;

    // Variáveis do Cronômetro
    private int _remainingSeconds;
    private bool _isTimerRunning;
    private DateTime _workoutStartTime; // Variável para o tempo total do treino

    // Importação do Beep do Windows (para teste no Windows Machine)
    [DllImport("kernel32.dll")]
    public static extern bool Beep(int frequency, int duration);

    public ExerciseExecutionPage(Guid workoutId, List<WorkoutExerciseDto> exercises)
    {
        InitializeComponent();
        _workoutId = workoutId;
        _allExercises = exercises;

        ExercisePicker.ItemsSource = _allExercises;
        ChangeExercise(0);

        _workoutStartTime = DateTime.Now; // Inicia a contagem do tempo total
    }

    private void ChangeExercise(int newIndex)
    {
        if (_allExercises.Count == 0) return;
        _currentIndex = Math.Clamp(newIndex, 0, _allExercises.Count - 1);

        var exercise = _allExercises[_currentIndex];
        ExercisePicker.SelectedIndex = _currentIndex;

        TitleLabel.Text = exercise.Exercise!.Name;
        WeightEntry.Text = exercise.Weight.ToString();
        RepsEntry.Text = exercise.Repetitions.ToString();

        _editingSetId = null;
        SaveBtn.Text = "REGISTRAR SÉRIE";

        LoadHistory();

        // Verifica se é o último exercício para mudar o botão
        if (_currentIndex == _allExercises.Count - 1)
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
        await LoadHistory();
    }

    private async Task LoadHistory()
    {
        try
        {
            if (_allExercises.Count == 0) return;
            var currentExerciseId = _allExercises[_currentIndex].Exercise!.Id;

            var history = await ApiService.GetHistoryAsync(currentExerciseId);

            var grouped = history
                .GroupBy(e => e.CompletedAt.Date)
                .Select(g => new GroupedExerciseSet(
                    g.Key.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                    g.OrderByDescending(x => x.CompletedAt).ToList()
                ))
                .OrderByDescending(g => g.DateDisplay)
                .ToList();

            HistoryList.ItemsSource = grouped;

            if (!_editingSetId.HasValue)
            {
                var todaySets = history.Where(x => x.CompletedAt.Date == DateTime.Now.Date).ToList();
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
        var currentExerciseId = currentExercise.Exercise!.Id;
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
            success = await ApiService.LogSetAsync(_workoutId, currentExerciseId, setNum, weight, reps);
            if (success)
            {
                SetEntry.Text = (setNum + 1).ToString();

                // INICIA O CRONÔMETRO AUTOMATICAMENTE APÓS REGISTRAR
                int restTime = currentExercise.RestSeconds > 0 ? currentExercise.RestSeconds : 90;
                StartRestTimer(restTime);
            }
            else { await DisplayAlertAsync("Erro API", $"Não foi possível registrar.\n{ApiService.LastError}", "OK"); }
        }

        if (success) await LoadHistory();
        SaveBtn.IsEnabled = true;
    }

    private async void OnLoadHistoryClicked(object sender, EventArgs e)
    {
        var currentExerciseId = _allExercises[_currentIndex].Exercise!.Id;
        var history = await ApiService.GetHistoryAsync(currentExerciseId);
        if (history.Count > 0)
        {
            var lastSet = history.OrderByDescending(x => x.CompletedAt).First();
            WeightEntry.Text = lastSet.Weight.ToString();
            RepsEntry.Text = lastSet.Repetitions.ToString();
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

    private void OnExitClicked(object sender, EventArgs e)
    {
        // Pausa o cronômetro de descanso se estiver rodando
        _isTimerRunning = false;
        RestTimerBorder.IsVisible = false;

        // Volta para a tela de detalhes sem mostrar o resumo
        Application.Current!.MainPage = new WorkoutDetailPage(_workoutId);
    }

    private void OnNextOrFinishClicked(object sender, EventArgs e)
    {
        // Se for o último exercício, finaliza o treino e mostra o resumo
        if (_currentIndex == _allExercises.Count - 1)
        {
            TimeSpan duration = DateTime.Now - _workoutStartTime;
            Application.Current!.MainPage = new WorkoutSummaryPage(duration, _allExercises, _workoutId);
        }
        // Se não for o último, vai para o próximo exercício
        else
        {
            ChangeExercise(_currentIndex + 1);
        }
    }

    // --- LÓGICA DO CRONÔMETRO ---

    private void StartRestTimer(int seconds)
    {
        _isTimerRunning = true;
        _remainingSeconds = seconds;
        RestTimerBorder.IsVisible = true;
        UpdateTimerLabel();

        Device.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            if (!_isTimerRunning) return false;

            _remainingSeconds--;

            if (_remainingSeconds <= 0)
            {
                TimerFinished();
                return false; // Para o timer
            }

            UpdateTimerLabel();
            return true; // Continua o timer
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
            // Toca um beep no Windows (Frequência 800Hz por 500ms)
            if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                Beep(800, 500);
            }

            // Vibra o celular por 1 segundo (funciona no Android/iOS)
            Vibration.Default.Vibrate(TimeSpan.FromSeconds(1));
        }
        catch { }
    }

    private void OnMinus15Clicked(object sender, EventArgs e)
    {
        _remainingSeconds = Math.Max(0, _remainingSeconds - 15);
        UpdateTimerLabel();
    }

    private void OnPlus15Clicked(object sender, EventArgs e)
    {
        _remainingSeconds += 15;
        UpdateTimerLabel();
    }

    private void OnSkipRestClicked(object sender, EventArgs e)
    {
        _remainingSeconds = 0;
        TimerFinished();
    }
}