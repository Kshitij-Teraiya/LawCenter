using LegalConnect.API.DTOs.Dues;
using LegalConnect.API.Entities;
using LegalConnect.API.Helpers;
using LegalConnect.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LegalConnect.API.Controllers;

[ApiController]
[Route("api/litigation-disputes")]
[Authorize]
public class LitigationDisputesController : ControllerBase
{
    private readonly ILitigationDisputeService _disputeService;

    public LitigationDisputesController(ILitigationDisputeService disputeService)
    {
        _disputeService = disputeService;
    }

    private int    CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string CurrentRole   => User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    /// <summary>GET api/litigation-disputes — Paged list.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DisputeFilterDto filter)
    {
        var result = await _disputeService.GetDisputesAsync(CurrentUserId, CurrentRole, filter);
        return Ok(ApiResponse<PagedResult<LitigationDisputeDto>>.Ok(result));
    }

    /// <summary>GET api/litigation-disputes/{id} — Single dispute.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await _disputeService.GetByIdAsync(CurrentUserId, CurrentRole, id);
        if (dto == null) return NotFound(ApiResponse<string>.Fail("Dispute not found."));
        return Ok(ApiResponse<LitigationDisputeDto>.Ok(dto));
    }

    /// <summary>POST api/litigation-disputes — Client raises dispute.</summary>
    [HttpPost]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> Raise([FromBody] RaiseDisputeDto dto)
    {
        var (success, message, data) = await _disputeService.RaiseDisputeAsync(CurrentUserId, dto);
        if (!success) return BadRequest(ApiResponse<string>.Fail(message));
        return Ok(ApiResponse<LitigationDisputeDto>.Ok(data!, message));
    }

    /// <summary>PUT api/litigation-disputes/{id}/admin-approve — Admin approves or rejects.</summary>
    [HttpPut("{id:int}/admin-approve")]
    [Authorize(Roles = "Admin,AdminStaff")]
    [RequireAdminStaffRole(AdminStaffRole.DisputeRefundManager)]
    public async Task<IActionResult> AdminApprove(int id, [FromBody] ApproveDisputeDto dto)
    {
        var (success, message) = await _disputeService.AdminApproveAsync(CurrentUserId, id, dto.Approve);
        if (!success) return BadRequest(ApiResponse<string>.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    /// <summary>PUT api/litigation-disputes/{id}/lawyer-approve — Lawyer approves or rejects.</summary>
    [HttpPut("{id:int}/lawyer-approve")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> LawyerApprove(int id, [FromBody] ApproveDisputeDto dto)
    {
        var (success, message) = await _disputeService.LawyerApproveAsync(CurrentUserId, id, dto.Approve);
        if (!success) return BadRequest(ApiResponse<string>.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }
}
