using AlePulse.Mobile.Services;

namespace AlePulse.Mobile.Views;

public partial class CreateWorkoutPage : ContentPage
{
    public CreateWorkoutPage()
    {
        InitializeComponent();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            await DisplayAlertAsync("Aviso", "Digite o nome do treino.", "OK");
            return;
        }

        var success = await ApiService.CreateWorkoutAsync(NameEntry.Text, DescriptionEntry.Text);

        if (success)
        {
            await DisplayAlertAsync("Sucesso", "Treino criado!", "OK");
            // Volta para a Home
            Application.Current!.MainPage = new HomePage();
        }
        else
        {
            await DisplayAlertAsync("Erro", "Não foi possível salvar o treino.", "OK");
        }
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Application.Current!.MainPage = new HomePage();
    }
}