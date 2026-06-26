using System.Net.Http.Json;

namespace CifraShop.Client.Services;

public class AuthService
{
    private readonly HttpClient _http;

    public AuthService(HttpClient http)
    {
        _http = http;
    }

    public string? CurrentUserEmail { get; private set; }
    public int? CurrentUserId { get; private set; }
    public int? CurrentBalance { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(CurrentUserEmail);
    public event Action? OnAuthStateChanged;

    public async Task<(bool success, string? error)> LoginAsync(string email, string password)
    {
        try
        {
            var request = new { Email = email, Password = password };
            var response = await _http.PostAsJsonAsync("api/Auth/login", request);

            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadFromJsonAsync<Models.UserDto>();
                if (user != null)
                {
                    CurrentUserEmail = user.Email;
                    CurrentUserId = user.Id;
                    CurrentBalance = user.Balance;
                    OnAuthStateChanged?.Invoke();
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
            var response = await _http.PostAsJsonAsync("api/User/create-student", request);

            if (response.IsSuccessStatusCode)
            {
                return await LoginAsync(email, password);
            }

            var error = await response.Content.ReadAsStringAsync();
            return (false, string.IsNullOrEmpty(error) ? "Ошибка регистрации" : error);
        }
        catch (Exception ex)
        {
            return (false, $"Ошибка соединения: {ex.Message}");
        }
    }

    public void Logout()
    {
        CurrentUserEmail = null;
        CurrentUserId = null;
        CurrentBalance = null;
        OnAuthStateChanged?.Invoke();
    }

    public async Task RefreshBalanceAsync()
    {
        if (!IsAuthenticated || !CurrentUserId.HasValue) return;

        try
        {
            var user = await _http.GetFromJsonAsync<Models.UserDto>($"api/User/by-id?id={CurrentUserId.Value}");
            if (user != null)
            {
                CurrentBalance = user.Balance;
                OnAuthStateChanged?.Invoke();
            }
        }
        catch { }
    }
}