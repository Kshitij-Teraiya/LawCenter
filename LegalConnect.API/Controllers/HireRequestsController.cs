using System.Security.Claims;
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
    private readonly IDealService _service;

    public HireRequestsController(IDealService service)
    {
        _service = service;
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

    [HttpPost("proposals/{proposalId:int}/invoice")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> GenerateInvoice(int proposalId, [FromBody] CreateInvoiceDto dto)
    {
        var (success, message, data) = await _service.GenerateInvoiceAsync(GetUserId(), proposalId, dto);
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

    // ── Helpers ──────────────────────────────────────────────────────

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException());

    private string GetRole() =>
        User.FindFirstValue(ClaimTypes.Role) ?? "Client";
}
