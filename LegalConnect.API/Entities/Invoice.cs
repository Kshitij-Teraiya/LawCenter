namespace LegalConnect.API.Entities;

public enum InvoiceStatus
{
    Pending = 0,
    Accepted = 1,
    Paid = 2,
    Rejected = 3
}

public class Invoice
{
    public int Id { get; set; }
    public int ProposalId { get; set; }
    public int DealId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Proposal Proposal { get; set; } = null!;
    public Deal Deal { get; set; } = null!;
}
