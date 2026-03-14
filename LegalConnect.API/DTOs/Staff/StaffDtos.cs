using System.ComponentModel.DataAnnotations;
using LegalConnect.API.Entities;

namespace LegalConnect.API.DTOs.Staff;

// ── Staff Role Constants ──────────────────────────────────────────────────

public static class StaffRoleTypeDto
{
    public const string Paralegal  = "Paralegal";
    public const string Secretary  = "Secretary";
    public const string Associate  = "Associate";
    public const string Intern     = "Intern";
    public const string Other      = "Other";
    public static readonly string[] All = [Paralegal, Secretary, Associate, Intern, Other];
}

// ── Staff Profile ─────────────────────────────────────────────────────────

public class StaffProfileDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int LawyerProfileId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string StaffRole { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateStaffDto
{
    [Required][MaxLength(50)]  public string FirstName { get; set; } = string.Empty;
    [Required][MaxLength(50)]  public string LastName  { get; set; } = string.Empty;
    [Required][EmailAddress]   public string Email     { get; set; } = string.Empty;
    [Required][MinLength(8)]   public string Password  { get; set; } = string.Empty;
    [Required][MaxLength(50)]  public string StaffRole { get; set; } = StaffRoleTypeDto.Paralegal;
}

// ── Case Staff Assignment ─────────────────────────────────────────────────

public class CaseStaffDto
{
    public int Id { get; set; }
    public int StaffProfileId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string StaffRole { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
    public bool IsActive { get; set; }
    public bool CanAddActivity { get; set; }
    public bool CanUploadDocument { get; set; }
}

public class AssignStaffDto
{
    [Required] public int StaffProfileId { get; set; }
    public bool CanAddActivity { get; set; } = false;
    public bool CanUploadDocument { get; set; } = false;
}

public class StaffCasePermissionsDto
{
    public bool CanAddActivity { get; set; }
    public bool CanUploadDocument { get; set; }
}

// ── Staff Tasks ───────────────────────────────────────────────────────────

public class StaffTaskDto
{
    public int Id { get; set; }
    public int LawyerProfileId { get; set; }
    public int? AssignedToStaffProfileId { get; set; }
    public string? AssignedToName { get; set; }
    public int? CaseId { get; set; }
    public string? CaseTitle { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? StaffNote { get; set; }
    public string Priority { get; set; } = "Medium";
    public string Status { get; set; } = "Pending";
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsOverdue => Status != "Completed" && Status != "Cancelled"
                             && DueDate.HasValue && DueDate.Value.Date < DateTime.UtcNow.Date;
}

public class CreateStaffTaskDto
{
    [Required][MaxLength(300)] public string Title { get; set; } = string.Empty;
    [MaxLength(2000)]          public string? Description { get; set; }
    public int? AssignedToStaffProfileId { get; set; }
    public int? CaseId { get; set; }
    public string Priority { get; set; } = "Medium";
    public DateTime? DueDate { get; set; }
}

public class UpdateStaffTaskDto
{
    [MaxLength(300)] public string? Title { get; set; }
    [MaxLength(2000)] public string? Description { get; set; }
    public int? AssignedToStaffProfileId { get; set; }
    public int? CaseId { get; set; }
    public string? Priority { get; set; }
    public string? Status { get; set; }
    public DateTime? DueDate { get; set; }
}

public class UpdateTaskStatusDto
{
    [Required] public string Status { get; set; } = string.Empty;
    [MaxLength(1000)] public string? StaffNote { get; set; }
}
