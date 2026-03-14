using System.ComponentModel.DataAnnotations;

namespace LegalConnect.Client.Models.Support;

public class SupportTicketDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? ClosedByUserName { get; set; }
    public int MessageCount { get; set; }
}

public class CreateSupportTicketDto
{
    [Required, MaxLength(300)]
    public string Subject { get; set; } = string.Empty;

    [Required, MaxLength(3000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Category { get; set; } = "General";

    public string Priority { get; set; } = "Medium";
}

public class SupportMessageDto
{
    public int Id { get; set; }
    public int SupportTicketId { get; set; }
    public int SenderUserId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderRole { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class SendSupportMessageDto
{
    [Required, MaxLength(4000)]
    public string Message { get; set; } = string.Empty;
}

public class UpdateTicketStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}

public static class TicketCategories
{
    public static readonly string[] All = ["Billing", "Technical", "Account", "General"];
}

public static class TicketPriorities
{
    public static readonly string[] All = ["Low", "Medium", "High", "Urgent"];
}

public static class TicketStatuses
{
    public static readonly string[] All = ["Open", "InProgress", "Resolved", "Closed"];
}
