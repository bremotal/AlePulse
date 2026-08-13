using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AlePulse.Mobile.Services;

public static class ApiService
{
    private static readonly HttpClient _client;

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

    public static async Task<string?> LoginAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/Users/login", new { email, password });

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
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
}

public class LoginResponse
{
    public string? Token { get; set; }
}