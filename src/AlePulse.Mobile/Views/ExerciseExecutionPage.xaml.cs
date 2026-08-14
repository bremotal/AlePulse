using AlePulse.Mobile.Models;
using AlePulse.Mobile.Services;
using System.Globalization;

namespace AlePulse.Mobile.Views;

public partial class ExerciseExecutionPage : ContentPage
{
    private readonly Guid _workoutId;
    private readonly Guid _exerciseId;
    private Guid? _editingSetId = null;

    public ExerciseExecutionPage(Guid workoutId, WorkoutExerciseDto exercise)
    {
        InitializeComponent();
        _workoutId = workoutId;
        _exerciseId = exercise.Exercise!.Id;

        TitleLabel.Text = exercise.Exercise.Name;
        WeightEntry.Text = exercise.Weight.ToString();
        RepsEntry.Text = exercise.Repetitions.ToString();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadHistory();
    }

    private async Task LoadHistory()
    {
        try
        {
            var history = await ApiService.GetHistoryAsync(_exerciseId);

            // Agrupa por dia e ordena do mais recente para o mais antigo
            var grouped = history
                .GroupBy(e => e.CompletedAt.Date)
                .Select(g => new GroupedExerciseSet(
                    g.Key.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                    g.OrderByDescending(x => x.CompletedAt).ToList()
                ))
                .OrderByDescending(g => g.DateDisplay)
                .ToList();

            HistoryList.ItemsSource = grouped;

            // Se não estiver editando, calcula o próximo número de série automaticamente
            if (!_editingSetId.HasValue)
            {
                // Pega apenas as séries feitas HOJE
                var todaySets = history.Where(x => x.CompletedAt.Date == DateTime.Now.Date).ToList();

                if (todaySets.Count > 0)
                {
                    // Se já fez séries hoje, pega o maior número e soma 1
                    SetEntry.Text = (todaySets.Max(x => x.SetNumber) + 1).ToString();
                }
                else
                {
                    // Se não fez nenhuma série hoje, começa do 1
                    SetEntry.Text = "1";
                }
            }
        }
        catch { }
    }
    private async void OnSaveSetClicked(object sender, EventArgs e)
    {
        SaveBtn.IsEnabled = false; // Bloqueia o botão para evitar cliques duplos

        if (!int.TryParse(SetEntry.Text, out int setNum) ||
            !decimal.TryParse(WeightEntry.Text, out decimal weight) ||
            !int.TryParse(RepsEntry.Text, out int reps))
        {
            await DisplayAlertAsync("Erro", "Preencha os valores corretamente.", "OK");
            SaveBtn.IsEnabled = true;
            return;
        }

        bool success = false;
        if (_editingSetId.HasValue)
        {
            // MODO EDIÇÃO
            success = await ApiService.UpdateSetAsync(_editingSetId.Value, setNum, weight, reps);
            if (success)
            {
                await DisplayAlertAsync("Sucesso", "Série atualizada!", "OK");
                _editingSetId = null;
                SaveBtn.Text = "REGISTRAR SÉRIE";
            }
            else
            {
                await DisplayAlertAsync("Erro API", $"Não foi possível atualizar.\n{ApiService.LastError}", "OK");
            }
        }
        else
        {
            // MODO INSERÇÃO
            success = await ApiService.LogSetAsync(_workoutId, _exerciseId, setNum, weight, reps);
            if (success)
            {
                await DisplayAlertAsync("Sucesso", "Série registrada!", "OK");
                SetEntry.Text = (setNum + 1).ToString(); // Incrementa automaticamente para a próxima série
            }
            else
            {
                await DisplayAlertAsync("Erro API", $"Não foi possível registrar.\n{ApiService.LastError}", "OK");
            }
        }

        if (success) await LoadHistory();

        SaveBtn.IsEnabled = true; // Libera o botão
    }

    private async void OnLoadHistoryClicked(object sender, EventArgs e)
    {
        var history = await ApiService.GetHistoryAsync(_exerciseId);
        if (history.Count > 0)
        {
            var lastSet = history.OrderByDescending(x => x.CompletedAt).First();
            WeightEntry.Text = lastSet.Weight.ToString();
            RepsEntry.Text = lastSet.Repetitions.ToString();
            // Não alteramos o SetEntry aqui para não bagunçar a contagem atual
            await DisplayAlertAsync("Carregado", "Carga e repetições do último treino preenchidas.", "OK");
        }
        else
        {
            await DisplayAlertAsync("Aviso", "Sem histórico anterior para este exercício.", "OK");
        }
    }

    private void OnEditSetClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is ExerciseSetDto set)
        {
            _editingSetId = set.Id;
            SetEntry.Text = set.SetNumber.ToString();
            WeightEntry.Text = set.Weight.ToString();
            RepsEntry.Text = set.Repetitions.ToString();
            SaveBtn.Text = "ATUALIZAR SÉRIE";
        }
    }

    private async void OnDeleteSetClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is ExerciseSetDto set)
        {
            bool confirm = await DisplayAlertAsync("Excluir", "Excluir esta série?", "Sim", "Não");
            if (confirm)
            {
                bool success = await ApiService.DeleteSetAsync(set.Id);
                if (success) await LoadHistory();
                else await DisplayAlertAsync("Erro API", $"Não foi possível excluir.\n{ApiService.LastError}", "OK");
            }
        }
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        Application.Current!.MainPage = new WorkoutDetailPage(_workoutId);
    }
}