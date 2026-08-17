using AlePulse.Mobile.Models;
using AlePulse.Mobile.Services;

namespace AlePulse.Mobile.Views;

public partial class EditExercisePage : ContentPage
{
    private readonly Guid _workoutId;
    private readonly WorkoutExerciseDto _exercise;

    public EditExercisePage(Guid workoutId, WorkoutExerciseDto exercise)
    {
        InitializeComponent();
        _workoutId = workoutId;
        _exercise = exercise;

        TitleLabel.Text = exercise.Exercise?.Name;
        SetsEntry.Text = exercise.Sets.ToString();
        RepsEntry.Text = exercise.Repetitions.ToString();
        WeightEntry.Text = exercise.Weight.ToString();
        RestEntry.Text = exercise.RestSeconds.ToString();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (!int.TryParse(SetsEntry.Text, out int sets) ||
            !int.TryParse(RepsEntry.Text, out int reps) ||
            !decimal.TryParse(WeightEntry.Text, out decimal weight) ||
            !int.TryParse(RestEntry.Text, out int rest))
        {
            await DisplayAlertAsync("Aviso", "Preencha os números corretamente.", "OK");
            return;
        }

        var success = await ApiService.UpdateWorkoutExerciseAsync(_workoutId, _exercise.Id, sets, reps, weight, rest);

        if (success)
        {
            await DisplayAlertAsync("Sucesso", "Exercício atualizado!", "OK");
            Application.Current!.MainPage = new WorkoutDetailPage(_workoutId);
        }
        else
        {
            // Mostra o erro exato que veio da API
            await DisplayAlertAsync("Erro API", $"Não foi possível atualizar.\n{ApiService.LastError}", "OK");
        }
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Application.Current!.MainPage = new WorkoutDetailPage(_workoutId);
    }
}