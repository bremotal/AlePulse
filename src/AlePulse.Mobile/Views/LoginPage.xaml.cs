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
            // Login deu certo! Navega para a Home
            Application.Current!.MainPage = new HomePage();
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