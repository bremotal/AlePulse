using AlePulse.Mobile.Services;

namespace AlePulse.Mobile.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;
        LoginButton.Text = "ENTRANDO...";
        LoginButton.IsEnabled = false;

        var token = await ApiService.LoginAsync(EmailEntry.Text, PasswordEntry.Text);

        if (!string.IsNullOrEmpty(token))
        {
            // Guarda o token no ApiService para usar nas próximas requisições
            ApiService.SetToken(token);

            // Navega para a Home
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