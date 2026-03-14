namespace LegalConnect.API.Entities;

public static class StaffRoleType
{
    public const string Paralegal      = "Paralegal";
    public const string Secretary      = "Secretary";
    public const string Associate      = "Associate";
    public const string Intern         = "Intern";
    public const string Other          = "Other";

    public static readonly string[] All =
        [Paralegal, Secretary, Associate, Intern, Other];
}

public class StaffProfile
{
    public int Id { get; set; }

    /// <summary>The login account for this staff member.</summary>
    public int UserId { get; set; }

    /// <summary>Which lawyer's firm this staff member belongs to.</summary>
    public int LawyerProfileId { get; set; }

    /// <summary>One of the <see cref="StaffRoleType"/> constants.</summary>
    public string StaffRole { get; set; } = StaffRoleType.Paralegal;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ApplicationUser User { get; set; } = null!;
    public LawyerProfile LawyerProfile { get; set; } = null!;
    public List<CaseStaff> CaseStaffs { get; set; } = [];
    public List<StaffTask> AssignedTasks { get; set; } = [];
}
