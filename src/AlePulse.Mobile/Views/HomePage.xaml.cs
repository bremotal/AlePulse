namespace AlePulse.Mobile.Views;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }

    private void OnCreateWorkoutClicked(object sender, EventArgs e)
    {
        // Navega para a tela de criar treino
        Application.Current!.MainPage = new CreateWorkoutPage();
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        // Volta para a tela de Login
        Application.Current!.MainPage = new LoginPage();
    }
}