namespace LegalConnect.API.Entities;

public class Experience
{
    public int Id { get; set; }
    public int LawyerProfileId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public string? Description { get; set; }

    // Navigation
    public LawyerProfile LawyerProfile { get; set; } = null!;
}
