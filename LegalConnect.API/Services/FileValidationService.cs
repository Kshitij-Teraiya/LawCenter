namespace LegalConnect.API.Services;

public interface IFileValidationService
{
    (bool IsValid, string Error) Validate(IFormFile file);
}

public class FileValidationService : IFileValidationService
{
    private static readonly HashSet<string> AllowedExtensions =
        [".pdf", ".docx", ".jpg", ".jpeg", ".png"];

    private static readonly HashSet<string> AllowedContentTypes =
    [
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "image/jpeg",
        "image/png"
    ];

    // Magic bytes for file type verification
    private static readonly Dictionary<string, byte[]> MagicBytes = new()
    {
        { ".pdf",  [0x25, 0x50, 0x44, 0x46] },          // %PDF
        { ".docx", [0x50, 0x4B, 0x03, 0x04] },           // PK (ZIP-based)
        { ".jpg",  [0xFF, 0xD8, 0xFF] },
        { ".jpeg", [0xFF, 0xD8, 0xFF] },
        { ".png",  [0x89, 0x50, 0x4E, 0x47] }
    };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public (bool IsValid, string Error) Validate(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return (false, "No file provided or file is empty.");

        if (file.Length > MaxFileSizeBytes)
            return (false, $"File exceeds maximum allowed size of 10 MB.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return (false, $"File type '{extension}' is not allowed. Allowed: PDF, DOCX, JPG, PNG.");

        if (!AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            return (false, "File content type is not allowed.");

        // Verify magic bytes
        if (MagicBytes.TryGetValue(extension, out var magic))
        {
            using var stream = file.OpenReadStream();
            var header = new byte[magic.Length];
            var bytesRead = stream.Read(header, 0, header.Length);
            if (bytesRead < magic.Length || !header.Take(magic.Length).SequenceEqual(magic))
                return (false, "File content does not match its declared type.");
        }

        return (true, string.Empty);
    }
}
