using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AlePulse.Mobile.Services;

public class ApiService
{
    private readonly HttpClient _client;

    public ApiService()
    {
        // ATENÇÃO: Troque a porta abaixo pela porta HTTP que a sua API está usando!
        // Olhe no terminal do Visual Studio qual é a porta (ex: http://localhost:5204)
        var baseUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5204" // 10.0.2.2 é como o emulador Android acessa o localhost do PC
            : "http://localhost:5204";

        _client = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public async Task<string?> LoginAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/Users/login", new { email, password });

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            return result?.Token;
        }
        return null;
    }
}

// Classe auxiliar para ler o JSON do token
public class LoginResponse
{
    public string? Token { get; set; }
}