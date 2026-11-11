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

        TimeLabel.Text = _duration.TotalHours >= 1
            ? _duration.ToString(@"hh\:mm\:ss")
            : _duration.ToString(@"mm\:ss");

        decimal totalVolume = 0;
        foreach (var ex in _exercises)
        {
            var exerciseId = ex.ExerciseId != Guid.Empty
                ? ex.ExerciseId
                : (ex.Exercise?.Id ?? Guid.Empty);

            if (exerciseId == Guid.Empty) continue;

            var history = await ApiService.GetHistoryAsync(exerciseId);
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

        Application.Current!.MainPage = new ProgramsPage();
    }
}