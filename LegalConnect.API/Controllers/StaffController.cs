using LegalConnect.API.DTOs.Staff;
using LegalConnect.API.Helpers;
using LegalConnect.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LegalConnect.API.Controllers;

[ApiController]
[Route("api/staff")]
[Authorize]
public class StaffController : ControllerBase
{
    private readonly IStaffService _staffService;

    public StaffController(IStaffService staffService)
    {
        _staffService = staffService;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ── Staff Management (Lawyer only) ───────────────────────────────────────

    /// <summary>GET api/staff/my-staff — List all staff belonging to this lawyer's firm.</summary>
    [HttpGet("my-staff")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> GetMyStaff()
    {
        var result = await _staffService.GetMyStaffAsync(CurrentUserId);
        return Ok(ApiResponse<List<StaffProfileDto>>.Ok(result));
    }

    /// <summary>POST api/staff/create — Create a new staff user account under this lawyer.</summary>
    [HttpPost("create")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> CreateStaff([FromBody] CreateStaffDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.Fail("Invalid data."));

        var (success, message, data) = await _staffService.CreateStaffAsync(CurrentUserId, dto);
        if (!success) return BadRequest(ApiResponse<string>.Fail(message));
        return Ok(ApiResponse<StaffProfileDto>.Ok(data!, message));
    }

    /// <summary>GET api/staff/{id} — Get a specific staff member's details.</summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> GetStaffById(int id)
    {
        var result = await _staffService.GetStaffByIdAsync(CurrentUserId, id);
        if (result == null) return NotFound(ApiResponse<string>.Fail("Staff member not found."));
        return Ok(ApiResponse<StaffProfileDto>.Ok(result));
    }

    /// <summary>PUT api/staff/{id}/toggle — Toggle a staff member's active/inactive status.</summary>
    [HttpPut("{id:int}/toggle")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> ToggleStaffActive(int id)
    {
        var (success, message) = await _staffService.ToggleStaffActiveAsync(CurrentUserId, id);
        if (!success) return BadRequest(ApiResponse<string>.Fail(message));
        return Ok(ApiResponse<string>.Ok(message, message));
    }

    // ── Case Staff Assignment (Lawyer + Staff) ───────────────────────────────

    /// <summary>GET api/staff/cases/{caseId}/staff — Get all staff assigned to a case.</summary>
    [HttpGet("cases/{caseId:int}/staff")]
    [Authorize(Roles = "Lawyer,Staff")]
    public async Task<IActionResult> GetCaseStaff(int caseId)
    {
        var result = await _staffService.GetCaseStaffAsync(caseId);
        return Ok(ApiResponse<List<CaseStaffDto>>.Ok(result));
    }

    /// <summary>POST api/staff/cases/{caseId}/assign — Assign a staff member to a case.</summary>
    [HttpPost("cases/{caseId:int}/assign")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> AssignStaffToCase(int caseId, [FromBody] AssignStaffDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.Fail("Invalid data."));

        var (success, message) = await _staffService.AssignStaffToCaseAsync(CurrentUserId, caseId, dto);
        if (!success) return BadRequest(ApiResponse<string>.Fail(message));
        return Ok(ApiResponse<string>.Ok(message, message));
    }

    /// <summary>DELETE api/staff/cases/{caseId}/staff/{staffId} — Remove a staff member from a case.</summary>
    [HttpDelete("cases/{caseId:int}/staff/{staffId:int}")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> RemoveStaffFromCase(int caseId, int staffId)
    {
        var (success, message) = await _staffService.RemoveStaffFromCaseAsync(CurrentUserId, caseId, staffId);
        if (!success) return BadRequest(ApiResponse<string>.Fail(message));
        return Ok(ApiResponse<string>.Ok(message, message));
    }

    /// <summary>GET api/staff/cases/{caseId}/my-permissions — Get the current staff member's permissions for a case.</summary>
    [HttpGet("cases/{caseId:int}/my-permissions")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> GetMyPermissions(int caseId)
    {
        var result = await _staffService.GetMyPermissionsAsync(CurrentUserId, caseId);
        if (result == null) return NotFound(ApiResponse<string>.Fail("No assignment found for this case."));
        return Ok(ApiResponse<StaffCasePermissionsDto>.Ok(result));
    }

    // ── Task Management (Lawyer) ─────────────────────────────────────────────

    /// <summary>GET api/staff/tasks — Get all tasks created by this lawyer.</summary>
    [HttpGet("tasks")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> GetAllTasks()
    {
        var result = await _staffService.GetAllTasksAsync(CurrentUserId);
        return Ok(ApiResponse<List<StaffTaskDto>>.Ok(result));
    }

    /// <summary>POST api/staff/tasks — Create a new task.</summary>
    [HttpPost("tasks")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> CreateTask([FromBody] CreateStaffTaskDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.Fail("Invalid data."));

        var (success, message, data) = await _staffService.CreateTaskAsync(CurrentUserId, dto);
        if (!success) return BadRequest(ApiResponse<string>.Fail(message));
        return Ok(ApiResponse<StaffTaskDto>.Ok(data!, message));
    }

    /// <summary>PUT api/staff/tasks/{id} — Update a task (full update by lawyer).</summary>
    [HttpPut("tasks/{id:int}")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateStaffTaskDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.Fail("Invalid data."));

        var (success, message, data) = await _staffService.UpdateTaskAsync(CurrentUserId, id, dto);
        if (!success) return BadRequest(ApiResponse<string>.Fail(message));
        return Ok(ApiResponse<StaffTaskDto>.Ok(data!, message));
    }

    /// <summary>DELETE api/staff/tasks/{id} — Soft-delete a task.</summary>
    [HttpDelete("tasks/{id:int}")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var (success, message) = await _staffService.DeleteTaskAsync(CurrentUserId, id);
        if (!success) return BadRequest(ApiResponse<string>.Fail(message));
        return Ok(ApiResponse<string>.Ok(message, message));
    }

    // ── Staff-facing endpoints ───────────────────────────────────────────────

    /// <summary>GET api/staff/my-tasks — Get tasks assigned to this staff member.</summary>
    [HttpGet("my-tasks")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> GetMyTasks()
    {
        var result = await _staffService.GetMyTasksAsync(CurrentUserId);
        return Ok(ApiResponse<List<StaffTaskDto>>.Ok(result));
    }

    /// <summary>PUT api/staff/my-tasks/{id} — Update task status and/or add a progress note.</summary>
    [HttpPut("my-tasks/{id:int}")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> UpdateMyTaskStatus(int id, [FromBody] UpdateTaskStatusDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.Fail("Invalid data."));

        var (success, message, data) = await _staffService.UpdateMyTaskStatusAsync(CurrentUserId, id, dto);
        if (!success) return BadRequest(ApiResponse<string>.Fail(message));
        return Ok(ApiResponse<StaffTaskDto>.Ok(data!, message));
    }
}
