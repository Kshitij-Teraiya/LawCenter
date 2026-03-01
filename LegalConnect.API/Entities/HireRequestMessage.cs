namespace LegalConnect.API.Entities;

public class HireRequestMessage
{
    public int Id { get; set; }
    public int HireRequestId { get; set; }
    public int SenderUserId { get; set; }
    public string SenderRole { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;

    // Navigation
    public HireRequest HireRequest { get; set; } = null!;
    public ApplicationUser Sender { get; set; } = null!;
}
