namespace LegalConnect.API.Services;

public interface IFileUploadService
{
    Task<(string StoredFileName, string FilePath)> SaveFileAsync(IFormFile file, int caseId);
    Task DeleteFileAsync(string filePath);
    FileStream? GetFileStream(string filePath);
    bool FileExists(string filePath);
}

public class FileUploadService : IFileUploadService
{
    private readonly IConfiguration _config;
    private readonly ILogger<FileUploadService> _logger;
    private readonly string _basePath;

    public FileUploadService(IConfiguration config, ILogger<FileUploadService> logger)
    {
        _config = config;
        _logger = logger;
        _basePath = config["FileStorage:UploadPath"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "CaseDocuments");
    }

    public async Task<(string StoredFileName, string FilePath)> SaveFileAsync(IFormFile file, int caseId)
    {
        // Store in subfolder per case for organization
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
}
