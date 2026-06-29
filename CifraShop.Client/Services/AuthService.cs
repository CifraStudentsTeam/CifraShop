using System.Net.Http.Json;
using System.Net.Http.Headers;
using CifraShop.Client.Models;

namespace CifraShop.Client.Services;

public class AuthService
{
    private readonly HttpClient _http;

    public AuthService(HttpClient http)
    {
        _http = http;
    }

    public string? Token { get; private set; }
    public string? CurrentUserEmail { get; private set; }
    public int? CurrentUserId { get; private set; }
    public int? CurrentBalance { get; private set; }
    public UserRole CurrentRole { get; private set; } = UserRole.Guest;
    public bool IsAuthenticated => CurrentRole != UserRole.Guest;
    public bool IsGuest => CurrentRole == UserRole.Guest;
    public event Action? OnAuthStateChanged;

    public async Task InitializeAsync()
    {
        if (!string.IsNullOrEmpty(Token))
            return;

        await GetGuestTokenAsync();
    }

    public async Task GetGuestTokenAsync()
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/Auth/guest-token", new { });
            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                if (authResponse != null)
                {
                    ApplyAuth(authResponse);
                }
            }
        }
        catch { }
    }

    public async Task<(bool success, string? error)> LoginAsync(string email, string password)
    {
        try
        {
            var request = new { Email = email, Password = password };
            var response = await _http.PostAsJsonAsync("api/Auth/login", request);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                if (authResponse != null)
                {
                    ApplyAuth(authResponse);
                    return (true, null);
                }
            }

            var error = await response.Content.ReadAsStringAsync();
            return (false, string.IsNullOrEmpty(error) ? "Неверный email или пароль" : error);
        }
        catch (Exception ex)
        {
            return (false, $"Ошибка соединения: {ex.Message}");
        }
    }

    public async Task<(bool success, string? error)> RegisterAsync(string email, string password)
    {
        try
        {
            var request = new { Email = email, Password = password };
            var response = await _http.PostAsJsonAsync("api/Auth/register-student", request);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                if (authResponse != null)
                {
                    ApplyAuth(authResponse);
                    return (true, null);
                }
            }

            var error = await response.Content.ReadAsStringAsync();
            return (false, string.IsNullOrEmpty(error) ? "Ошибка регистрации" : error);
        }
        catch (Exception ex)
        {
            return (false, $"Ошибка соединения: {ex.Message}");
        }
    }

    public async void Logout()
    {
        Token = null;
        CurrentUserEmail = null;
        CurrentUserId = null;
        CurrentBalance = null;
        CurrentRole = UserRole.Guest;

        _http.DefaultRequestHeaders.Authorization = null;

        OnAuthStateChanged?.Invoke();
        await GetGuestTokenAsync();
    }

    public async Task RefreshBalanceAsync()
    {
        if (!IsAuthenticated || IsGuest || !CurrentUserId.HasValue) return;

        try
        {
            var user = await _http.GetFromJsonAsync<UserDto>("api/User/profile");
            if (user != null)
            {
                CurrentBalance = user.Balance;
                OnAuthStateChanged?.Invoke();
            }
        }
        catch { }
    }

    private void ApplyAuth(AuthResponseDto authResponse)
    {
        Token = authResponse.Token;
        CurrentUserEmail = authResponse.Email;
        CurrentUserId = authResponse.UserId;
        CurrentRole = Enum.TryParse<UserRole>(authResponse.Role, out var role) ? role : UserRole.Guest;
        CurrentBalance = null;

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.Token);

        OnAuthStateChanged?.Invoke();
    }
}
