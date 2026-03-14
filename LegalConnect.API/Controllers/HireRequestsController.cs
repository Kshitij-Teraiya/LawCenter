using System.Security.Claims;
using LegalConnect.API.DTOs.Cases;
using LegalConnect.API.DTOs.Deals;
using LegalConnect.API.Helpers;
using LegalConnect.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LegalConnect.API.Controllers;

[ApiController]
[Route("api/hire-requests")]
[Authorize(Roles = "Lawyer,Client")]
public class HireRequestsController : ControllerBase
{
    private readonly IDealService                _service;
    private readonly IHireRequestDocumentService _docs;
    private readonly ICaseDocumentService        _caseDocs;

    public HireRequestsController(IDealService service, IHireRequestDocumentService docs, ICaseDocumentService caseDocs)
    {
        _service  = service;
        _docs     = docs;
        _caseDocs = caseDocs;
    }

    // ── HireRequest CRUD ─────────────────────────────────────────────

    [HttpPost]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> Create([FromBody] CreateHireRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
        var (success, message, data) = await _service.CreateHireRequestAsync(GetUserId(), dto);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse<HireRequestDto>.Ok(data!, message));
    }

    [HttpGet("my")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetMyHireRequests()
    {
        var result = await _service.GetMyHireRequestsAsync(GetUserId());
        return Ok(ApiResponse<List<HireRequestDto>>.Ok(result));
    }

    [HttpGet("my-invoices")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetMyInvoices()
    {
        var result = await _service.GetMyInvoicesAsync(GetUserId());
        return Ok(ApiResponse<List<ClientInvoiceSummaryDto>>.Ok(result));
    }

    [HttpGet("my-paid-invoices")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> GetMyPaidInvoices()
    {
        var result = await _service.GetLawyerPaidInvoicesAsync(GetUserId());
        return Ok(ApiResponse<List<LawyerPaidInvoiceDto>>.Ok(result));
    }

    [HttpGet("incoming")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> GetIncomingHireRequests()
    {
        var result = await _service.GetIncomingHireRequestsAsync(GetUserId());
        return Ok(ApiResponse<List<HireRequestDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetHireRequestByIdAsync(GetUserId(), GetRole(), id);
        if (result == null) return NotFound(ApiResponse.Fail("Hire request not found or access denied."));
        return Ok(ApiResponse<HireRequestDetailDto>.Ok(result));
    }

    [HttpPut("{id:int}/accept")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> Accept(int id)
    {
        var (success, message, data) = await _service.AcceptHireRequestAsync(GetUserId(), id);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse<DealDto>.Ok(data!, message));
    }

    [HttpPut("{id:int}/reject")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> Reject(int id)
    {
        var (success, message) = await _service.RejectHireRequestAsync(GetUserId(), id);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> Cancel(int id)
    {
        var (success, message) = await _service.CancelHireRequestAsync(GetUserId(), id);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    /// <summary>PUT api/hire-requests/{id}/cancel — Cancel after acceptance; accessible by both Client and Lawyer.</summary>
    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> CancelByParty(int id)
    {
        var (success, message) = await _service.CancelHireRequestByPartyAsync(GetUserId(), GetRole(), id);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    // ── Messages ─────────────────────────────────────────────────────

    [HttpGet("{id:int}/messages")]
    public async Task<IActionResult> GetMessages(int id)
    {
        var result = await _service.GetMessagesAsync(GetUserId(), id);
        return Ok(ApiResponse<List<HireRequestMessageDto>>.Ok(result));
    }

    [HttpPost("{id:int}/messages")]
    public async Task<IActionResult> SendMessage(int id, [FromBody] SendMessageDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var fullName = User.FindFirstValue("fullName") ?? User.FindFirstValue(ClaimTypes.Name) ?? "Unknown";
        var picture = User.FindFirstValue("profilePicture");

        var (success, message, data) = await _service.SendMessageAsync(GetUserId(), GetRole(), fullName, picture, id, dto);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse<HireRequestMessageDto>.Ok(data!, message));
    }

    [HttpPut("{id:int}/messages/read")]
    public async Task<IActionResult> MarkMessagesRead(int id)
    {
        var (success, message) = await _service.MarkMessagesReadAsync(GetUserId(), id);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    // ── Proposals (on Deal) ──────────────────────────────────────────

    [HttpPost("deals/{dealId:int}/proposals")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> CreateProposal(int dealId, [FromBody] CreateProposalDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
        var (success, message, data) = await _service.CreateProposalAsync(GetUserId(), dealId, dto);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse<ProposalDto>.Ok(data!, message));
    }

    [HttpPut("proposals/{proposalId:int}/accept")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> AcceptProposal(int proposalId, [FromBody] RespondToProposalDto? dto)
    {
        var (success, message) = await _service.AcceptProposalAsync(GetUserId(), proposalId, dto?.Note);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    [HttpPut("proposals/{proposalId:int}/reject")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> RejectProposal(int proposalId, [FromBody] RespondToProposalDto? dto)
    {
        var (success, message) = await _service.RejectProposalAsync(GetUserId(), proposalId, dto?.Note);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    // ── Invoices (on Deal) ───────────────────────────────────────────

    /// <summary>
    /// Create a partial/milestone invoice for a deal. Multiple allowed;
    /// total must not exceed accepted proposal amount.
    /// </summary>
    [HttpPost("deals/{dealId:int}/invoices")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> GenerateInvoice(int dealId, [FromBody] CreateInvoiceDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
        var (success, message, data) = await _service.GenerateInvoiceAsync(GetUserId(), dealId, dto);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse<InvoiceDto>.Ok(data!, message));
    }

    [HttpPut("invoices/{invoiceId:int}/accept")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> AcceptInvoice(int invoiceId)
    {
        var (success, message) = await _service.AcceptInvoiceAsync(GetUserId(), invoiceId);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    [HttpPut("invoices/{invoiceId:int}/paid")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> MarkInvoicePaid(int invoiceId)
    {
        var (success, message) = await _service.MarkInvoicePaidAsync(GetUserId(), invoiceId);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    [HttpPut("invoices/{invoiceId:int}/reject")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> RejectInvoice(int invoiceId)
    {
        var (success, message) = await _service.RejectInvoiceAsync(GetUserId(), invoiceId);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    // ── Invoice Settings (Lawyer only) ───────────────────────────────

    [HttpGet("invoice-settings")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> GetInvoiceSettings()
    {
        var result = await _service.GetInvoiceSettingsAsync(GetUserId());
        return Ok(ApiResponse<LawyerInvoiceSettingsDto>.Ok(result ?? new LawyerInvoiceSettingsDto()));
    }

    [HttpPut("invoice-settings")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> UpsertInvoiceSettings([FromBody] UpsertLawyerInvoiceSettingsDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
        var (success, message, data) = await _service.UpsertInvoiceSettingsAsync(GetUserId(), dto);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse<LawyerInvoiceSettingsDto>.Ok(data!, message));
    }

    // ── Hire Request Documents ───────────────────────────────────────

    [HttpGet("{id:int}/documents")]
    public async Task<IActionResult> GetDocuments(int id)
    {
        var result = await _docs.GetDocumentsAsync(GetUserId(), GetRole(), id);
        return Ok(ApiResponse<List<HireRequestDocumentDto>>.Ok(result));
    }

    [HttpPost("{id:int}/documents")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> UploadDocument(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse.Fail("No file provided."));
        var (success, message, data) = await _docs.UploadDocumentAsync(GetUserId(), id, file);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse<HireRequestDocumentDto>.Ok(data!, message));
    }

    [HttpGet("{id:int}/documents/{docId:int}/download")]
    public async Task<IActionResult> DownloadDocument(int id, int docId)
    {
        var (doc, canAccess) = await _docs.GetDocumentForDownloadAsync(GetUserId(), GetRole(), docId);
        if (doc == null || !canAccess || doc.HireRequestId != id)
            return NotFound(ApiResponse.Fail("Document not found or access denied."));

        var stream = ((IFileUploadService)HttpContext.RequestServices
            .GetRequiredService(typeof(IFileUploadService)))
            .GetHireRequestDocumentStream(doc.FilePath);
        if (stream == null) return NotFound(ApiResponse.Fail("File not found on server."));

        return File(stream, doc.ContentType, doc.FileName);
    }

    [HttpDelete("{id:int}/documents/{docId:int}")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> DeleteDocument(int id, int docId)
    {
        // Verify the doc belongs to this hire request before deleting
        var (doc, _) = await _docs.GetDocumentForDownloadAsync(GetUserId(), GetRole(), docId);
        if (doc == null || doc.HireRequestId != id)
            return NotFound(ApiResponse.Fail("Document not found."));

        var (success, message) = await _docs.DeleteDocumentAsync(GetUserId(), docId);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    /// <summary>
    /// Download a case document that is marked IsAvailableForDeal, via hire-request context.
    /// Accessible by the assigned lawyer (pre-hire) and the client who owns the hire request.
    /// </summary>
    [HttpGet("{id:int}/case-documents/{caseDocId:int}/download")]
    public async Task<IActionResult> DownloadCaseDocumentViaHireRequest(int id, int caseDocId)
    {
        var (doc, canAccess) = await _caseDocs.GetDealDocumentViaHireRequestAsync(GetUserId(), GetRole(), id, caseDocId);
        if (doc == null || !canAccess)
            return NotFound(ApiResponse.Fail("Document not found or access denied."));

        var stream = HttpContext.RequestServices
            .GetRequiredService<IFileUploadService>()
            .GetFileStream(doc.FilePath);
        if (stream == null) return NotFound(ApiResponse.Fail("File not found on server."));

        return File(stream, doc.ContentType, doc.FileName);
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException());

    private string GetRole() =>
        User.FindFirstValue(ClaimTypes.Role) ?? "Client";
}
