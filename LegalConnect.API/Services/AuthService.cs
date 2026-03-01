using Google.Apis.Auth;
using LegalConnect.API.Data;
using LegalConnect.API.DTOs.Auth;
using LegalConnect.API.Entities;
using LegalConnect.API.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LegalConnect.API.Services;

public interface IAuthService
{
    Task<(bool Success, string Message, AuthResponseDto? Data)> LoginAsync(LoginDto dto);
    Task<(bool Success, string Message, AuthResponseDto? Data)> RegisterClientAsync(RegisterClientDto dto);
    Task<(bool Success, string Message)> RegisterLawyerAsync(RegisterLawyerDto dto);
    Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordDto dto);
    Task<(bool Success, string Message, AuthResponseDto? Data)> GoogleLoginAsync(string credential);
}

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _db;
    private readonly JwtHelper _jwt;
    private readonly IConfiguration _config;

    public AuthService(UserManager<ApplicationUser> userManager, AppDbContext db, JwtHelper jwt, IConfiguration config)
    {
        _userManager = userManager;
        _db = db;
        _jwt = jwt;
        _config = config;
    }

    public async Task<(bool Success, string Message, AuthResponseDto? Data)> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !user.IsActive)
            return (false, "Invalid email or password.", null);

        var isValidPassword = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!isValidPassword)
            return (false, "Invalid email or password.", null);

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Client";

        var token = _jwt.GenerateToken(user, role);
        var response = new AuthResponseDto
        {
            Token = token,
            Email = user.Email ?? "",
            FullName = user.FullName,
            Role = role,
            ExpiresAt = _jwt.GetExpiry(),
            ProfilePictureUrl = user.ProfilePictureUrl
        };

        return (true, "Login successful.", response);
    }

    public async Task<(bool Success, string Message, AuthResponseDto? Data)> RegisterClientAsync(RegisterClientDto dto)
    {
        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing != null)
            return (false, "Email is already registered.", null);

        var user = new ApplicationUser
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            UserName = dto.Email,
            PhoneNumber = dto.PhoneNumber
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return (false, "Registration failed.", null);

        await _userManager.AddToRoleAsync(user, "Client");

        var clientProfile = new ClientProfile { UserId = user.Id, City = dto.City };
        _db.ClientProfiles.Add(clientProfile);
        await _db.SaveChangesAsync();

        var token = _jwt.GenerateToken(user, "Client");
        var response = new AuthResponseDto
        {
            Token = token,
            Email = user.Email ?? "",
            FullName = user.FullName,
            Role = "Client",
            ExpiresAt = _jwt.GetExpiry()
        };

        return (true, "Registration successful.", response);
    }

    public async Task<(bool Success, string Message)> RegisterLawyerAsync(RegisterLawyerDto dto)
    {
        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing != null)
            return (false, "Email is already registered.");

        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == dto.CategoryId && c.IsActive);
        if (!categoryExists)
            return (false, "Invalid category selected.");

        var user = new ApplicationUser
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            UserName = dto.Email,
            PhoneNumber = dto.PhoneNumber
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return (false, "Registration failed.");

        await _userManager.AddToRoleAsync(user, "Lawyer");

        var lawyerProfile = new LawyerProfile
        {
            UserId = user.Id,
            BarCouncilNumber = dto.BarCouncilNumber,
            CategoryId = dto.CategoryId,
            City = dto.City,
            Court = dto.Court,
            YearsOfExperience = dto.YearsOfExperience,
            ConsultationFee = dto.ConsultationFee,
            IsVerified = false
        };

        _db.LawyerProfiles.Add(lawyerProfile);
        await _db.SaveChangesAsync();

        return (true, "Registration successful. Your account is pending verification by an admin.");
    }

    public async Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        // Always return success to prevent email enumeration
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user != null)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            // In production: send email with reset link containing token
            // For now, we just acknowledge the request
        }
        return (true, "If this email is registered, you will receive a password reset link shortly.");
    }

    public async Task<(bool Success, string Message, AuthResponseDto? Data)> GoogleLoginAsync(string credential)
    {
        // 1. Validate the Google ID token
        GoogleJsonWebSignature.Payload payload;
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _config["Google:ClientId"] ?? "" }
            };
            payload = await GoogleJsonWebSignature.ValidateAsync(credential, settings);
        }
        catch (InvalidJwtException)
        {
            return (false, "Invalid Google token.", null);
        }

        var email = payload.Email;
        if (string.IsNullOrWhiteSpace(email))
            return (false, "Google account does not have an email address.", null);

        // 2. Check if user already exists
        var user = await _userManager.FindByEmailAsync(email);

        if (user != null)
        {
            if (!user.IsActive)
                return (false, "This account has been deactivated.", null);

            // Ensure the existing user is a Client (not Lawyer/Admin)
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Client";
            if (role != "Client")
                return (false, "This email is registered as a Lawyer/Admin account. Google login is only available for Clients.", null);

            // Update profile picture from Google if not set
            if (string.IsNullOrEmpty(user.ProfilePictureUrl) && !string.IsNullOrEmpty(payload.Picture))
            {
                user.ProfilePictureUrl = payload.Picture;
                await _userManager.UpdateAsync(user);
            }

            var token = _jwt.GenerateToken(user, "Client");
            return (true, "Login successful.", new AuthResponseDto
            {
                Token = token,
                Email = user.Email ?? "",
                FullName = user.FullName,
                Role = "Client",
                ExpiresAt = _jwt.GetExpiry(),
                ProfilePictureUrl = user.ProfilePictureUrl
            });
        }

        // 3. Auto-create new Client user (no password)
        var newUser = new ApplicationUser
        {
            FirstName = payload.GivenName ?? payload.Name?.Split(' ').FirstOrDefault() ?? "User",
            LastName = payload.FamilyName ?? payload.Name?.Split(' ').Skip(1).FirstOrDefault() ?? "",
            Email = email,
            UserName = email,
            EmailConfirmed = true,
            ProfilePictureUrl = payload.Picture
        };

        var createResult = await _userManager.CreateAsync(newUser);
        if (!createResult.Succeeded)
            return (false, "Failed to create account.", null);

        await _userManager.AddToRoleAsync(newUser, "Client");

        var clientProfile = new ClientProfile { UserId = newUser.Id };
        _db.ClientProfiles.Add(clientProfile);
        await _db.SaveChangesAsync();

        var newToken = _jwt.GenerateToken(newUser, "Client");
        return (true, "Account created successfully.", new AuthResponseDto
        {
            Token = newToken,
            Email = newUser.Email ?? "",
            FullName = newUser.FullName,
            Role = "Client",
            ExpiresAt = _jwt.GetExpiry(),
            ProfilePictureUrl = newUser.ProfilePictureUrl
        });
    }
}
