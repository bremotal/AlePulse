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
        LoadingSpinner.IsVisible = true;
        LoadingSpinner.IsRunning = true;

        try
        {
            var workouts = await ApiService.GetWorkoutsAsync();
            WorkoutsList.ItemsSource = workouts;
        }
        catch (Exception)
        {
            WorkoutsList.ItemsSource = new List<WorkoutDto>();
        }
        finally
        {
            LoadingSpinner.IsRunning = false;
            LoadingSpinner.IsVisible = false;
        }
    }

    private void OnCreateWorkoutClicked(object sender, EventArgs e)
    {
        Application.Current!.MainPage = new CreateWorkoutPage();
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await ApiService.LogoutAsync();
        Application.Current!.MainPage = new LoginPage();
    }

    private async void OnWorkoutTapped(object sender, TappedEventArgs e)
    {
        if (sender is Border border && border.BindingContext is WorkoutDto workout)
        {
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
                await LoadWorkouts();
            }
        }
    }

    // NOVO MÉTODO: Abrir tela de perfil
    private void OnProfileClicked(object sender, EventArgs e)
    {
        Application.Current!.MainPage = new ProfilePage();
    }
}