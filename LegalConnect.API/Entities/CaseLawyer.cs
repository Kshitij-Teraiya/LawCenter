namespace LegalConnect.API.Entities;

/// <summary>Junction entity: a case can have multiple lawyers assigned.</summary>
public class CaseLawyer
{
    public int Id { get; set; }
    public int CaseId { get; set; }
    public int LawyerProfileId { get; set; }

    /// <summary>Who added this lawyer to the case: "Client" or "Lawyer".</summary>
    public string AddedByRole { get; set; } = "Client";
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Navigation
    public Case Case { get; set; } = null!;
    public LawyerProfile LawyerProfile { get; set; } = null!;
}
