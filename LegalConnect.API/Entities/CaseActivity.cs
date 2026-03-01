namespace LegalConnect.API.Entities;

public enum ActivityType
{
    Hearing,
    DocumentUpload,
    Note,
    StatusChange,
    DocumentDownload,
    CaseCreated,
    CaseClosed
}

public class CaseActivity
{
    public int Id { get; set; }
    public int CaseId { get; set; }
    public int CreatedByUserId { get; set; }

    public ActivityType ActivityType { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime ActivityDate { get; set; } = DateTime.UtcNow;
    public string CreatedByRole { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;

    // Navigation
    public Case Case { get; set; } = null!;
    public ApplicationUser CreatedBy { get; set; } = null!;
}
