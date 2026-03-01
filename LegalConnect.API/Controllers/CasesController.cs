using System.Security.Claims;
using LegalConnect.API.DTOs.Cases;
using LegalConnect.API.Entities;
using LegalConnect.API.Helpers;
using LegalConnect.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LegalConnect.API.Controllers;

[ApiController]
[Route("api/cases")]
[Authorize(Roles = "Lawyer,Client,Admin")]
public class CasesController : ControllerBase
{
    private readonly ICaseService _caseService;

    public CasesController(ICaseService caseService)
    {
        _caseService = caseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCases([FromQuery] CaseFilterDto filter)
    {
        var result = await _caseService.GetCasesAsync(GetUserId(), GetUserRole(), filter);
        return Ok(ApiResponse<PagedResult<CaseSummaryDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCaseById(int id)
    {
        var result = await _caseService.GetCaseByIdAsync(GetUserId(), GetUserRole(), id);
        if (result == null) return NotFound(ApiResponse.Fail("Case not found."));
        return Ok(ApiResponse<CaseDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Lawyer,Client")]
    public async Task<IActionResult> CreateCase([FromBody] CreateCaseDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
        var (success, message, data) = await _caseService.CreateCaseAsync(GetUserId(), GetUserRole(), dto);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse<CaseDto>.Ok(data!, message));
    }

    [HttpGet("my-lawyers")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetMyLawyers()
    {
        var result = await _caseService.GetMyLawyersAsync(GetUserId());
        return Ok(ApiResponse<List<HiredLawyerDto>>.Ok(result));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> UpdateCase(int id, [FromBody] UpdateCaseDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
        var (success, message) = await _caseService.UpdateCaseAsync(GetUserId(), id, dto);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
    {
        var (success, message) = await _caseService.UpdateStatusAsync(GetUserId(), id, dto.Status);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    [HttpPut("{id:int}/close")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> CloseCase(int id, [FromBody] CloseCaseDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse.Fail("Validation failed"));
        var (success, message) = await _caseService.CloseCaseAsync(GetUserId(), id, dto);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> DeleteCase(int id)
    {
        var (success, message) = await _caseService.SoftDeleteAsync(GetUserId(), id);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    [HttpGet("{id:int}/lawyers")]
    public async Task<IActionResult> GetCaseLawyers(int id)
    {
        var result = await _caseService.GetCaseLawyersAsync(GetUserId(), GetUserRole(), id);
        return Ok(ApiResponse<List<CaseLawyerDto>>.Ok(result));
    }

    [HttpPost("{id:int}/lawyers")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> AddLawyerToCase(int id, [FromBody] AddCaseLawyerDto dto)
    {
        var (success, message) = await _caseService.AddLawyerToCaseAsync(GetUserId(), id, dto.LawyerProfileId);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    [HttpDelete("{id:int}/lawyers/{lawyerProfileId:int}")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> RemoveLawyerFromCase(int id, int lawyerProfileId)
    {
        var (success, message) = await _caseService.RemoveLawyerFromCaseAsync(GetUserId(), id, lawyerProfileId);
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
}

public class UpdateStatusDto { public CaseStatus Status { get; set; } }
public class AddCaseLawyerDto { public int LawyerProfileId { get; set; } }
