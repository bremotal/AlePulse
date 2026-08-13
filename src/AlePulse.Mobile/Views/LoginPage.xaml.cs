using AlePulse.Mobile.Services;

namespace AlePulse.Mobile.Views;

public partial class LoginPage : ContentPage
{
    private readonly ApiService _apiService;

    public LoginPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;
        LoginButton.Text = "ENTRANDO...";
        LoginButton.IsEnabled = false;

        var token = await _apiService.LoginAsync(EmailEntry.Text, PasswordEntry.Text);

        if (!string.IsNullOrEmpty(token))
        {
            // Login deu certo! (Método atualizado para o .NET 10)
            await DisplayAlertAsync("Sucesso", "Login realizado com sucesso!", "OK");

            // No futuro, navegar para a tela principal (Home)
        }
        else
        {
            ErrorLabel.Text = "E-mail ou senha inválidos.";
            ErrorLabel.IsVisible = true;
        }

        LoginButton.Text = "ENTRAR";
        LoginButton.IsEnabled = true;
    }
}