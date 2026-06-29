using System.Net.Http.Json;
using System.Net.Http.Headers;
using CifraShop.Client.Models;
using Microsoft.JSInterop;

namespace CifraShop.Client.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;

    private const string TokenKey = "cifrashop_token";
    private const string EmailKey = "cifrashop_email";
    private const string UserIdKey = "cifrashop_userid";
    private const string RoleKey = "cifrashop_role";

    public AuthService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
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

        // Попытка восстановить сессию из localStorage
        var savedToken = await _js.InvokeAsync<string?>("authStorage.load", TokenKey);
        if (!string.IsNullOrEmpty(savedToken))
        {
            var savedEmail = await _js.InvokeAsync<string?>("authStorage.load", EmailKey);
            var savedUserIdStr = await _js.InvokeAsync<string?>("authStorage.load", UserIdKey);
            var savedRoleStr = await _js.InvokeAsync<string?>("authStorage.load", RoleKey);

            if (!string.IsNullOrEmpty(savedRoleStr) && Enum.TryParse<UserRole>(savedRoleStr, out var role) && role != UserRole.Guest)
            {
                Token = savedToken;
                CurrentUserEmail = savedEmail;
                CurrentUserId = int.TryParse(savedUserIdStr, out var uid) ? uid : null;
                CurrentRole = role;
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", savedToken);
                OnAuthStateChanged?.Invoke();
                return;
            }
        }

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
                    await ApplyAuthAsync(authResponse);
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
                    await ApplyAuthAsync(authResponse);
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
                    await ApplyAuthAsync(authResponse);
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

    public async Task LogoutAsync()
    {
        Token = null;
        CurrentUserEmail = null;
        CurrentUserId = null;
        CurrentBalance = null;
        CurrentRole = UserRole.Guest;

        _http.DefaultRequestHeaders.Authorization = null;

        await _js.InvokeVoidAsync("authStorage.remove", TokenKey);
        await _js.InvokeVoidAsync("authStorage.remove", EmailKey);
        await _js.InvokeVoidAsync("authStorage.remove", UserIdKey);
        await _js.InvokeVoidAsync("authStorage.remove", RoleKey);

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
        catch (Exception ex)
        {
            Console.WriteLine($"[Auth] RefreshBalance ошибка: {ex.Message}");
        }
    }

    private async Task ApplyAuthAsync(AuthResponseDto authResponse)
    {
        Token = authResponse.Token;
        CurrentUserEmail = authResponse.Email;
        CurrentUserId = authResponse.UserId;
        CurrentRole = Enum.TryParse<UserRole>(authResponse.Role, out var role) ? role : UserRole.Guest;
        CurrentBalance = null;

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.Token);

        // Сохраняем в localStorage только если это НЕ guest
        if (CurrentRole != UserRole.Guest)
        {
            await _js.InvokeVoidAsync("authStorage.save", TokenKey, authResponse.Token);
            await _js.InvokeVoidAsync("authStorage.save", EmailKey, authResponse.Email);
            await _js.InvokeVoidAsync("authStorage.save", UserIdKey, authResponse.UserId.ToString());
            await _js.InvokeVoidAsync("authStorage.save", RoleKey, authResponse.Role);
        }

        OnAuthStateChanged?.Invoke();
    }
}
