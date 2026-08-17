using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace AlePulse.Mobile.Services;

public static class ApiService
{
    private static readonly HttpClient _client;
    public static string? LastError { get; private set; }

    static ApiService()
    {
        var baseUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5204"
            : "http://localhost:5204";

        _client = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public static void SetToken(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public static async Task<bool> IsUserLoggedInAsync()
    {
        var token = await SecureStorage.GetAsync("auth_token");
        if (!string.IsNullOrEmpty(token))
        {
            SetToken(token);
            return true;
        }
        return false;
    }

    public static async Task LogoutAsync()
    {
        SecureStorage.Remove("auth_token");
        _client.DefaultRequestHeaders.Authorization = null;
    }

    public static async Task<string?> LoginAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/Users/login", new { email, password });

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result?.Token != null)
            {
                await SecureStorage.SetAsync("auth_token", result.Token);
                SetToken(result.Token);
            }
            return result?.Token;
        }
        return null;
    }

    public static async Task<bool> CreateWorkoutAsync(string name, string description)
    {
        var newWorkout = new { name, description };
        var response = await _client.PostAsJsonAsync("/api/Workouts", newWorkout);
        return response.IsSuccessStatusCode;
    }

    public static async Task<List<Models.WorkoutDto>> GetWorkoutsAsync()
    {
        var response = await _client.GetAsync("/api/Workouts");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<Models.WorkoutDto>>();
        }
        return new List<Models.WorkoutDto>();
    }

    public static async Task<Models.WorkoutDetailDto?> GetWorkoutByIdAsync(Guid id)
    {
        var response = await _client.GetAsync($"/api/Workouts/{id}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Models.WorkoutDetailDto>();
        }
        return null;
    }

    public static async Task<bool> DeleteWorkoutAsync(Guid id)
    {
        var response = await _client.DeleteAsync($"/api/Workouts/{id}");
        return response.IsSuccessStatusCode;
    }

    public static async Task<List<Models.ExerciseDto>> GetExercisesAsync()
    {
        var response = await _client.GetAsync("/api/Exercises");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<Models.ExerciseDto>>();
        }
        return new List<Models.ExerciseDto>();
    }

    public static async Task<bool> AddExerciseToWorkoutAsync(Guid workoutId, Guid exerciseId, int sets, int reps, decimal weight, int rest)
    {
        var dto = new { exerciseId, sets, repetitions = reps, weight, restSeconds = rest };
        var response = await _client.PostAsJsonAsync($"/api/Workouts/{workoutId}/exercises", dto);
        return response.IsSuccessStatusCode;
    }

    public static async Task<bool> DeleteWorkoutExerciseAsync(Guid workoutId, Guid workoutExerciseId)
    {
        var response = await _client.DeleteAsync($"/api/Workouts/{workoutId}/exercises/{workoutExerciseId}");
        return response.IsSuccessStatusCode;
    }

    public static async Task<Models.ExerciseDto?> CreateExerciseAsync(string name)
    {
        var dto = new { name, primaryMuscleGroup = "Personalizado", secondaryMuscleGroup = "", equipment = "", difficulty = "Intermediário", instructions = "" };
        var response = await _client.PostAsJsonAsync("/api/Exercises", dto);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Models.ExerciseDto>();
        }
        return null;
    }

    public static async Task<List<Models.ExerciseSetDto>> GetHistoryAsync(Guid exerciseId)
    {
        var response = await _client.GetAsync($"/api/History/{exerciseId}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<Models.ExerciseSetDto>>();
        }
        return new List<Models.ExerciseSetDto>();
    }

    public static async Task<bool> LogSetAsync(Guid workoutId, Guid exerciseId, int setNumber, decimal weight, int reps)
    {
        var dto = new { setNumber, weight, repetitions = reps };
        var response = await _client.PostAsJsonAsync($"/api/History/{workoutId}/{exerciseId}", dto);
        return response.IsSuccessStatusCode;
    }

    public static async Task<bool> UpdateSetAsync(Guid setId, int setNumber, decimal weight, int reps)
    {
        var dto = new { setNumber, weight, repetitions = reps };
        var response = await _client.PutAsJsonAsync($"/api/History/{setId}", dto);

        if (!response.IsSuccessStatusCode)
        {
            LastError = $"{response.StatusCode} - {await response.Content.ReadAsStringAsync()}";
        }
        return response.IsSuccessStatusCode;
    }

    public static async Task<bool> DeleteSetAsync(Guid setId)
    {
        var response = await _client.DeleteAsync($"/api/History/{setId}");

        if (!response.IsSuccessStatusCode)
        {
            LastError = $"{response.StatusCode} - {await response.Content.ReadAsStringAsync()}";
        }
        return response.IsSuccessStatusCode;
    }
    public static async Task<bool> UpdateWorkoutExerciseAsync(Guid workoutId, Guid exerciseId, int sets, int reps, decimal weight, int rest)
    {
        var dto = new { sets, repetitions = reps, weight, restSeconds = rest };
        var response = await _client.PutAsJsonAsync($"/api/Workouts/{workoutId}/exercises/{exerciseId}", dto);

        if (!response.IsSuccessStatusCode)
        {
            LastError = $"{response.StatusCode} - {await response.Content.ReadAsStringAsync()}";
        }
        return response.IsSuccessStatusCode;
    }

    public class LoginResponse
    {
        public string? Token { get; set; }
    }
}