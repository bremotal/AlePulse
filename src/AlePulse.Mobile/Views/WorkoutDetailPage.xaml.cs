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
        try
        {
            var workout = await ApiService.GetWorkoutByIdAsync(_workoutId);
            if (workout != null)
            {
                WorkoutNameLabel.Text = workout.Name ?? "Treino";
                WorkoutDescLabel.Text = workout.Description ?? string.Empty;

                var completedIds = await ApiService.GetCompletedExercisesTodayAsync(_workoutId);

                if (workout.Exercises != null)
                {
                    foreach (var ex in workout.Exercises)
                    {
                        // FALLBACK: Usa ExerciseId ou Exercise.Id (caso um deles venha vazio da API)
                        var exerciseGuid = ex.ExerciseId != Guid.Empty
                            ? ex.ExerciseId
                            : (ex.Exercise?.Id ?? Guid.Empty);

                        ex.IsCompletedToday = completedIds.Contains(exerciseGuid);
                    }
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ExercisesList.ItemsSource = workout.Exercises;
                });
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", $"Não foi possível carregar o treino:\n{ex.Message}", "OK");
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
        try
        {
            var workout = await ApiService.GetWorkoutByIdAsync(_workoutId);
            if (workout?.Exercises != null && workout.Exercises.Count > 0)
            {
                Application.Current!.MainPage = new ExerciseExecutionPage(_workoutId, workout.Exercises.ToList(), false);
            }
            else
            {
                await DisplayAlertAsync("Aviso", "Adicione exercícios ao treino antes de iniciar.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", $"Não foi possível iniciar:\n{ex.Message}", "OK");
        }
    }

    private void OnExerciseTapped(object sender, TappedEventArgs e)
    {
        if (sender is Border border && border.BindingContext is WorkoutExerciseDto exercise)
        {
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