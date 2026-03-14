using System.ComponentModel.DataAnnotations;

namespace LegalConnect.Client.Models.Admin;

public class CreateAdminStaffModel
{
    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Department { get; set; }

    public List<string> Roles { get; set; } = [];
}

public class UpdateAdminStaffRolesModel
{
    public List<string> Roles { get; set; } = [];
}

public class ResetAdminStaffPasswordModel
{
    [Required, MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
}

public class AdminStaffDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Department { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Roles { get; set; } = [];
    public string CreatedByName { get; set; } = string.Empty;
}

public class AdminStaffRoleInfoDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
