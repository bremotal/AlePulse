using AlePulse.Mobile.Models;
using AlePulse.Mobile.Services;

namespace AlePulse.Mobile.Views;

public partial class ProgramDetailPage : ContentPage
{
    private readonly Guid _programId;

    public ProgramDetailPage(Guid programId, string programName)
    {
        InitializeComponent();
        _programId = programId;
        ProgramNameLabel.Text = programName;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadWorkouts();
    }

    private async Task LoadWorkouts()
    {
        var program = await ApiService.GetProgramByIdAsync(_programId);
        if (program != null)
        {
            WorkoutsList.ItemsSource = program.Workouts;
        }
    }

    private async void OnAddWorkoutClicked(object sender, EventArgs e)
    {
        string name = await DisplayPromptAsync("Novo Treino", "Nome do treino (ex: Treino A - Peito):", "OK", "Cancelar");
        if (string.IsNullOrWhiteSpace(name)) return;
        string desc = await DisplayPromptAsync("Descrição", "Descrição (opcional):", "OK", "Cancelar");

        bool success = await ApiService.AddWorkoutToProgramAsync(_programId, name, desc);
        if (success) await LoadWorkouts();
    }

    private async void OnEditWorkoutClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is WorkoutDto workout)
        {
            string newName = await DisplayPromptAsync("Editar Treino", "Nome:", "OK", "Cancelar", initialValue: workout.Name);
            if (string.IsNullOrWhiteSpace(newName)) return;
            string newDesc = await DisplayPromptAsync("Descrição", "Descrição:", "OK", "Cancelar", initialValue: workout.Description ?? "");

            bool success = await ApiService.UpdateWorkoutAsync(workout.Id, newName, newDesc);
            if (success) await LoadWorkouts();
        }
    }

    private async void OnDeleteWorkoutClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is WorkoutDto workout)
        {
            bool confirm = await DisplayAlertAsync("Excluir", $"Excluir o {workout.Name}?", "Sim", "Não");
            if (confirm)
            {
                bool success = await ApiService.DeleteWorkoutAsync(workout.Id);
                if (success) await LoadWorkouts();
            }
        }
    }

    private void OnWorkoutTapped(object sender, TappedEventArgs e)
    {
        if (sender is Border border && border.BindingContext is WorkoutDto workout)
        {
            Application.Current!.MainPage = new WorkoutDetailPage(workout.Id);
        }
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        Application.Current!.MainPage = new ProgramsPage();
    }
}