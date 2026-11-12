using AlePulse.Mobile.Services;
using Microsoft.Maui.ApplicationModel;

namespace AlePulse.Mobile.Views;

public partial class ReportProblemPage : ContentPage
{
    private readonly bool _fromProfile;

    public ReportProblemPage(bool fromProfile = false)
    {
        InitializeComponent();
        _fromProfile = fromProfile;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CrashReportService.AddBreadcrumb("ReportProblemPage aberta");
        LogsList.ItemsSource = CrashReportService.GetUnreportedLogs();
    }

    private void OnBackClicked(object? sender, EventArgs e)
    {
        Application.Current!.Windows[0].Page = _fromProfile ? new ProfilePage() : new ProgramsPage();
    }

    private async void OnSendClicked(object? sender, EventArgs e)
    {
        try
        {
            var body = CrashReportService.BuildReportBody(DescriptionEditor.Text);
            var message = new EmailMessage
            {
                Subject = "[AlePulse] Relato de problema",
                Body = body,
                To = new List<string> { CrashReportService.SupportEmail }
            };
            await Email.ComposeAsync(message);
            CrashReportService.MarkAllReported();
            await DisplayAlertAsync("Obrigado!", "Seu app de e-mail foi aberto com o relatório pronto. Confira e envie a mensagem para concluir.", "OK");
            Application.Current!.Windows[0].Page = _fromProfile ? new ProfilePage() : new ProgramsPage();
        }
        catch (FeatureNotSupportedException)
        {
            await CopyToClipboardAsync("Nenhum app de e-mail encontrado no celular.");
        }
        catch (Exception ex)
        {
            CrashReportService.AddBreadcrumb($"Falha ao enviar relatório: {ex.Message}");
            await CopyToClipboardAsync("Não foi possível abrir o e-mail. Relatório copiado:");
        }
    }

    private async Task CopyToClipboardAsync(string title)
    {
        try
        {
            await Clipboard.SetTextAsync(CrashReportService.BuildReportBody(DescriptionEditor.Text));
            await DisplayAlertAsync(title, "O relatório completo foi copiado para a área de transferência. Cole em qualquer mensagem para nos enviar.", "OK");
        }
        catch
        {
            await DisplayAlertAsync("Atenção", $"Não foi possível copiar. Envie manualmente para: {CrashReportService.SupportEmail}", "OK");
        }
    }

    private async void OnCopyClicked(object? sender, EventArgs e)
    {
        await CopyToClipboardAsync("Relatório copiado!");
    }

    private void OnLogTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border border && border.BindingContext is CrashLogItem item)
        {
            LogDetailLabel.Text = item.Content;
            LogDetailOverlay.IsVisible = true;
        }
    }

    private void OnCloseDetailClicked(object? sender, EventArgs e)
    {
        LogDetailOverlay.IsVisible = false;
    }
}
