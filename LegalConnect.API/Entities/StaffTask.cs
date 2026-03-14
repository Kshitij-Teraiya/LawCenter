namespace LegalConnect.API.Entities;

public enum TaskPriority
{
    Low,
    Medium,
    High,
    Urgent
}

public enum StaffTaskStatus
{
    Pending,
    InProgress,
    Completed,
    Cancelled
}

public class StaffTask
{
    public int Id { get; set; }

    /// <summary>Lawyer who created and owns this task.</summary>
    public int LawyerProfileId { get; set; }

    /// <summary>Null means the task is unassigned (visible in lawyer task board).</summary>
    public int? AssignedToStaffProfileId { get; set; }

    /// <summary>Optional: task linked to a specific case.</summary>
    public int? CaseId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Progress note added by staff when updating their status.</summary>
    public string? StaffNote { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public StaffTaskStatus Status { get; set; } = StaffTaskStatus.Pending;

    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public LawyerProfile LawyerProfile { get; set; } = null!;
    public StaffProfile? AssignedTo { get; set; }
    public Case? Case { get; set; }
}
