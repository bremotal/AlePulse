using AlePulse.Mobile.Services;

namespace AlePulse.Mobile.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;
        RegisterButton.IsVisible = false;
        LoadingSpinner.IsVisible = true;
        LoadingSpinner.IsRunning = true;

        try
        {
            bool success = await ApiService.RegisterAsync(NameEntry.Text, EmailEntry.Text, PasswordEntry.Text);
            if (success)
            {
                await DisplayAlertAsync("Sucesso", "Conta criada! Faça o login.", "OK");
                Application.Current!.MainPage = new LoginPage();
            }
            else
            {
                ErrorLabel.Text = "Não foi possível cadastrar. E-mail já existe?";
                ErrorLabel.IsVisible = true;
            }
        }
        catch
        {
            ErrorLabel.Text = "Erro de conexão com o servidor.";
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            LoadingSpinner.IsRunning = false;
            LoadingSpinner.IsVisible = false;
            RegisterButton.IsVisible = true;
        }
    }

    private void OnBackToLoginClicked(object sender, EventArgs e)
    {
        Application.Current!.MainPage = new LoginPage();
    }
}