namespace LegalConnect.API.Entities;

public enum AdminStaffRole
{
    LawyerApprovalManager = 0,
    UserManagementStaff = 1,
    SupportStaff = 2,
    DisputeRefundManager = 3,
    FinanceStaff = 4
}

public class AdminStaffProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Department { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedByUserId { get; set; }

    // Navigation
    public ApplicationUser User { get; set; } = null!;
    public ApplicationUser CreatedBy { get; set; } = null!;
    public List<AdminStaffRoleAssignment> RoleAssignments { get; set; } = [];
}

public class AdminStaffRoleAssignment
{
    public int Id { get; set; }
    public int AdminStaffProfileId { get; set; }
    public AdminStaffRole Role { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public int AssignedByUserId { get; set; }

    // Navigation
    public AdminStaffProfile AdminStaffProfile { get; set; } = null!;
    public ApplicationUser AssignedBy { get; set; } = null!;
}
