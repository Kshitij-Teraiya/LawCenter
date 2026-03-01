using System.Security.Claims;
using LegalConnect.API.DTOs.Admin;
using LegalConnect.API.DTOs.Lawyer;
using LegalConnect.API.Helpers;
using LegalConnect.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LegalConnect.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("lawyers/pending")]
    public async Task<IActionResult> GetPendingLawyers()
    {
        var lawyers = await _adminService.GetPendingLawyersAsync();
        return Ok(ApiResponse<List<PendingLawyerDto>>.Ok(lawyers));
    }

    [HttpPut("lawyers/{lawyerId:int}/approve")]
    public async Task<IActionResult> ApproveLawyer(int lawyerId)
    {
        var (success, message) = await _adminService.ApproveLawyerAsync(lawyerId);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    [HttpPut("lawyers/{lawyerId:int}/reject")]
    public async Task<IActionResult> RejectLawyer(int lawyerId, [FromBody] RejectLawyerDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed"));

        var (success, message) = await _adminService.RejectLawyerAsync(lawyerId, dto.Reason);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    [HttpGet("commission")]
    public async Task<IActionResult> GetCommission()
    {
        var setting = await _adminService.GetCommissionSettingAsync();
        return Ok(ApiResponse<CommissionSettingDto>.Ok(setting));
    }

    [HttpPut("commission")]
    public async Task<IActionResult> SetCommission([FromBody] SetCommissionDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed"));

        var adminName = User.FindFirstValue(ClaimTypes.Name) ?? "Admin";
        var (success, message) = await _adminService.SetCommissionAsync(dto, adminName);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _adminService.GetAllCategoriesAsync();
        return Ok(ApiResponse<List<CategoryDto>>.Ok(categories));
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed"));

        var (success, message) = await _adminService.CreateCategoryAsync(dto);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    [HttpPut("categories/{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed"));

        dto.Id = id;
        var (success, message) = await _adminService.UpdateCategoryAsync(dto);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    [HttpDelete("categories/{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var (success, message) = await _adminService.DeleteCategoryAsync(id);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenue()
    {
        var stats = await _adminService.GetRevenueStatsAsync();
        return Ok(ApiResponse<RevenueStatsDto>.Ok(stats));
    }
}
