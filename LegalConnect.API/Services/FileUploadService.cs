namespace LegalConnect.API.Services;

public interface IFileUploadService
{
    // Case documents
    Task<(string StoredFileName, string FilePath)> SaveFileAsync(IFormFile file, int caseId);
    Task DeleteFileAsync(string filePath);
    FileStream? GetFileStream(string filePath);
    bool FileExists(string filePath);

    // Hire-request documents (separate folder)
    Task<(string StoredFileName, string FilePath)> SaveHireRequestDocumentAsync(IFormFile file, int hireRequestId);
    FileStream? GetHireRequestDocumentStream(string filePath);
    Task DeleteHireRequestDocumentAsync(string filePath);
}

public class FileUploadService : IFileUploadService
{
    private readonly ILogger<FileUploadService> _logger;
    private readonly string _basePath;
    private readonly string _hrDocPath;

    public FileUploadService(IConfiguration config, ILogger<FileUploadService> logger)
    {
        _logger  = logger;
        _basePath = config["FileStorage:UploadPath"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "CaseDocuments");
        _hrDocPath = config["FileStorage:HireRequestDocPath"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "HireRequestDocs");
    }

    // ── Case documents ──────────────────────────────────────────────

    public async Task<(string StoredFileName, string FilePath)> SaveFileAsync(IFormFile file, int caseId)
    {
        var caseFolder = Path.Combine(_basePath, $"case_{caseId}");
        Directory.CreateDirectory(caseFolder);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(caseFolder, storedFileName);
        var relativePath = Path.Combine($"case_{caseId}", storedFileName);

        await using var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await file.CopyToAsync(stream);

        _logger.LogInformation("File saved: {StoredFileName} for case {CaseId}", storedFileName, caseId);
        return (storedFileName, relativePath);
    }

    public async Task DeleteFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("File deleted: {FilePath}", filePath);
        }
        await Task.CompletedTask;
    }

    public FileStream? GetFileStream(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        return File.Exists(fullPath)
            ? new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read)
            : null;
    }

    public bool FileExists(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        return File.Exists(fullPath);
    }

    // ── Hire-request documents ──────────────────────────────────────

    public async Task<(string StoredFileName, string FilePath)> SaveHireRequestDocumentAsync(IFormFile file, int hireRequestId)
    {
        var folder = Path.Combine(_hrDocPath, $"hr_{hireRequestId}");
        Directory.CreateDirectory(folder);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(folder, storedFileName);
        var relativePath = Path.Combine($"hr_{hireRequestId}", storedFileName);

        await using var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await file.CopyToAsync(stream);

        _logger.LogInformation("HireRequest doc saved: {StoredFileName} for HR {HireRequestId}", storedFileName, hireRequestId);
        return (storedFileName, relativePath);
    }

    public FileStream? GetHireRequestDocumentStream(string filePath)
    {
        var fullPath = Path.Combine(_hrDocPath, filePath);
        return File.Exists(fullPath)
            ? new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read)
            : null;
    }

    public async Task DeleteHireRequestDocumentAsync(string filePath)
    {
        var fullPath = Path.Combine(_hrDocPath, filePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("HireRequest doc deleted: {FilePath}", filePath);
        }
        await Task.CompletedTask;
    }
}
