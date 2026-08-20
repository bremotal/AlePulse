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
            ExercisesList.ItemsSource = workout.Exercises;
        }
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        Application.Current!.MainPage = new HomePage();
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
            // Envia a lista inteira de exercícios para o Player
            Application.Current!.MainPage = new ExerciseExecutionPage(_workoutId, workout.Exercises.ToList());
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
            // Se clicar no card (mas não nos botões), abre apenas ele
            Application.Current!.MainPage = new ExerciseExecutionPage(_workoutId, new List<WorkoutExerciseDto> { exercise });
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

    // NOVO MÉTODO: Fazer upload de imagem da galeria
    private async void OnUploadImageClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is WorkoutExerciseDto exercise)
        {
            try
            {
                // Abre a galeria de fotos do celular/computador
                var file = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Selecione a imagem do exercício",
                    FileTypes = FilePickerFileType.Images
                });

                if (file == null) return; // Usuário cancelou

                // Envia para a API
                bool success = await ApiService.UploadExerciseImageAsync(exercise.Exercise!.Id, file);
                if (success)
                {
                    await DisplayAlertAsync("Sucesso", "Imagem enviada!", "OK");
                    await LoadWorkoutDetails(); // Atualiza a tela
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