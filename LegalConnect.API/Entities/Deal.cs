namespace LegalConnect.API.Entities;

public enum DealStatus
{
    Negotiation = 0,
    ProposalSent = 1,
    ProposalAccepted = 2,
    InvoiceGenerated = 3,
    Paid = 4,
    Completed = 5
}

public class Deal
{
    public int Id { get; set; }
    public int HireRequestId { get; set; }
    public int ClientProfileId { get; set; }
    public int LawyerProfileId { get; set; }
    public DealStatus Status { get; set; } = DealStatus.Negotiation;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public HireRequest HireRequest { get; set; } = null!;
    public ClientProfile ClientProfile { get; set; } = null!;
    public LawyerProfile LawyerProfile { get; set; } = null!;
    public List<Proposal> Proposals { get; set; } = [];
    public List<Invoice> Invoices { get; set; } = [];
}
