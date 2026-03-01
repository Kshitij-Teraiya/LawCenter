namespace LegalConnect.API.Entities;

public class CaseDocument
{
    public int Id { get; set; }
    public int CaseId { get; set; }
    public int UploadedByUserId { get; set; }

    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;       // Original filename
    public string StoredFileName { get; set; } = string.Empty;  // UUID-based stored filename
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }                          // Bytes
    public string ContentType { get; set; } = string.Empty;

    public bool IsPrivate { get; set; } = false;                // Private to uploader only
    public string UploadedByRole { get; set; } = "Lawyer";     // "Lawyer" or "Client"

    /// <summary>
    /// When false, only lawyers listed in CaseDocumentLawyerShares can see this document.
    /// When true (default), all lawyers assigned to the case can see it.
    /// Only relevant for client-uploaded, non-private documents.
    /// </summary>
    public bool SharedWithAllLawyers { get; set; } = true;

    public bool IsDeleted { get; set; } = false;
    public DateTime UploadedDate { get; set; } = DateTime.UtcNow;

    // Navigation
    public Case Case { get; set; } = null!;
    public ApplicationUser UploadedBy { get; set; } = null!;
    public List<CaseDocumentLawyerShare> LawyerShares { get; set; } = [];
}
