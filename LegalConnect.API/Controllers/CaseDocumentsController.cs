using System.Security.Claims;
using LegalConnect.API.DTOs.Cases;
using LegalConnect.API.Helpers;
using LegalConnect.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LegalConnect.API.Controllers;

[ApiController]
[Authorize(Roles = "Lawyer,Client,Admin")]
public class CaseDocumentsController : ControllerBase
{
    private readonly ICaseDocumentService _service;

    public CaseDocumentsController(ICaseDocumentService service)
    {
        _service = service;
    }

    /// <summary>List documents for a case.</summary>
    [HttpGet("api/cases/{caseId:int}/documents")]
    public async Task<IActionResult> GetDocuments(int caseId, [FromQuery] CaseDocumentFilterDto filter)
    {
        var docs = await _service.GetDocumentsAsync(GetUserId(), GetUserRole(), caseId, filter);
        return Ok(ApiResponse<List<CaseDocumentDto>>.Ok(docs));
    }

    /// <summary>Upload a document to a case (Lawyer or Client).</summary>
    [HttpPost("api/cases/{caseId:int}/documents")]
    [RequestSizeLimit(11 * 1024 * 1024)] // 11 MB limit (validation is 10 MB)
    public async Task<IActionResult> UploadDocument(
        int caseId,
        IFormFile file,
        [FromForm] UploadDocumentDto dto)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse.Fail("No file provided."));

        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var (success, message, data) = await _service.UploadDocumentAsync(
            GetUserId(), GetUserRole(), GetUserName(), caseId, file, dto);

        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse<CaseDocumentDto>.Ok(data!, message));
    }

    /// <summary>Securely download a document (validates ownership).</summary>
    [HttpGet("api/case-documents/{id:int}/download")]
    public async Task<IActionResult> DownloadDocument(int id)
    {
        var (document, canAccess) = await _service.GetDocumentForDownloadAsync(GetUserId(), GetUserRole(), id);

        if (document == null) return NotFound(ApiResponse.Fail("Document not found."));
        if (!canAccess) return Forbid();

        var stream = HttpContext.RequestServices
            .GetRequiredService<IFileUploadService>()
            .GetFileStream(document.FilePath);

        if (stream == null) return NotFound(ApiResponse.Fail("File not found on server."));

        return File(stream, document.ContentType, document.FileName);
    }

    /// <summary>Delete a document (uploader or lawyer of the case).</summary>
    [HttpDelete("api/case-documents/{id:int}")]
    public async Task<IActionResult> DeleteDocument(int id)
    {
        var (success, message) = await _service.DeleteDocumentAsync(GetUserId(), GetUserRole(), id);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException());

    private string GetUserRole() =>
        User.FindFirstValue(ClaimTypes.Role)
        ?? User.FindFirstValue("role")
        ?? "Client";

    private string GetUserName() =>
        User.FindFirstValue(ClaimTypes.Name)
        ?? User.FindFirstValue("fullName")
        ?? "Unknown";
}
