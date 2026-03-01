namespace LegalConnect.API.DTOs.Lawyer;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? IconClass { get; set; }
    public string? Description { get; set; }
    public int LawyerCount { get; set; }
}
