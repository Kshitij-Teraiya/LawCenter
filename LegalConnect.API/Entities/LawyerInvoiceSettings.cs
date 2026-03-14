namespace LegalConnect.API.Entities;

public class LawyerInvoiceSettings
{
    public int Id { get; set; }
    public int LawyerProfileId { get; set; }

    public string? FirmName { get; set; }
    public string? FirmLogoPath { get; set; }
    public string? FirmAddress { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? GSTNumber { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? AuthorizedSignImagePath { get; set; }
    public string? BankDetails { get; set; }
    public string? NotesForInvoice { get; set; }
    public string? TermsAndConditions { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public LawyerProfile LawyerProfile { get; set; } = null!;
}
