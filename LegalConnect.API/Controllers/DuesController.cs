using LegalConnect.API.DTOs.Dues;
using LegalConnect.API.Entities;
using LegalConnect.API.Helpers;
using LegalConnect.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LegalConnect.API.Controllers;

[ApiController]
[Route("api/dues")]
[Authorize]
public class DuesController : ControllerBase
{
    private readonly IDuesService _duesService;

    public DuesController(IDuesService duesService)
    {
        _duesService = duesService;
    }

    private int    CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string CurrentRole   => User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    /// <summary>GET api/dues/my — Lawyer's own dues summary.</summary>
    [HttpGet("my")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> GetMy()
    {
        var summary = await _duesService.GetMyDuesAsync(CurrentUserId);
        return Ok(ApiResponse<DuesSummaryDto>.Ok(summary));
    }

    /// <summary>GET api/dues/lawyer/{lawyerProfileId} — Admin views a lawyer's dues.</summary>
    [HttpGet("lawyer/{lawyerProfileId:int}")]
    [Authorize(Roles = "Admin,AdminStaff")]
    [RequireAdminStaffRole(AdminStaffRole.FinanceStaff)]
    public async Task<IActionResult> GetLawyerDues(int lawyerProfileId)
    {
        var summary = await _duesService.GetLawyerDuesAsync(lawyerProfileId);
        if (summary == null) return NotFound(ApiResponse<string>.Fail("Lawyer not found."));
        return Ok(ApiResponse<LawyerDuesSummaryDto>.Ok(summary));
    }

    /// <summary>GET api/dues/all — Admin: all lawyers with dues (paged).</summary>
    [HttpGet("all")]
    [Authorize(Roles = "Admin,AdminStaff")]
    [RequireAdminStaffRole(AdminStaffRole.FinanceStaff)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _duesService.GetAllLawyerDuesAsync(page, pageSize);
        return Ok(ApiResponse<PagedResult<LawyerDuesSummaryDto>>.Ok(result));
    }
}
