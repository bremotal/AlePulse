using AlePulse.Mobile.Models;
using AlePulse.Mobile.Services;

namespace AlePulse.Mobile.Views;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadWorkouts();
    }

    private async Task LoadWorkouts()
    {
        try
        {
            var workouts = await ApiService.GetWorkoutsAsync();
            WorkoutsList.ItemsSource = workouts;
        }
        catch (Exception)
        {
            WorkoutsList.ItemsSource = new List<WorkoutDto>();
        }
    }

    private void OnCreateWorkoutClicked(object sender, EventArgs e)
    {
        Application.Current!.MainPage = new CreateWorkoutPage();
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await ApiService.LogoutAsync(); // Apaga o token
        Application.Current!.MainPage = new LoginPage();
    }

    // Evento de clique em um treino da lista
    private async void OnWorkoutTapped(object sender, TappedEventArgs e)
    {
        if (sender is Border border && border.BindingContext is WorkoutDto workout)
        {
            // Navega direto para os detalhes passando o ID do treino
            Application.Current!.MainPage = new WorkoutDetailPage(workout.Id);
        }
    }
    private async void OnDeleteWorkoutClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is WorkoutDto workout)
        {
            bool confirm = await DisplayAlertAsync("Excluir", $"Deseja excluir o treino {workout.Name}?", "Sim", "Não");
            if (confirm)
            {
                await ApiService.DeleteWorkoutAsync(workout.Id);
                await LoadWorkouts(); // Recarrega a lista
            }
        }
    }
}