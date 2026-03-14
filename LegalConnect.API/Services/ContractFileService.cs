namespace LegalConnect.API.Services;

public interface IContractFileService
{
    Task<string> SaveContractAsync(byte[] pdfBytes, string subfolder, string fileName);
    FileStream? GetContractStream(string relativePath);
    Task DeleteContractAsync(string relativePath);
}

public class ContractFileService : IContractFileService
{
    private readonly string _basePath;
    private readonly ILogger<ContractFileService> _logger;

    public ContractFileService(ILogger<ContractFileService> logger)
    {
        _logger = logger;
        _basePath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Contracts");
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveContractAsync(byte[] pdfBytes, string subfolder, string fileName)
    {
        var folder = Path.Combine(_basePath, subfolder);
        Directory.CreateDirectory(folder);

        var fullPath = Path.Combine(folder, fileName);
        await File.WriteAllBytesAsync(fullPath, pdfBytes);

        var relativePath = Path.Combine(subfolder, fileName).Replace('\\', '/');
        _logger.LogInformation("Contract saved: {RelativePath}", relativePath);
        return relativePath;
    }

    public FileStream? GetContractStream(string relativePath)
    {
        var fullPath = Path.Combine(_basePath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(fullPath)
            ? new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read)
            : null;
    }

    public async Task DeleteContractAsync(string relativePath)
    {
        var fullPath = Path.Combine(_basePath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("Contract deleted: {RelativePath}", relativePath);
        }
        await Task.CompletedTask;
    }
}
