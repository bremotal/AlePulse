using AlePulse.Mobile.Models;
using AlePulse.Mobile.Services;

namespace AlePulse.Mobile.Views;

public partial class WorkoutDetailPage : ContentPage
{
    private readonly Guid _workoutId;

    // ============================================================
    // DRAG AND DROP
    // ============================================================

    // Exercício que está sendo arrastado
    private WorkoutExerciseDto? _draggedItem;

    // Border correspondente ao exercício arrastado
    private Border? _draggedBorder;

    // Brush original da borda
    private Brush? _originalStroke;

    // Espessura original da borda
    private double _originalStrokeThickness = 1;

    // Lista atual de exercícios
    private List<WorkoutExerciseDto> _currentExercises = new();


    // ============================================================
    // CONSTRUTOR
    // ============================================================

    public WorkoutDetailPage(Guid workoutId)
    {
        InitializeComponent();

        _workoutId = workoutId;
    }


    // ============================================================
    // APARECER NA TELA
    // ============================================================

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadWorkoutDetails();
    }


    // ============================================================
    // CARREGAR DETALHES DO TREINO
    // ============================================================

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


            // ====================================================
            // DADOS DO TREINO
            // ====================================================

            WorkoutNameLabel.Text =
                workout.Name ?? "Treino";

            WorkoutDescLabel.Text =
                workout.Description ?? string.Empty;


            // ====================================================
            // EXERCÍCIOS CONCLUÍDOS HOJE
            // ====================================================

            var completedIds =
                await ApiService.GetCompletedExercisesTodayAsync(
                    _workoutId);


            // ====================================================
            // EXERCÍCIOS
            // ====================================================

            if (workout.Exercises != null)
            {
                // IMPORTANTE:
                // Sempre respeita a propriedade Order salva
                // no servidor.

                var orderedExercises =
                    workout.Exercises
                           .OrderBy(e => e.Order)
                           .ToList();


                _currentExercises =
                    orderedExercises;


                // =================================================
                // ATUALIZAR STATUS DE CONCLUSÃO
                // =================================================

                foreach (var ex in orderedExercises)
                {
                    var exerciseGuid =
                        ex.ExerciseId != Guid.Empty
                            ? ex.ExerciseId
                            : (ex.Exercise?.Id ?? Guid.Empty);


                    ex.IsCompletedToday =
                        completedIds.Contains(exerciseGuid);
                }


                // =================================================
                // ATUALIZAR INTERFACE
                // =================================================

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


    // ============================================================
    // VOLTAR
    // ============================================================

    private void OnBackClicked(
        object sender,
        EventArgs e)
    {
        Application.Current!.MainPage =
            new ProgramsPage();
    }


    // ============================================================
    // ADICIONAR EXERCÍCIO
    // ============================================================

    private void OnAddExerciseClicked(
        object sender,
        EventArgs e)
    {
        Application.Current!.MainPage =
            new AddExercisePage(_workoutId);
    }


    // ============================================================
    // INICIAR TREINO
    // ============================================================

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
                // IMPORTANTE:
                // Também respeita a ordem salva no servidor.

                var orderedExercises =
                    workout.Exercises
                           .OrderBy(e => e.Order)
                           .ToList();


                Application.Current!.MainPage =
                    new ExerciseExecutionPage(
                        _workoutId,
                        orderedExercises,
                        false);
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


    // ============================================================
    // CLICAR NO EXERCÍCIO
    // ============================================================

    private void OnExerciseTapped(
        object sender,
        TappedEventArgs e)
    {
        if (sender is Border border &&
            border.BindingContext is WorkoutExerciseDto exercise)
        {
            Application.Current!.MainPage =
                new ExerciseExecutionPage(
                    _workoutId,
                    new List<WorkoutExerciseDto>
                    {
                        exercise
                    },
                    true);
        }
    }


    // ============================================================
    // EDITAR EXERCÍCIO
    // ============================================================

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


    // ============================================================
    // EXCLUIR EXERCÍCIO
    // ============================================================

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


    // ============================================================
    // UPLOAD DE IMAGEM
    // ============================================================

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


    // ============================================================
    // DRAG - INICIAR
    // ============================================================

    private async void OnDragStarting(
        object sender,
        DragStartingEventArgs e)
    {
        if (sender is not Border border ||
            border.BindingContext is not WorkoutExerciseDto exercise)
        {
            return;
        }


        // ========================================================
        // GUARDAR ITEM
        // ========================================================

        _draggedItem =
            exercise;


        // ========================================================
        // GUARDAR BORDER
        // ========================================================

        _draggedBorder =
            border;


        // ========================================================
        // GUARDAR ESTADO ORIGINAL
        // ========================================================

        _originalStroke =
            border.Stroke;

        _originalStrokeThickness =
            border.StrokeThickness;


        // ========================================================
        // APLICAR DESTAQUE
        // ========================================================

        border.Stroke =
            new SolidColorBrush(
                Color.FromArgb("#FF6A00"));

        border.StrokeThickness =
            3;


        // ========================================================
        // ANIMAÇÃO
        // ========================================================

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


    // ============================================================
    // DRAG - RESTAURAR VISUAL
    // ============================================================

    private async Task RestoreDraggedBorderAsync()
    {
        if (_draggedBorder == null)
            return;


        var border =
            _draggedBorder;


        try
        {
            // ====================================================
            // ANIMAÇÃO DE RETORNO
            // ====================================================

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


            // ====================================================
            // RESTAURAR BORDA ORIGINAL
            // ====================================================

            border.Stroke =
                _originalStroke;

            border.StrokeThickness =
                _originalStrokeThickness;
        }
        catch
        {
            // Segurança para garantir que o visual volte ao normal

            border.Scale = 1.0;

            border.Opacity = 1.0;

            border.Stroke =
                _originalStroke;

            border.StrokeThickness =
                _originalStrokeThickness;
        }


        // ========================================================
        // LIMPAR REFERÊNCIAS
        // ========================================================

        _draggedBorder = null;

        _originalStroke = null;

        _originalStrokeThickness = 1;
    }


    // ============================================================
    // DROP
    // ============================================================

    private async void OnDrop(
        object sender,
        DropEventArgs e)
    {
        // ========================================================
        // VALIDAR DRAG
        // ========================================================

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


        // ========================================================
        // NÃO PERMITE SOLTAR SOBRE ELE MESMO
        // ========================================================

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


            // ====================================================
            // 1. GUARDAR ITEM ARRASTADO
            // ====================================================

            var draggedItem =
                _draggedItem;


            // ====================================================
            // 2. REMOVER DA POSIÇÃO ORIGINAL
            // ====================================================

            items.Remove(draggedItem);


            // ====================================================
            // 3. LOCALIZAR DESTINO
            // ====================================================

            var targetIndex =
                items.IndexOf(targetItem);


            // ====================================================
            // 4. INSERIR NA NOVA POSIÇÃO
            // ====================================================

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


            // ====================================================
            // 5. RECALCULAR ORDER
            // ====================================================

            for (int i = 0; i < items.Count; i++)
            {
                items[i].Order =
                    i + 1;
            }


            // ====================================================
            // 6. ATUALIZAR INTERFACE
            // ====================================================

            ExercisesList.ItemsSource =
                null;

            ExercisesList.ItemsSource =
                items;


            // ====================================================
            // 7. SALVAR NO SERVIDOR
            // ====================================================

            bool success =
                await ApiService.ReorderExercisesAsync(
                    _workoutId,
                    items.Select(ex => ex.Id).ToList());


            // ====================================================
            // 8. VERIFICAR RESULTADO DA API
            // ====================================================

            if (!success)
            {
                await DisplayAlertAsync(
                    "Erro",
                    $"Não foi possível salvar a nova ordem.\n\n{ApiService.LastError}",
                    "OK");


                // Volta para a ordem realmente salva
                await LoadWorkoutDetails();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Erro",
                $"Não foi possível salvar a nova ordem:\n{ex.Message}",
                "OK");


            // ====================================================
            // RESTAURAR ORDEM DO SERVIDOR
            // ====================================================

            await LoadWorkoutDetails();
        }
        finally
        {
            // ====================================================
            // RESTAURAR VISUAL
            // ====================================================

            await RestoreDraggedBorderAsync();


            // ====================================================
            // LIMPAR ITEM ARRASTADO
            // ====================================================

            _draggedItem = null;
        }
    }
}