namespace LegalConnect.API.Entities;

public class Publication
{
    public int Id { get; set; }
    public int LawyerProfileId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? Url { get; set; }

    // Navigation
    public LawyerProfile LawyerProfile { get; set; } = null!;
}
