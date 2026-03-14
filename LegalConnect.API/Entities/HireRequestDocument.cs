namespace LegalConnect.API.Entities;

public class HireRequestDocument
{
    public int Id { get; set; }
    public int HireRequestId { get; set; }

    public string FileName    { get; set; } = string.Empty;
    public string FilePath    { get; set; } = string.Empty;
    public long   FileSize    { get; set; }
    public string ContentType { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public HireRequest HireRequest { get; set; } = null!;
}
