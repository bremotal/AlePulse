using AlePulse.Mobile.Models;
using AlePulse.Mobile.Services;

namespace AlePulse.Mobile.Views;

public partial class WorkoutDetailPage : ContentPage
{
    private readonly Guid _workoutId;

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

            // Busca quais exercícios já foram feitos hoje
            var completedIds = await ApiService.GetCompletedExercisesTodayAsync(_workoutId);

            // Marca cada exercício com um check se já foi feito
            if (workout.Exercises != null)
            {
                foreach (var ex in workout.Exercises)
                {
                    ex.IsCompletedToday = completedIds.Contains(ex.ExerciseId);
                }
            }

            ExercisesList.ItemsSource = workout.Exercises;
        }
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        Application.Current!.MainPage = new ProgramsPage();
    }

    private void OnAddExerciseClicked(object sender, EventArgs e)
    {
        Application.Current!.MainPage = new AddExercisePage(_workoutId);
    }

    private async void OnStartWorkoutClicked(object sender, EventArgs e)
    {
        var workout = await ApiService.GetWorkoutByIdAsync(_workoutId);
        if (workout?.Exercises != null && workout.Exercises.Count > 0)
        {
            // Inicia o treino completo (isSingleExercise = false)
            Application.Current!.MainPage = new ExerciseExecutionPage(_workoutId, workout.Exercises.ToList(), false);
        }
        else
        {
            await DisplayAlertAsync("Aviso", "Adicione exercícios ao treino antes de iniciar.", "OK");
        }
    }

    private void OnExerciseTapped(object sender, TappedEventArgs e)
    {
        if (sender is Border border && border.BindingContext is WorkoutExerciseDto exercise)
        {
            // Inicia apenas UM exercício (isSingleExercise = true)
            Application.Current!.MainPage = new ExerciseExecutionPage(_workoutId, new List<WorkoutExerciseDto> { exercise }, true);
        }
    }

    private void OnEditExerciseClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is WorkoutExerciseDto exercise)
        {
            Application.Current!.MainPage = new EditExercisePage(_workoutId, exercise);
        }
    }

    private async void OnDeleteExerciseClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is WorkoutExerciseDto exercise)
        {
            bool confirm = await DisplayAlertAsync("Excluir", $"Remover {exercise.Exercise?.Name} do treino?", "Sim", "Não");
            if (confirm)
            {
                await ApiService.DeleteWorkoutExerciseAsync(_workoutId, exercise.Id);
                await LoadWorkoutDetails();
            }
        }
    }

    private async void OnUploadImageClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is WorkoutExerciseDto exercise)
        {
            try
            {
                var customFileType = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.Android, new[] { "image/jpeg", "image/png", "image/gif" } },
                        { DevicePlatform.WinUI, new[] { ".jpg", ".png", ".gif" } }
                    });

                var file = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Selecione a imagem ou GIF do exercício",
                    FileTypes = customFileType
                });

                if (file == null) return;

                bool success = await ApiService.UploadExerciseImageAsync(exercise.Exercise!.Id, file);
                if (success)
                {
                    await DisplayAlertAsync("Sucesso", "Imagem enviada!", "OK");
                    await LoadWorkoutDetails();
                }
                else
                {
                    await DisplayAlertAsync("Erro", $"Falha no upload.\n{ApiService.LastError}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Erro", ex.Message, "OK");
            }
        }
    }
}