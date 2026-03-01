using System.Net.Http.Json;
using System.Security.Claims;
using LegalConnect.Client.Helpers;
using LegalConnect.Client.Models.Auth;
using Microsoft.AspNetCore.Components.Authorization;

namespace LegalConnect.Client.Services;

public class AuthService : IAuthService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly JwtAuthenticationStateProvider _authProvider;
    private readonly AuthenticationStateProvider _stateProvider;

    public AuthService(
        IHttpClientFactory httpFactory,
        JwtAuthenticationStateProvider authProvider,
        AuthenticationStateProvider stateProvider)
    {
        _httpFactory   = httpFactory;
        _authProvider  = authProvider;
        _stateProvider = stateProvider;
    }

    // ── Login ────────────────────────────────────────────────────────────────

    public async Task<(bool Success, string? Error)> LoginAsync(LoginDto dto)
    {
        try
        {
            var client   = _httpFactory.CreateClient("public");
            var response = await client.PostAsJsonAsync("auth/login", dto);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadFromJsonAsync<ApiResponse>();
                return (false, err?.Message ?? "Login failed. Please check your credentials.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
            if (result?.Data?.Token is null)
                return (false, "Invalid response from server.");

            await _authProvider.SetTokenAsync(result.Data.Token);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Connection error: {ex.Message}");
        }
    }

    // ── Register Client ──────────────────────────────────────────────────────

    public async Task<(bool Success, string? Error)> RegisterClientAsync(RegisterClientDto dto)
    {
        try
        {
            var client   = _httpFactory.CreateClient("public");
            var response = await client.PostAsJsonAsync("auth/register/client", dto);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadFromJsonAsync<ApiResponse>();
                return (false, err?.Message ?? "Registration failed.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
            if (result?.Data?.Token is null)
                return (false, "Invalid response from server.");

            await _authProvider.SetTokenAsync(result.Data.Token);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Connection error: {ex.Message}");
        }
    }

    // ── Register Lawyer ──────────────────────────────────────────────────────

    public async Task<(bool Success, string? Error)> RegisterLawyerAsync(RegisterLawyerDto dto)
    {
        try
        {
            var client   = _httpFactory.CreateClient("public");
            var response = await client.PostAsJsonAsync("auth/register/lawyer", dto);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadFromJsonAsync<ApiResponse>();
                return (false, err?.Message ?? "Registration failed.");
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Connection error: {ex.Message}");
        }
    }

    // ── Forgot Password ──────────────────────────────────────────────────────

    public async Task<(bool Success, string? Error)> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        try
        {
            var client   = _httpFactory.CreateClient("public");
            var response = await client.PostAsJsonAsync("auth/forgot-password", dto);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadFromJsonAsync<ApiResponse>();
                return (false, err?.Message ?? "Request failed.");
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Connection error: {ex.Message}");
        }
    }

    // ── Google Login ──────────────────────────────────────────────────────────

    public async Task<(bool Success, string? Error)> GoogleLoginAsync(string credential)
    {
        try
        {
            var client   = _httpFactory.CreateClient("public");
            var response = await client.PostAsJsonAsync("auth/google-login",
                new GoogleLoginDto { Credential = credential });

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadFromJsonAsync<ApiResponse>();
                return (false, err?.Message ?? "Google login failed.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
            if (result?.Data?.Token is null)
                return (false, "Invalid response from server.");

            await _authProvider.SetTokenAsync(result.Data.Token);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Connection error: {ex.Message}");
        }
    }

    // ── Logout ───────────────────────────────────────────────────────────────

    public async Task LogoutAsync() => await _authProvider.ClearTokenAsync();

    // ── Helpers ──────────────────────────────────────────────────────────────

    public async Task<string?> GetCurrentUserRoleAsync()
    {
        var state = await _stateProvider.GetAuthenticationStateAsync();
        return state.User.FindFirst(ClaimTypes.Role)?.Value;
    }

    public async Task<string?> GetCurrentUserNameAsync()
    {
        var state = await _stateProvider.GetAuthenticationStateAsync();
        return state.User.FindFirst(ClaimTypes.Name)?.Value
            ?? state.User.FindFirst("name")?.Value;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var state = await _stateProvider.GetAuthenticationStateAsync();
        return state.User.Identity?.IsAuthenticated ?? false;
    }
}
