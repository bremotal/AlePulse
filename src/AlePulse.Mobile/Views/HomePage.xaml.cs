namespace AlePulse.Mobile.Views;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        // Volta para a tela de Login
        Application.Current!.MainPage = new LoginPage();
    }
}