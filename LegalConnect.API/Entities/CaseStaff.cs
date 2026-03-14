namespace LegalConnect.API.Entities;

/// <summary>
/// Junction table: assigns a StaffProfile to a Case.
/// Mirrors the CaseLawyer pattern — unique (CaseId, StaffProfileId).
/// </summary>
public class CaseStaff
{
    public int Id { get; set; }

    public int CaseId { get; set; }
    public int StaffProfileId { get; set; }

    public int AddedByUserId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    /// <summary>Whether this staff member can add activities/notes to the case timeline.</summary>
    public bool CanAddActivity { get; set; } = false;

    /// <summary>Whether this staff member can upload documents to the case.</summary>
    public bool CanUploadDocument { get; set; } = false;

    // Navigation
    public Case Case { get; set; } = null!;
    public StaffProfile StaffProfile { get; set; } = null!;
}
