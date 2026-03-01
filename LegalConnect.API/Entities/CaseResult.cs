namespace LegalConnect.API.Entities;

public class CaseResult
{
    public int Id { get; set; }
    public int LawyerProfileId { get; set; }
    public string CaseTitle { get; set; } = string.Empty;
    public string Court { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? Description { get; set; }

    // Navigation
    public LawyerProfile LawyerProfile { get; set; } = null!;
}
