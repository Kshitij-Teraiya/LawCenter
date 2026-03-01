namespace LegalConnect.API.Entities;

public class Certification
{
    public int Id { get; set; }
    public int LawyerProfileId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string IssuingOrganization { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? CertificateUrl { get; set; }

    // Navigation
    public LawyerProfile LawyerProfile { get; set; } = null!;
}
