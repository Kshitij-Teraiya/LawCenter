using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LegalConnect.API.Entities;
using Microsoft.IdentityModel.Tokens;

namespace LegalConnect.API.Helpers;

public class JwtHelper
{
    private readonly IConfiguration _config;

    public JwtHelper(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(ApplicationUser user, string role, IEnumerable<string>? adminStaffRoles = null)
    {
        var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiryMinutes = int.Parse(_config["Jwt:ExpiryMinutes"] ?? "1440");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, role),
            new("role", role),
            new("fullName", user.FullName),
            new("profilePicture", user.ProfilePictureUrl ?? "")
        };

        // Add admin staff sub-role claims
        if (adminStaffRoles != null)
        {
            foreach (var staffRole in adminStaffRoles)
                claims.Add(new Claim("adminStaffRole", staffRole));
        }

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public DateTime GetExpiry()
    {
        var expiryMinutes = int.Parse(_config["Jwt:ExpiryMinutes"] ?? "1440");
        return DateTime.UtcNow.AddMinutes(expiryMinutes);
    }
}
