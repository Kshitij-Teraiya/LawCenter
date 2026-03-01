namespace LegalConnect.API.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconClass { get; set; } = "bi bi-briefcase";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public List<LawyerProfile> Lawyers { get; set; } = [];
    public int LawyerCount => Lawyers.Count(l => l.IsVerified);
}
