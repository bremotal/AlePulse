using AlePulse.Mobile.Models;
using AlePulse.Mobile.Services;

namespace AlePulse.Mobile.Views;

public partial class WorkoutDetailPage : ContentPage
{
    private readonly Guid _workoutId;

    private WorkoutExerciseDto? _draggedItem;

    private Border? _draggedBorder;

    private Brush? _originalStroke;

    private double _originalStrokeThickness = 1;

    private List<WorkoutExerciseDto> _currentExercises = new();

    public WorkoutDetailPage(Guid workoutId)
    {
        InitializeComponent();

        _workoutId = workoutId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadWorkoutDetails();
        UpdateStartButton();
    }

    private void UpdateStartButton()
    {
        bool hasActiveSession =
            ActiveWorkoutSession.IsActive &&
            ActiveWorkoutSession.WorkoutId == _workoutId;

        StartWorkoutBtn.Text = hasActiveSession ? "CONTINUAR" : "INICIAR";
    }

    private async Task LoadWorkoutDetails()
    {
        try
        {
            var workout =
                await ApiService.GetWorkoutByIdAsync(_workoutId);

            if (workout == null)
            {
                await DisplayAlertAsync(
                    "Erro",
                    "Não foi possível carregar o treino.",
                    "OK");

                return;
            }

            WorkoutNameLabel.Text =
                workout.Name ?? "Treino";

            WorkoutDescLabel.Text =
                workout.Description ?? string.Empty;

            var completedIds =
                await ApiService.GetCompletedExercisesTodayAsync(
                    _workoutId);

            if (workout.Exercises != null)
            {

                var orderedExercises =
                    workout.Exercises
                           .OrderBy(e => e.Order)
                           .ToList();

                _currentExercises =
                    orderedExercises;

                foreach (var ex in orderedExercises)
                {
                    var exerciseGuid =
                        ex.ExerciseId != Guid.Empty
                            ? ex.ExerciseId
                            : (ex.Exercise?.Id ?? Guid.Empty);

                    ex.IsCompletedToday =
                        completedIds.Contains(exerciseGuid);
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ExercisesList.ItemsSource = null;

                    ExercisesList.ItemsSource =
                        _currentExercises;
                });
            }
            else
            {
                _currentExercises = new List<WorkoutExerciseDto>();

                ExercisesList.ItemsSource = null;

                ExercisesList.ItemsSource =
                    _currentExercises;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Erro",
                $"Não foi possível carregar o treino:\n{ex.Message}",
                "OK");
        }
    }

    private void OnBackClicked(
        object sender,
        EventArgs e)
    {
        Application.Current!.MainPage =
            new ProgramsPage();
    }

    private void OnAddExerciseClicked(
        object sender,
        EventArgs e)
    {
        Application.Current!.MainPage =
            new AddExercisePage(_workoutId);
    }

    private async void OnStartWorkoutClicked(
        object sender,
        EventArgs e)
    {
        try
        {
            var workout =
                await ApiService.GetWorkoutByIdAsync(
                    _workoutId);

            if (workout?.Exercises != null &&
                workout.Exercises.Count > 0)
            {
                var orderedExercises =
                    workout.Exercises
                           .OrderBy(e => e.Order)
                           .ToList();

                if (ActiveWorkoutSession.IsActive &&
                    ActiveWorkoutSession.WorkoutId == _workoutId)
                {
                    ActiveWorkoutSession.UpdateExercises(
                        orderedExercises,
                        ActiveWorkoutSession.CurrentIndex);
                }
                else
                {
                    ActiveWorkoutSession.Start(
                        _workoutId,
                        orderedExercises);
                }

                Application.Current!.MainPage =
                    new ExerciseExecutionPage(
                        _workoutId,
                        ActiveWorkoutSession.Exercises,
                        false,
                        ActiveWorkoutSession.CurrentIndex);
            }
            else
            {
                await DisplayAlertAsync(
                    "Aviso",
                    "Adicione exercícios ao treino antes de iniciar.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Erro",
                $"Não foi possível iniciar:\n{ex.Message}",
                "OK");
        }
    }

    private void OnExerciseTapped(
        object sender,
        TappedEventArgs e)
    {
        if (sender is Border border &&
            border.BindingContext is WorkoutExerciseDto exercise)
        {
            var index = _currentExercises.IndexOf(exercise);

            Application.Current!.MainPage =
                new ExerciseExecutionPage(
                    _workoutId,
                    _currentExercises,
                    true,
                    index < 0 ? 0 : index);
        }
    }

    private void OnEditExerciseClicked(
        object sender,
        EventArgs e)
    {
        if (sender is Button button &&
            button.BindingContext is WorkoutExerciseDto exercise)
        {
            Application.Current!.MainPage =
                new EditExercisePage(
                    _workoutId,
                    exercise);
        }
    }

    private async void OnDeleteExerciseClicked(
        object sender,
        EventArgs e)
    {
        if (sender is Button button &&
            button.BindingContext is WorkoutExerciseDto exercise)
        {
            bool confirm =
                await DisplayAlertAsync(
                    "Excluir",
                    $"Remover {exercise.Exercise?.Name} do treino?",
                    "Sim",
                    "Não");

            if (confirm)
            {
                bool success =
                    await ApiService.DeleteWorkoutExerciseAsync(
                        _workoutId,
                        exercise.Id);

                if (success)
                {
                    await LoadWorkoutDetails();
                }
                else
                {
                    await DisplayAlertAsync(
                        "Erro",
                        $"Não foi possível excluir o exercício.\n\n{ApiService.LastError}",
                        "OK");
                }
            }
        }
    }

    private async void OnUploadImageClicked(
        object sender,
        EventArgs e)
    {
        if (sender is Button button &&
            button.BindingContext is WorkoutExerciseDto exercise)
        {
            try
            {
                var customFileType =
                    new FilePickerFileType(
                        new Dictionary<
                            DevicePlatform,
                            IEnumerable<string>>
                        {
                            {
                                DevicePlatform.Android,
                                new[]
                                {
                                    "image/jpeg",
                                    "image/png",
                                    "image/gif"
                                }
                            },

                            {
                                DevicePlatform.WinUI,
                                new[]
                                {
                                    ".jpg",
                                    ".png",
                                    ".gif"
                                }
                            }
                        });

                var file =
                    await FilePicker.Default.PickAsync(
                        new PickOptions
                        {
                            PickerTitle =
                                "Selecione a imagem ou GIF do exercício",

                            FileTypes =
                                customFileType
                        });

                if (file == null)
                    return;

                bool success =
                    await ApiService.UploadExerciseImageAsync(
                        exercise.Exercise!.Id,
                        file);

                if (success)
                {
                    await DisplayAlertAsync(
                        "Sucesso",
                        "Imagem enviada!",
                        "OK");

                    await LoadWorkoutDetails();
                }
                else
                {
                    await DisplayAlertAsync(
                        "Erro",
                        $"Falha no upload.\n{ApiService.LastError}",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync(
                    "Erro",
                    ex.Message,
                    "OK");
            }
        }
    }

    private async void OnDragStarting(
        object sender,
        DragStartingEventArgs e)
    {
        if (sender is not Border border ||
            border.BindingContext is not WorkoutExerciseDto exercise)
        {
            return;
        }

        _draggedItem =
            exercise;

        _draggedBorder =
            border;

        _originalStroke =
            border.Stroke;

        _originalStrokeThickness =
            border.StrokeThickness;

        border.Stroke =
            new SolidColorBrush(
                Color.FromArgb("#FF6A00"));

        border.StrokeThickness =
            3;

        await Task.WhenAll(
            border.ScaleTo(
                1.03,
                150,
                Easing.CubicOut),

            border.FadeTo(
                0.75,
                150,
                Easing.CubicOut)
        );

        e.Handled = true;
    }

    private async Task RestoreDraggedBorderAsync()
    {
        if (_draggedBorder == null)
            return;

        var border =
            _draggedBorder;

        try
        {

            await Task.WhenAll(
                border.ScaleTo(
                    1.0,
                    150,
                    Easing.CubicInOut),

                border.FadeTo(
                    1.0,
                    150,
                    Easing.CubicInOut)
            );

            border.Stroke =
                _originalStroke;

            border.StrokeThickness =
                _originalStrokeThickness;
        }
        catch
        {

            border.Scale = 1.0;

            border.Opacity = 1.0;

            border.Stroke =
                _originalStroke;

            border.StrokeThickness =
                _originalStrokeThickness;
        }

        _draggedBorder = null;

        _originalStroke = null;

        _originalStrokeThickness = 1;
    }

    private async void OnDrop(
        object sender,
        DropEventArgs e)
    {

        if (_draggedItem == null ||
            sender is not Element element ||
            element.BindingContext is not WorkoutExerciseDto targetItem)
        {
            e.Handled = true;

            await RestoreDraggedBorderAsync();

            _draggedItem = null;

            return;
        }

        e.Handled = true;

        if (_draggedItem.Id == targetItem.Id)
        {
            await RestoreDraggedBorderAsync();

            _draggedItem = null;

            return;
        }

        try
        {
            var items =
                _currentExercises;

            var draggedItem =
                _draggedItem;

            items.Remove(draggedItem);

            var targetIndex =
                items.IndexOf(targetItem);

            if (targetIndex < 0)
            {
                items.Add(draggedItem);
            }
            else
            {
                items.Insert(
                    targetIndex,
                    draggedItem);
            }

            for (int i = 0; i < items.Count; i++)
            {
                items[i].Order =
                    i + 1;
            }

            ExercisesList.ItemsSource =
                null;

            ExercisesList.ItemsSource =
                items;

            bool success =
                await ApiService.ReorderExercisesAsync(
                    _workoutId,
                    items.Select(ex => ex.Id).ToList());

            if (!success)
            {
                await DisplayAlertAsync(
                    "Erro",
                    $"Não foi possível salvar a nova ordem.\n\n{ApiService.LastError}",
                    "OK");

                await LoadWorkoutDetails();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Erro",
                $"Não foi possível salvar a nova ordem:\n{ex.Message}",
                "OK");

            await LoadWorkoutDetails();
        }
        finally
        {

            await RestoreDraggedBorderAsync();

            _draggedItem = null;
        }
    }
}