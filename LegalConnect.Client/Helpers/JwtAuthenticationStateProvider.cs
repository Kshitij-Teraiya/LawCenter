using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace LegalConnect.Client.Helpers;

/// <summary>
/// Custom <see cref="AuthenticationStateProvider"/> that reads the JWT stored in
/// browser LocalStorage and populates <see cref="ClaimsPrincipal"/> accordingly.
/// </summary>
public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private const string TokenKey = "jwt_token";
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly ILocalStorageService _localStorage;

    public JwtAuthenticationStateProvider(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    // ── Public API ───────────────────────────────────────────────────────────

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _localStorage.GetItemAsStringAsync(TokenKey);
            if (string.IsNullOrWhiteSpace(token))
                return Anonymous;

            if (IsTokenExpired(token))
            {
                await ClearTokenAsync();
                return Anonymous;
            }

            var principal = BuildPrincipal(token);
            return new AuthenticationState(principal);
        }
        catch
        {
            return Anonymous;
        }
    }

    /// <summary>Persist a fresh token and notify Blazor of the auth change.</summary>
    public async Task SetTokenAsync(string token)
    {
        await _localStorage.SetItemAsStringAsync(TokenKey, token);
        var principal = BuildPrincipal(token);
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(principal)));
    }

    /// <summary>Remove the token and mark the user as anonymous.</summary>
    public async Task ClearTokenAsync()
    {
        await _localStorage.RemoveItemAsync(TokenKey);
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }

    /// <summary>Return the raw JWT string from storage.</summary>
    public async Task<string?> GetTokenAsync()
        => await _localStorage.GetItemAsStringAsync(TokenKey);

    // ── Private Helpers ──────────────────────────────────────────────────────

    private static ClaimsPrincipal BuildPrincipal(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token))
            return new ClaimsPrincipal(new ClaimsIdentity());

        var jwt = handler.ReadJwtToken(token);
        var claims = jwt.Claims.ToList();

        // Blazor needs ClaimTypes.Role – map "role" → ClaimTypes.Role if absent
        if (!claims.Any(c => c.Type == ClaimTypes.Role))
        {
            var roleClaims = jwt.Claims
                .Where(c => c.Type is "role" or "roles")
                .Select(c => new Claim(ClaimTypes.Role, c.Value));
            claims.AddRange(roleClaims);
        }

        var identity = new ClaimsIdentity(claims, "jwt");
        return new ClaimsPrincipal(identity);
    }

    private static bool IsTokenExpired(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token)) return true;
            var jwt = handler.ReadJwtToken(token);
            return jwt.ValidTo < DateTime.UtcNow;
        }
        catch
        {
            return true;
        }
    }
}
