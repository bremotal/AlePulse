using AlePulse.Mobile.Models;
using AlePulse.Mobile.Services;

namespace AlePulse.Mobile.Views;

public partial class AddExercisePage : ContentPage
{
    private readonly Guid _workoutId;

    public AddExercisePage(Guid workoutId)
    {
        InitializeComponent();
        _workoutId = workoutId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var exercises = await ApiService.GetExercisesAsync();
        ExercisePicker.ItemsSource = exercises;
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        Guid exerciseId = Guid.Empty;

        // Se o usuário digitou um novo exercício, cria ele primeiro
        if (!string.IsNullOrWhiteSpace(NewExerciseEntry.Text))
        {
            var newEx = await ApiService.CreateExerciseAsync(NewExerciseEntry.Text);
            if (newEx != null) exerciseId = newEx.Id;
        }
        // Senão, pega o selecionado no Picker
        else if (ExercisePicker.SelectedItem is ExerciseDto selectedExercise)
        {
            exerciseId = selectedExercise.Id;
        }

        if (exerciseId == Guid.Empty)
        {
            await DisplayAlertAsync("Aviso", "Selecione ou crie um exercício.", "OK");
            return;
        }

        if (!int.TryParse(SetsEntry.Text, out int sets) ||
            !int.TryParse(RepsEntry.Text, out int reps) ||
            !int.TryParse(RestEntry.Text, out int rest) ||
            !decimal.TryParse(WeightEntry.Text, out decimal weight))
        {
            await DisplayAlertAsync("Aviso", "Preencha os números corretamente.", "OK");
            return;
        }

        var success = await ApiService.AddExerciseToWorkoutAsync(_workoutId, exerciseId, sets, reps, weight, rest);

        if (success)
        {
            await DisplayAlertAsync("Sucesso", "Exercício adicionado!", "OK");
            Application.Current!.MainPage = new WorkoutDetailPage(_workoutId);
        }
        else
        {
            await DisplayAlertAsync("Erro", "Não foi possível adicionar.", "OK");
        }
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Application.Current!.MainPage = new WorkoutDetailPage(_workoutId);
    }
}