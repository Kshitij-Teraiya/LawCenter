namespace LegalConnect.API.Entities;

// ── Enums ────────────────────────────────────────────────────────────────────

public enum TicketCategory
{
    Billing = 0,
    Technical = 1,
    Account = 2,
    General = 3
}

public enum TicketPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Urgent = 3
}

public enum TicketStatus
{
    Open = 0,
    InProgress = 1,
    Resolved = 2,
    Closed = 3
}

// ── Entities ─────────────────────────────────────────────────────────────────

public class SupportTicket
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public string UserRole { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public TicketCategory Category { get; set; } = TicketCategory.General;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public TicketStatus Status { get; set; } = TicketStatus.Open;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }
    public int? ClosedByUserId { get; set; }

    // Navigation
    public ApplicationUser User { get; set; } = null!;
    public ApplicationUser? ClosedByUser { get; set; }
    public List<SupportMessage> Messages { get; set; } = [];
}

public class SupportMessage
{
    public int Id { get; set; }

    public int SupportTicketId { get; set; }
    public int SenderUserId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderRole { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public SupportTicket SupportTicket { get; set; } = null!;
    public ApplicationUser Sender { get; set; } = null!;
}
