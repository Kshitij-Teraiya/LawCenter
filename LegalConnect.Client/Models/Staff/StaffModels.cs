namespace LegalConnect.Client.Models.Staff;

// ── Staff Role Constants ──────────────────────────────────────────────────

public static class StaffRoleType
{
    public const string Paralegal = "Paralegal";
    public const string Secretary = "Secretary";
    public const string Associate = "Associate";
    public const string Intern    = "Intern";
    public const string Other     = "Other";
    public static readonly string[] All = [Paralegal, Secretary, Associate, Intern, Other];
}

// ── Task Priority / Status ────────────────────────────────────────────────

public static class TaskPriorityType
{
    public const string Low    = "Low";
    public const string Medium = "Medium";
    public const string High   = "High";
    public const string Urgent = "Urgent";
    public static readonly string[] All = [Low, Medium, High, Urgent];
}

public static class StaffTaskStatusType
{
    public const string Pending    = "Pending";
    public const string InProgress = "InProgress";
    public const string Completed  = "Completed";
    public const string Cancelled  = "Cancelled";
    public static readonly string[] All = [Pending, InProgress, Completed, Cancelled];
}

// ── Staff Profile ─────────────────────────────────────────────────────────

public class StaffProfileModel
{
    public int      Id               { get; set; }
    public int      UserId           { get; set; }
    public int      LawyerProfileId  { get; set; }
    public string   FullName         { get; set; } = string.Empty;
    public string   Email            { get; set; } = string.Empty;
    public string   StaffRole        { get; set; } = string.Empty;
    public bool     IsActive         { get; set; }
    public DateTime CreatedAt        { get; set; }
}

public class CreateStaffModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName  { get; set; } = string.Empty;
    public string Email     { get; set; } = string.Empty;
    public string Password  { get; set; } = string.Empty;
    public string StaffRole { get; set; } = StaffRoleType.Paralegal;
}

// ── Case Staff Assignment ─────────────────────────────────────────────────

public class CaseStaffModel
{
    public int      Id                { get; set; }
    public int      StaffProfileId    { get; set; }
    public string   FullName          { get; set; } = string.Empty;
    public string   Email             { get; set; } = string.Empty;
    public string   StaffRole         { get; set; } = string.Empty;
    public DateTime AddedAt           { get; set; }
    public bool     IsActive          { get; set; }
    public bool     CanAddActivity    { get; set; }
    public bool     CanUploadDocument { get; set; }
}

public class AssignStaffModel
{
    public int  StaffProfileId    { get; set; }
    public bool CanAddActivity    { get; set; } = false;
    public bool CanUploadDocument { get; set; } = false;
}

public class StaffCasePermissionsModel
{
    public bool CanAddActivity    { get; set; }
    public bool CanUploadDocument { get; set; }
}

// ── Staff Tasks ───────────────────────────────────────────────────────────

public class StaffTaskModel
{
    public int      Id                       { get; set; }
    public int      LawyerProfileId          { get; set; }
    public int?     AssignedToStaffProfileId { get; set; }
    public string?  AssignedToName           { get; set; }
    public int?     CaseId                   { get; set; }
    public string?  CaseTitle                { get; set; }
    public string   Title                    { get; set; } = string.Empty;
    public string?  Description              { get; set; }
    public string?  StaffNote               { get; set; }
    public string   Priority                 { get; set; } = TaskPriorityType.Medium;
    public string   Status                   { get; set; } = StaffTaskStatusType.Pending;
    public DateTime? DueDate                 { get; set; }
    public DateTime? CompletedAt             { get; set; }
    public DateTime  CreatedAt               { get; set; }
    public DateTime  UpdatedAt               { get; set; }
    public bool      IsOverdue               =>
        Status != StaffTaskStatusType.Completed && Status != StaffTaskStatusType.Cancelled
        && DueDate.HasValue && DueDate.Value.Date < DateTime.UtcNow.Date;
}

public class CreateStaffTaskModel
{
    public string   Title                    { get; set; } = string.Empty;
    public string?  Description              { get; set; }
    public int?     AssignedToStaffProfileId { get; set; }
    public int?     CaseId                   { get; set; }
    public string   Priority                 { get; set; } = TaskPriorityType.Medium;
    public DateTime? DueDate                 { get; set; }
}

public class UpdateStaffTaskModel
{
    public string?  Title                    { get; set; }
    public string?  Description              { get; set; }
    public int?     AssignedToStaffProfileId { get; set; }
    public int?     CaseId                   { get; set; }
    public string?  Priority                 { get; set; }
    public string?  Status                   { get; set; }
    public DateTime? DueDate                 { get; set; }
}

public class UpdateTaskStatusModel
{
    public string  Status    { get; set; } = string.Empty;
    public string? StaffNote { get; set; }
}
