using Microsoft.AspNetCore.Identity;

namespace LegalConnect.API.Entities;

public class ApplicationUser : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public string FullName => $"{FirstName} {LastName}".Trim();

    // Navigation
    public LawyerProfile? LawyerProfile { get; set; }
    public ClientProfile? ClientProfile { get; set; }
}
