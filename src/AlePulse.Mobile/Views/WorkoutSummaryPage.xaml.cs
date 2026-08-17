using AlePulse.Mobile.Models;
using AlePulse.Mobile.Services;

namespace AlePulse.Mobile.Views;

public partial class WorkoutSummaryPage : ContentPage
{
    private readonly Guid _workoutId;
    private readonly TimeSpan _duration;
    private readonly List<WorkoutExerciseDto> _exercises;

    public WorkoutSummaryPage(TimeSpan duration, List<WorkoutExerciseDto> exercises, Guid workoutId)
    {
        InitializeComponent();
        _duration = duration;
        _exercises = exercises;
        _workoutId = workoutId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Formata o tempo (ex: 01:05)
        TimeLabel.Text = _duration.ToString(@"mm\:ss");

        // Calcula o Volume Total (Soma de Peso x Repetições de todas as séries de hoje)
        decimal totalVolume = 0;
        foreach (var ex in _exercises)
        {
            var history = await ApiService.GetHistoryAsync(ex.Exercise!.Id);
            var todaySets = history.Where(x => x.CompletedAt.Date == DateTime.Now.Date);

            foreach (var set in todaySets)
            {
                totalVolume += set.Weight * set.Repetitions;
            }
        }

        VolumeLabel.Text = $"{totalVolume:F1} kg";
    }

    private void OnBackHomeClicked(object sender, EventArgs e)
    {
        Application.Current!.MainPage = new HomePage();
    }
}