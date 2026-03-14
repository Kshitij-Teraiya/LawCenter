using LegalConnect.API.DTOs.Dues;
using LegalConnect.API.Entities;
using LegalConnect.API.Helpers;
using LegalConnect.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LegalConnect.API.Controllers;

[ApiController]
[Route("api/refund-invoices")]
[Authorize]
public class RefundInvoicesController : ControllerBase
{
    private readonly IRefundInvoiceService _refundService;

    public RefundInvoicesController(IRefundInvoiceService refundService)
    {
        _refundService = refundService;
    }

    private int    CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string CurrentRole   => User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    private string CurrentName   => $"{User.FindFirstValue(ClaimTypes.GivenName)} {User.FindFirstValue(ClaimTypes.Surname)}".Trim();

    /// <summary>GET api/refund-invoices — Admin: all refund invoices (paged).</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,AdminStaff")]
    [RequireAdminStaffRole(AdminStaffRole.DisputeRefundManager)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _refundService.GetAllAsync(page, pageSize);
        return Ok(ApiResponse<PagedResult<RefundInvoiceDto>>.Ok(result));
    }

    /// <summary>GET api/refund-invoices/my — Lawyer: own refund invoices.</summary>
    [HttpGet("my")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> GetMy([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _refundService.GetForLawyerAsync(CurrentUserId, page, pageSize);
        return Ok(ApiResponse<PagedResult<RefundInvoiceDto>>.Ok(result));
    }

    /// <summary>GET api/refund-invoices/{id} — Get single.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await _refundService.GetByIdAsync(id);
        if (dto == null) return NotFound(ApiResponse<string>.Fail("Refund invoice not found."));
        return Ok(ApiResponse<RefundInvoiceDto>.Ok(dto));
    }

    /// <summary>POST api/refund-invoices — Admin creates refund invoice for a lawyer.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,AdminStaff")]
    [RequireAdminStaffRole(AdminStaffRole.DisputeRefundManager)]
    public async Task<IActionResult> Create([FromBody] CreateRefundInvoiceDto dto)
    {
        var adminName = User.FindFirstValue("FullName") ?? "Admin";
        var (success, message, data) = await _refundService.CreateAsync(CurrentUserId, adminName, dto);
        if (!success) return BadRequest(ApiResponse<string>.Fail(message));
        return Ok(ApiResponse<RefundInvoiceDto>.Ok(data!, message));
    }

    /// <summary>GET api/refund-invoices/{id}/download — Download PDF.</summary>
    [HttpGet("{id:int}/download")]
    public async Task<IActionResult> Download(int id)
    {
        var (stream, fileName) = await _refundService.DownloadAsync(id);
        if (stream == null) return NotFound(ApiResponse<string>.Fail("PDF not found."));
        return File(stream, "application/pdf", fileName);
    }
}
