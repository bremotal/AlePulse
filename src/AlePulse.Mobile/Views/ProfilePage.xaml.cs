using AlePulse.Mobile.Services;

namespace AlePulse.Mobile.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadProfile();
    }

    private async Task LoadProfile()
    {
        var profile = await ApiService.GetMyProfileAsync();
        if (profile != null)
        {
            NameLabel.Text = profile.Name;
            EmailLabel.Text = profile.Email;

            // Mostra os dados físicos (ou um traço se estiver vazio)
            WeightLabel.Text = profile.Weight.HasValue ? profile.Weight.Value.ToString("F1") : "---";
            HeightLabel.Text = profile.Height.HasValue ? profile.Height.Value.ToString("F0") : "---";
            GoalLabel.Text = !string.IsNullOrEmpty(profile.TrainingGoal) ? profile.TrainingGoal : "---";
        }
    }

    private async void OnEditProfileClicked(object sender, EventArgs e)
    {
        string newName = await DisplayPromptAsync("Editar Nome", "Digite seu nome:", "OK", "Cancelar", initialValue: NameLabel.Text);
        if (string.IsNullOrWhiteSpace(newName)) return;

        string newEmail = await DisplayPromptAsync("Editar E-mail", "Digite seu e-mail:", "OK", "Cancelar", initialValue: EmailLabel.Text, keyboard: Keyboard.Email);
        if (string.IsNullOrWhiteSpace(newEmail)) return;

        string weightStr = await DisplayPromptAsync("Peso (kg)", "Digite seu peso atual:", "OK", "Cancelar", initialValue: WeightLabel.Text == "---" ? "" : WeightLabel.Text, keyboard: Keyboard.Numeric);
        decimal? newWeight = decimal.TryParse(weightStr, out var w) ? w : null;

        string heightStr = await DisplayPromptAsync("Altura (cm)", "Digite sua altura:", "OK", "Cancelar", initialValue: HeightLabel.Text == "---" ? "" : HeightLabel.Text, keyboard: Keyboard.Numeric);
        decimal? newHeight = decimal.TryParse(heightStr, out var h) ? h : null;

        string newGoal = await DisplayPromptAsync("Objetivo", "Qual seu objetivo? (ex: Hipertrofia)", "OK", "Cancelar", initialValue: GoalLabel.Text == "---" ? "" : GoalLabel.Text);

        bool success = await ApiService.UpdateProfileAsync(newName, newEmail, newWeight, newHeight, newGoal);
        if (success)
        {
            await DisplayAlertAsync("Sucesso", "Dados atualizados!", "OK");
            await LoadProfile();
        }
        else
        {
            await DisplayAlertAsync("Erro", $"Não foi possível atualizar.\n{ApiService.LastError}", "OK");
        }
    }

    private async void OnChangePasswordClicked(object sender, EventArgs e)
    {
        string currentPass = await DisplayPromptAsync("Senha Atual", "Digite sua senha atual:", "OK", "Cancelar");
        if (string.IsNullOrWhiteSpace(currentPass)) return;

        string newPass = await DisplayPromptAsync("Nova Senha", "Digite sua nova senha:", "OK", "Cancelar");
        if (string.IsNullOrWhiteSpace(newPass)) return;

        bool success = await ApiService.ChangePasswordAsync(currentPass, newPass);
        if (success)
        {
            await DisplayAlertAsync("Sucesso", "Senha alterada com sucesso!", "OK");
        }
        else
        {
            await DisplayAlertAsync("Erro", $"Não foi possível alterar a senha.\n{ApiService.LastError}", "OK");
        }
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await ApiService.LogoutAsync();
        Application.Current!.MainPage = new LoginPage();
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        Application.Current!.MainPage = new ProgramsPage();
    }

    private async void OnDebugClicked(object sender, EventArgs e)
    {
        var token = await SecureStorage.GetAsync("auth_token");
        var lastError = ApiService.LastError ?? "Nenhum erro registrado.";

        await DisplayAlertAsync("Debug Info",
            $"Token: {(token != null ? token.Substring(0, 20) + "..." : "Nulo")}\n\n" +
            $"Último Erro API:\n{lastError}", "OK");
    }
}