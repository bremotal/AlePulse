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
        var baseUrl = "https://alepulse-api.onrender.com";
        _client = new HttpClient { BaseAddress = new Uri(baseUrl), Timeout = TimeSpan.FromSeconds(60) };
        _client.DefaultRequestHeaders.Add("ngrok-skip-browser-warning", "69420");
    }

    private static void SetLastError(string message)
    {
        LastError = message;
        CrashReportService.AddBreadcrumb($"API erro: {message[..Math.Min(150, message.Length)]}");
    }

    private static string NetworkError(Exception ex)
    {
        return ex is TaskCanceledException ? "Tempo esgotado (timeout)" : $"Falha de rede: {ex.Message}";
    }

    public static string GetAbsoluteUrl(string relativeUrl)
    {
        if (string.IsNullOrEmpty(relativeUrl)) return string.Empty;
        if (relativeUrl.StartsWith("http")) return relativeUrl;
        return $"{_client.BaseAddress}{relativeUrl.TrimStart('/')}";
    }

    public static void SetToken(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public static async Task<bool> IsUserLoggedInAsync()
    {
        try
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (!string.IsNullOrEmpty(token))
            {
                SetToken(token);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<List<Models.WorkoutDto>> GetWorkoutsAsync()
    {
        var programs = await GetProgramsAsync();
        return programs
            .SelectMany(program => program.Workouts ?? Enumerable.Empty<Models.WorkoutDto>())
            .ToList();
    }

    public static Task LogoutAsync()
    {
        try
        {
            SecureStorage.Remove("auth_token");
            _client.DefaultRequestHeaders.Authorization = null;
        }
        catch { }

        return Task.CompletedTask;
    }

    public static async Task<string?> LoginAsync(string email, string password)
    {
        try
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
        catch (Exception ex)
        {
            SetLastError(NetworkError(ex));
            return null;
        }
    }

    public static Task<bool> CreateWorkoutAsync(string name, string? description)
    {
        return CreateProgramAsync(name, description ?? string.Empty);
    }

    public static async Task<bool> RegisterAsync(string name, string email, string password)
    {
        try
        {
            var dto = new { name, email, password };
            var response = await _client.PostAsJsonAsync("/api/Users/register", dto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            SetLastError(NetworkError(ex));
            return false;
        }
    }

    public static async Task<List<Models.WorkoutProgramDto>> GetProgramsAsync()
    {
        try
        {
            var response = await _client.GetAsync("/api/WorkoutPrograms");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<Models.WorkoutProgramDto>>() ?? new List<Models.WorkoutProgramDto>();
            }
            return new List<Models.WorkoutProgramDto>();
        }
        catch (Exception ex)
        {
            SetLastError(NetworkError(ex));
            return new List<Models.WorkoutProgramDto>();
        }
    }

    public static async Task<Models.WorkoutProgramDto?> GetProgramByIdAsync(Guid id)
    {
        try
        {
            var response = await _client.GetAsync($"/api/WorkoutPrograms/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Models.WorkoutProgramDto>();
            }
            return null;
        }
        catch (Exception ex)
        {
            SetLastError(NetworkError(ex));
            return null;
        }
    }

    public static async Task<bool> CreateProgramAsync(string name, string description)
    {
        try
        {
            var dto = new { name, description };
            var response = await _client.PostAsJsonAsync("/api/WorkoutPrograms", dto);
            if (!response.IsSuccessStatusCode) SetLastError($"{response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            SetLastError(NetworkError(ex));
            return false;
        }
    }

    public static async Task<bool> UpdateProgramAsync(Guid id, string name, string description)
    {
        try
        {
            var dto = new { name, description };
            var response = await _client.PutAsJsonAsync($"/api/WorkoutPrograms/{id}", dto);
            if (!response.IsSuccessStatusCode) SetLastError($"{response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            SetLastError(NetworkError(ex));
            return false;
        }
    }

    public static async Task<bool> DeleteProgramAsync(Guid id)
    {
        try
        {
            var response = await _client.DeleteAsync($"/api/WorkoutPrograms/{id}");
            if (!response.IsSuccessStatusCode) SetLastError($"{response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            SetLastError(NetworkError(ex));
            return false;
        }
    }

    public static async Task<bool> AddWorkoutToProgramAsync(Guid programId, string name, string description)
    {
        try
        {
            var dto = new { name, description };
            var response = await _client.PostAsJsonAsync($"/api/WorkoutPrograms/{programId}/workouts", dto);
            if (!response.IsSuccessStatusCode) SetLastError($"{response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            SetLastError(NetworkError(ex));
            return false;
        }
    }

    public static async Task<Models.WorkoutDetailDto?> GetWorkoutByIdAsync(Guid id)
    {
        try
        {
            var response = await _client.GetAsync($"/api/Workouts/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Models.WorkoutDetailDto>();
            }
            return null;
        }
        catch (Exception ex)
        {
            SetLastError(NetworkError(ex));
            return null;
        }
    }

    public static async Task<bool> UpdateWorkoutAsync(Guid id, string name, string description)
    {
        try
        {
            var dto = new { name, description };
            var response = await _client.PutAsJsonAsync($"/api/Workouts/{id}", dto);
            if (!response.IsSuccessStatusCode) SetLastError($"{response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            SetLastError(NetworkError(ex));
            return false;
        }
    }

    public static async Task<bool> DeleteWorkoutAsync(Guid id)
    {
        try
        {
            var response = await _client.DeleteAsync($"/api/Workouts/{id}");
            if (!response.IsSuccessStatusCode) SetLastError($"{response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            SetLastError(NetworkError(ex));
            return false;
        }
    }

    public static async Task<bool> AddExerciseToWorkoutAsync(Guid workoutId, Guid exerciseId, int sets, int reps, decimal weight, int rest)
    {
        try
        {
            var dto = new { exerciseId, sets, repetitions = reps, weight, restSeconds = rest };
            var response = await _client.PostAsJsonAsync($"/api/Workouts/{workoutId}/exercises", dto);
            if (!response.IsSuccessStatusCode) SetLastError($"{response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            SetLastError(NetworkError(ex));
            return false;
        }
    }

    public static async Task<bool> DeleteWorkoutExerciseAsync(Guid workoutId, Guid workoutExerciseId)
    {
        try
        {
            var response = await _client.DeleteAsync($"/api/Workouts/{workoutId}/exercises/{workoutExerciseId}");
            if (!response.IsSuccessStatusCode) SetLastError($"{response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            SetLastError(NetworkError(ex));
            return false;
        }
    }

    public static async Task<bool> UpdateWorkoutExerciseAsync(Guid workoutId, Guid exerciseId, int sets, int reps, decimal weight, int rest)
    {
        try
        {
            var dto = new { sets, repetitions = reps, weight, restSeconds = rest };
            var response = await _client.PutAsJsonAsync($"/api/Workouts/{workoutId}/exercises/{exerciseId}", dto);
            if (!response.IsSuccessStatusCode) SetLastError($"{response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            SetLastError(NetworkError(ex));
            return false;
        }
    }

    public static async Task<bool> ReorderExercisesAsync(Guid workoutId, List<Guid> exerciseIds)
    {
        try
        {
            var dto = new { exerciseIds };
            var response = await _client.PutAsJsonAsync($"/api/Workouts/{workoutId}/exercises/reorder", dto);
            if (!response.IsSuccessStatusCode)
            {
                SetLastError($"{response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            SetLastError(NetworkError(ex));
            return false;
        }
    }

    public static async Task<List<Models.ExerciseDto>> GetExercisesAsync()
    {
        try
        {
            var response = await _client.GetAsync("/api/Exercises");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<Models.ExerciseDto>>() ?? new List<Models.ExerciseDto>();
            }
            return new List<Models.ExerciseDto>();
        }
        catch (Exception ex)
        {
            SetLastError(NetworkError(ex));
            return new List<Models.ExerciseDto>();
        }
    }

    public static async Task<Models.ExerciseDto?> CreateExerciseAsync(string name)
    {
        try
        {
            var dto = new { name, primaryMuscleGroup = "Personalizado", secondaryMuscleGroup = "", equipment = "", difficulty = "Intermediário", instructions = "" };
            var response = await _client.PostAsJsonAsync("/api/Exercises", dto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Models.ExerciseDto>();
            }
            return null;
        }
        catch (Exception ex)
        {
            SetLastError(NetworkError(ex));
            return null;
        }
    }

    public static async Task<bool> DeleteExerciseAsync(Guid exerciseId)
    {
        try
        {
            var response = await _client.DeleteAsync($"/api/Exercises/{exerciseId}");
            if (!response.IsSuccessStatusCode)
            {
                SetLastError($"{response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            SetLastError(NetworkError(ex));
            return false;
        }
    }

    public static async Task<bool> UploadExerciseImageAsync(Guid exerciseId, FileResult file)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            var stream = await file.OpenReadAsync();
            content.Add(new StreamContent(stream), "file", file.FileName);

            var response = await _client.PostAsync($"/api/Exercises/{exerciseId}/media/upload", content);
            if (!response.IsSuccessStatusCode) SetLastError($"{response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            SetLastError(NetworkError(ex));
            return false;
        }
    }

    public static async Task<List<Models.ExerciseSetDto>> GetHistoryAsync(Guid exerciseId)
    {
        try
        {
            var response = await _client.GetAsync($"/api/History/{exerciseId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<Models.ExerciseSetDto>>() ?? new List<Models.ExerciseSetDto>();
            }
            return new List<Models.ExerciseSetDto>();
        }
        catch (Exception ex)
        {
            SetLastError(NetworkError(ex));
            return new List<Models.ExerciseSetDto>();
        }
    }

    public static async Task<List<Guid>> GetCompletedExercisesTodayAsync(Guid workoutId)
    {
        try
        {
            var response = await _client.GetAsync($"/api/History/completed-today/{workoutId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<Guid>>() ?? new List<Guid>();
            }
            return new List<Guid>();
        }
        catch (Exception ex)
        {
            SetLastError(NetworkError(ex));
            return new List<Guid>();
        }
    }

    public static async Task<bool> LogSetAsync(Guid workoutId, Guid exerciseId, int setNumber, decimal weight, int reps)
    {
        try
        {
            var dto = new { setNumber, weight, repetitions = reps };
            var response = await _client.PostAsJsonAsync($"/api/History/{workoutId}/{exerciseId}", dto);
            if (!response.IsSuccessStatusCode) SetLastError($"{response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            SetLastError(NetworkError(ex));
            return false;
        }
    }

    public static async Task<bool> UpdateSetAsync(Guid setId, int setNumber, decimal weight, int reps)
    {
        try
        {
            var dto = new { setNumber, weight, repetitions = reps };
            var response = await _client.PutAsJsonAsync($"/api/History/{setId}", dto);
            if (!response.IsSuccessStatusCode) SetLastError($"{response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            SetLastError(NetworkError(ex));
            return false;
        }
    }

    public static async Task<bool> DeleteSetAsync(Guid setId)
    {
        try
        {
            var response = await _client.DeleteAsync($"/api/History/{setId}");
            if (!response.IsSuccessStatusCode) SetLastError($"{response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            SetLastError(NetworkError(ex));
            return false;
        }
    }

    public static async Task<Models.ProfileDto?> GetMyProfileAsync()
    {
        try
        {
            var response = await _client.GetAsync("/api/Users/me");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Models.ProfileDto>();
            }
            return null;
        }
        catch (Exception ex)
        {
            SetLastError(NetworkError(ex));
            return null;
        }
    }

    public static async Task<bool> UpdateProfileAsync(string name, string email, decimal? weight, decimal? height, string? goal)
    {
        try
        {
            var dto = new { name, email, weight, height, trainingGoal = goal };
            var response = await _client.PutAsJsonAsync("/api/Users/update-profile", dto);

            if (!response.IsSuccessStatusCode)
            {
                SetLastError($"{response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            SetLastError(NetworkError(ex));
            return false;
        }
    }

    public static async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        try
        {
            var dto = new { currentPassword, newPassword };
            var response = await _client.PutAsJsonAsync("/api/Users/change-password", dto);

            if (!response.IsSuccessStatusCode)
            {
                SetLastError($"{response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            SetLastError(NetworkError(ex));
            return false;
        }
    }
}

public class LoginResponse
{
    public string? Token { get; set; }
}
