using System.Security.Claims;
using LegalConnect.API.DTOs.Support;
using LegalConnect.API.Entities;
using LegalConnect.API.Helpers;
using LegalConnect.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LegalConnect.API.Controllers;

[ApiController]
[Route("api/support")]
[Authorize]
public class SupportController : ControllerBase
{
    private readonly ISupportService _service;

    public SupportController(ISupportService service) => _service = service;

    // ── Create Ticket (Lawyer, Client) ──────────────────────────────────────

    [HttpPost]
    [Authorize(Roles = "Lawyer,Client")]
    public async Task<IActionResult> CreateTicket([FromBody] CreateSupportTicketDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed."));

        var userName = GetUserName();
        var (success, message, data) = await _service.CreateTicketAsync(GetUserId(), GetRole(), userName, dto);

        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse<SupportTicketDto>.Ok(data!, message));
    }

    // ── My Tickets (Lawyer, Client) ─────────────────────────────────────────

    [HttpGet("my")]
    [Authorize(Roles = "Lawyer,Client")]
    public async Task<IActionResult> GetMyTickets()
    {
        var result = await _service.GetMyTicketsAsync(GetUserId());
        return Ok(ApiResponse<List<SupportTicketDto>>.Ok(result));
    }

    // ── All Tickets (Admin) ─────────────────────────────────────────────────

    [HttpGet("all")]
    [Authorize(Roles = "Admin,AdminStaff")]
    [RequireAdminStaffRole(AdminStaffRole.SupportStaff)]
    public async Task<IActionResult> GetAllTickets([FromQuery] string? status, [FromQuery] string? category)
    {
        var result = await _service.GetAllTicketsAsync(status, category);
        return Ok(ApiResponse<List<SupportTicketDto>>.Ok(result));
    }

    // ── Ticket Detail (All roles) ───────────────────────────────────────────

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Lawyer,Client,Admin,AdminStaff")]
    public async Task<IActionResult> GetTicketById(int id)
    {
        var result = await _service.GetTicketByIdAsync(GetUserId(), GetRole(), id);
        if (result == null) return NotFound(ApiResponse.Fail("Ticket not found or access denied."));
        return Ok(ApiResponse<SupportTicketDto>.Ok(result));
    }

    // ── Update Status (Admin) ───────────────────────────────────────────────

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "Admin,AdminStaff")]
    [RequireAdminStaffRole(AdminStaffRole.SupportStaff)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateTicketStatusDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed."));

        var (success, message) = await _service.UpdateTicketStatusAsync(GetUserId(), GetUserName(), id, dto);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    // ── Get Messages ────────────────────────────────────────────────────────

    [HttpGet("{id:int}/messages")]
    [Authorize(Roles = "Lawyer,Client,Admin,AdminStaff")]
    public async Task<IActionResult> GetMessages(int id)
    {
        var result = await _service.GetMessagesAsync(GetUserId(), GetRole(), id);
        return Ok(ApiResponse<List<SupportMessageDto>>.Ok(result));
    }

    // ── Send Message ────────────────────────────────────────────────────────

    [HttpPost("{id:int}/messages")]
    [Authorize(Roles = "Lawyer,Client,Admin,AdminStaff")]
    public async Task<IActionResult> SendMessage(int id, [FromBody] SendSupportMessageDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed."));

        var (success, message, data) = await _service.SendMessageAsync(GetUserId(), GetRole(), GetUserName(), id, dto);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse<SupportMessageDto>.Ok(data!, message));
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private int GetUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException());

    private string GetRole()
        => User.FindFirstValue(ClaimTypes.Role) ?? "Client";

    private string GetUserName()
        => User.FindFirstValue(ClaimTypes.Name) ?? "Unknown";
}
