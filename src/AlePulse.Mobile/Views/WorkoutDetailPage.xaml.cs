using AlePulse.Mobile.Models;
using AlePulse.Mobile.Services;

namespace AlePulse.Mobile.Views;

public partial class WorkoutDetailPage : ContentPage
{
    private readonly Guid _workoutId;

    // Recebe o ID do treino quando a tela é criada
    public WorkoutDetailPage(Guid workoutId)
    {
        InitializeComponent();
        _workoutId = workoutId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadWorkoutDetails();
    }

    private async Task LoadWorkoutDetails()
    {
        var workout = await ApiService.GetWorkoutByIdAsync(_workoutId);
        if (workout != null)
        {
            WorkoutNameLabel.Text = workout.Name;
            WorkoutDescLabel.Text = workout.Description;
            ExercisesList.ItemsSource = workout.Exercises;
        }
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        Application.Current!.MainPage = new HomePage();
    }
    private void OnAddExerciseClicked(object sender, EventArgs e)
    {
        // Vai para a tela de adicionar exercício, passando o ID do treino
        Application.Current!.MainPage = new AddExercisePage(_workoutId);
    }

    private async void OnStartWorkoutClicked(object sender, EventArgs e)
    {
        // Aviso temporário até criarmos a tela de execução de fato
        await DisplayAlertAsync("Em breve", "A execução do treino em tempo real será implementada na próxima sprint!", "OK");
    }
    private async void OnDeleteExerciseClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is WorkoutExerciseDto exercise)
        {
            bool confirm = await DisplayAlertAsync("Excluir", $"Remover {exercise.Exercise?.Name} do treino?", "Sim", "Não");
            if (confirm)
            {
                await ApiService.DeleteWorkoutExerciseAsync(_workoutId, exercise.Id);
                await LoadWorkoutDetails(); // Recarrega a lista de exercícios
            }
        }
    }
    private void OnExerciseTapped(object sender, TappedEventArgs e)
    {
        if (sender is Border border && border.BindingContext is WorkoutExerciseDto exercise)
        {
            // Abre a tela de execução passando o ID do treino e os dados do exercício
            Application.Current!.MainPage = new ExerciseExecutionPage(_workoutId, exercise);
        }
    }
}