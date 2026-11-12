using AlePulse.Mobile.Models;
using AlePulse.Mobile.Services;

namespace AlePulse.Mobile.Views;

public partial class ProgramsPage : ContentPage
{
    private static bool _crashPromptShown;

    public ProgramsPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        CrashReportService.AddBreadcrumb("ProgramsPage aberta");
        await LoadPrograms();
        await PromptCrashReportAsync();
    }

    private async Task LoadPrograms()
    {
        try
        {
            var programs = await ApiService.GetProgramsAsync();
            ProgramsList.ItemsSource = programs;
        }
        catch (Exception ex)
        {
            CrashReportService.AddBreadcrumb($"Erro ao carregar fichas: {ex.Message}");
        }
    }

    private async Task PromptCrashReportAsync()
    {
        try
        {
            if (_crashPromptShown || !CrashReportService.HasUnreportedCrashes) return;
            _crashPromptShown = true;
            bool report = await DisplayAlertAsync("Encontramos um problema",
                "O AlePulse registrou um fechamento inesperado ou erro neste celular.\n\nDeseja reportar agora e ajudar a corrigir?",
                "Reportar agora", "Depois");
            if (report) Application.Current!.Windows[0].Page = new ReportProblemPage();
        }
        catch { }
    }

    private async void OnCreateProgramClicked(object? sender, EventArgs e)
    {
        string name = await DisplayPromptAsync("Nova Ficha", "Nome da Ficha (ex: Hipertrofia ABC):", "OK", "Cancelar");
        if (string.IsNullOrWhiteSpace(name)) return;
        string desc = await DisplayPromptAsync("Descrição", "Descrição (opcional):", "OK", "Cancelar");

        bool success = await ApiService.CreateProgramAsync(name, desc);
        if (success) await LoadPrograms();
    }

    private async void OnEditProgramClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is WorkoutProgramDto program)
        {
            string newName = await DisplayPromptAsync("Editar Ficha", "Nome:", "OK", "Cancelar", initialValue: program.Name);
            if (string.IsNullOrWhiteSpace(newName)) return;
            string newDesc = await DisplayPromptAsync("Descrição", "Descrição:", "OK", "Cancelar", initialValue: program.Description ?? "");

            bool success = await ApiService.UpdateProgramAsync(program.Id, newName, newDesc);
            if (success) await LoadPrograms();
        }
    }

    private async void OnDeleteProgramClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is WorkoutProgramDto program)
        {
            bool confirm = await DisplayAlertAsync("Excluir", $"Excluir a ficha {program.Name}?", "Sim", "Não");
            if (confirm)
            {
                bool success = await ApiService.DeleteProgramAsync(program.Id);
                if (success) await LoadPrograms();
            }
        }
    }

    private void OnProgramTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border border && border.BindingContext is WorkoutProgramDto program)
        {
            Application.Current!.Windows[0].Page = new ProgramDetailPage(program.Id, program.Name);
        }
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        await ApiService.LogoutAsync();
        Application.Current!.Windows[0].Page = new LoginPage();
    }

    private void OnProfileClicked(object? sender, EventArgs e)
    {
        Application.Current!.Windows[0].Page = new ProfilePage();
    }
}