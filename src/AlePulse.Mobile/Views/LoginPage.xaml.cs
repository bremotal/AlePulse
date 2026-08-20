using AlePulse.Mobile.Services;

namespace AlePulse.Mobile.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    // Executa sempre que a tela de login aparece
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Verifica se o usuário já está logado sem travar a tela
        bool isLogged = await ApiService.IsUserLoggedInAsync();
        if (isLogged)
        {
            // Se tiver token salvo, pula direto para a Home!
            Application.Current!.MainPage = new HomePage();
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;
        LoginButton.IsVisible = false;
        LoadingSpinner.IsVisible = true;
        LoadingSpinner.IsRunning = true;

        try
        {
            var token = await ApiService.LoginAsync(EmailEntry.Text, PasswordEntry.Text);

            if (!string.IsNullOrEmpty(token))
            {
                ApiService.SetToken(token);
                Application.Current!.MainPage = new HomePage();
            }
            else
            {
                ErrorLabel.Text = "E-mail ou senha inválidos.";
                ErrorLabel.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            // Se der erro de rede/conexão, mostra o erro sem fechar o app
            ErrorLabel.Text = "Erro de conexão com o servidor. Verifique o IP e o Firewall.";
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            LoadingSpinner.IsRunning = false;
            LoadingSpinner.IsVisible = false;
            LoginButton.IsVisible = true;
        }
    }
}