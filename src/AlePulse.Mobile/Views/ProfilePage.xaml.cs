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
        }
    }

    private async void OnEditProfileClicked(object sender, EventArgs e)
    {
        string currentName = NameLabel.Text;
        string currentEmail = EmailLabel.Text;

        string newName = await DisplayPromptAsync("Editar Nome", "Digite seu nome:", "OK", "Cancelar", initialValue: currentName);
        if (string.IsNullOrWhiteSpace(newName)) return;

        string newEmail = await DisplayPromptAsync("Editar E-mail", "Digite seu e-mail:", "OK", "Cancelar", initialValue: currentEmail, keyboard: Keyboard.Email);
        if (string.IsNullOrWhiteSpace(newEmail)) return;

        bool success = await ApiService.UpdateProfileAsync(newName, newEmail);
        if (success)
        {
            await DisplayAlertAsync("Sucesso", "Dados atualizados!", "OK");
            await LoadProfile(); // Atualiza a tela
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
        Application.Current!.MainPage = new HomePage();
    }
}