namespace LegalConnect.API.Entities;

public enum ProposalStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2
}

public class Proposal
{
    public int Id { get; set; }
    public int DealId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsNegotiable { get; set; } = false;
    public ProposalStatus Status { get; set; } = ProposalStatus.Pending;
    public string? ClientNote { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Deal Deal { get; set; } = null!;
    public Invoice? Invoice { get; set; }
}
