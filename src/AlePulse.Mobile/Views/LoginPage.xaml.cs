using AlePulse.Mobile.Services;

namespace AlePulse.Mobile.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        bool isLogged = await ApiService.IsUserLoggedInAsync();
        if (isLogged)
        {
            Application.Current!.MainPage = new ProgramsPage();
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
                // MUDAMOS DE HomePage PARA ProgramsPage AQUI!
                Application.Current!.MainPage = new ProgramsPage();
            }
            else
            {
                ErrorLabel.Text = "E-mail ou senha inválidos.";
                ErrorLabel.IsVisible = true;
            }
        }
        catch
        {
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

    private void OnRegisterClicked(object sender, EventArgs e)
    {
        Application.Current!.MainPage = new RegisterPage();
    }
}