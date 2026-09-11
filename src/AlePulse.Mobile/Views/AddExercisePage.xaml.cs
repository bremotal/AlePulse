using AlePulse.Mobile.Models;
using AlePulse.Mobile.Services;

namespace AlePulse.Mobile.Views;

public partial class AddExercisePage : ContentPage
{
    private readonly Guid _workoutId;
    private List<ExerciseDto> _allExercises = new();
    private ExerciseDto? _selectedExercise;

    public AddExercisePage(Guid workoutId)
    {
        InitializeComponent();
        _workoutId = workoutId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _allExercises = await ApiService.GetExercisesAsync();
        ExerciseList.ItemsSource = _allExercises;
    }

    // Lógica de pesquisa
    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var keyword = e.NewTextValue.ToLower();
        ExerciseList.ItemsSource = _allExercises.Where(ex => ex.Name.ToLower().Contains(keyword)).ToList();
    }

    // Lógica de seleção na lista
    private void OnExerciseSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedExercise = e.CurrentSelection.FirstOrDefault() as ExerciseDto;
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        Guid exerciseId = Guid.Empty;

        if (!string.IsNullOrWhiteSpace(NewExerciseEntry.Text))
        {
            var newEx = await ApiService.CreateExerciseAsync(NewExerciseEntry.Text);
            if (newEx != null) exerciseId = newEx.Id;
        }
        else if (_selectedExercise != null)
        {
            exerciseId = _selectedExercise.Id;
        }

        if (exerciseId == Guid.Empty)
        {
            await DisplayAlertAsync("Aviso", "Selecione um exercício da lista ou crie um novo.", "OK");
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

        bool success = await ApiService.AddExerciseToWorkoutAsync(_workoutId, exerciseId, sets, reps, weight, rest);

        if (success)
        {
            await DisplayAlertAsync("Sucesso", "Exercício adicionado!", "OK");
            Application.Current!.MainPage = new WorkoutDetailPage(_workoutId);
        }
        else
        {
            await DisplayAlertAsync("Erro", $"Não foi possível adicionar.\n{ApiService.LastError}", "OK");
        }
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Application.Current!.MainPage = new WorkoutDetailPage(_workoutId);
    }

    // Excluir da biblioteca
    private async void OnDeleteFromLibraryClicked(object sender, EventArgs e)
    {
        if (_selectedExercise == null)
        {
            await DisplayAlertAsync("Aviso", "Selecione um exercício da lista para excluí-lo.", "OK");
            return;
        }

        bool confirm = await DisplayAlertAsync("Excluir", $"Excluir '{_selectedExercise.Name}' permanentemente da biblioteca?", "Sim", "Não");
        if (!confirm) return;

        bool success = await ApiService.DeleteExerciseAsync(_selectedExercise.Id);
        if (success)
        {
            await DisplayAlertAsync("Sucesso", "Exercício excluído da biblioteca!", "OK");
            _allExercises = await ApiService.GetExercisesAsync();
            ExerciseList.ItemsSource = _allExercises;
            _selectedExercise = null;
        }
        else
        {
            await DisplayAlertAsync("Erro", $"Não foi possível excluir.\n{ApiService.LastError}", "OK");
        }
    }
}