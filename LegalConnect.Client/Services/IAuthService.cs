using LegalConnect.Client.Models.Auth;

namespace LegalConnect.Client.Services;

public interface IAuthService
{
    Task<(bool Success, string? Error)> LoginAsync(LoginDto dto);
    Task<(bool Success, string? Error)> RegisterClientAsync(RegisterClientDto dto);
    Task<(bool Success, string? Error)> RegisterLawyerAsync(RegisterLawyerDto dto);
    Task<(bool Success, string? Error)> ForgotPasswordAsync(ForgotPasswordDto dto);
    Task<(bool Success, string? Error)> GoogleLoginAsync(string credential);
    Task LogoutAsync();
    Task<string?> GetCurrentUserRoleAsync();
    Task<string?> GetCurrentUserNameAsync();
    Task<bool> IsAuthenticatedAsync();
}
