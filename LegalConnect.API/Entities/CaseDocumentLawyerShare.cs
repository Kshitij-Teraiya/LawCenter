namespace LegalConnect.API.Entities;

/// <summary>
/// When a client uploads a document to a case with multiple lawyers and chooses
/// to share with SPECIFIC lawyers (not all), this table records which lawyers
/// can see that document.
/// </summary>
public class CaseDocumentLawyerShare
{
    public int Id { get; set; }
    public int CaseDocumentId { get; set; }
    public int LawyerProfileId { get; set; }

    // Navigation
    public CaseDocument CaseDocument { get; set; } = null!;
    public LawyerProfile LawyerProfile { get; set; } = null!;
}
