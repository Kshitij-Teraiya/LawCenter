using System.Security.Claims;
using LegalConnect.API.DTOs.Admin;
using LegalConnect.API.Helpers;
using LegalConnect.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LegalConnect.API.Controllers;

[ApiController]
[Route("api/admin/staff")]
[Authorize(Roles = "Admin")]
public class AdminStaffController : ControllerBase
{
    private readonly IAdminStaffService _service;

    public AdminStaffController(IAdminStaffService service) => _service = service;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAdminStaffDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed."));

        var (success, message, data) = await _service.CreateAsync(GetUserId(), dto);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse<AdminStaffDto>.Ok(data!, message));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(ApiResponse<List<AdminStaffDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound(ApiResponse.Fail("Admin staff not found."));
        return Ok(ApiResponse<AdminStaffDto>.Ok(result));
    }

    [HttpPut("{id:int}/roles")]
    public async Task<IActionResult> UpdateRoles(int id, [FromBody] UpdateAdminStaffRolesDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed."));

        var (success, message) = await _service.UpdateRolesAsync(id, GetUserId(), dto);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    [HttpPut("{id:int}/toggle-active")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var (success, message) = await _service.ToggleActiveAsync(id);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    [HttpPut("{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetAdminStaffPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed."));

        var (success, message) = await _service.ResetPasswordAsync(id, dto.NewPassword);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    [HttpGet("roles")]
    public IActionResult GetAvailableRoles()
    {
        var roles = _service.GetAvailableRoles();
        return Ok(ApiResponse<List<AdminStaffRoleInfoDto>>.Ok(roles));
    }

    private int GetUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException());
}
