using System.Security.Claims;
using LegalConnect.API.DTOs.Cases;
using LegalConnect.API.Helpers;
using LegalConnect.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LegalConnect.API.Controllers;

[ApiController]
[Route("api/cases/{caseId:int}/activities")]
[Authorize(Roles = "Lawyer,Client,Admin")]
public class CaseActivitiesController : ControllerBase
{
    private readonly ICaseActivityService _service;

    public CaseActivitiesController(ICaseActivityService service)
    {
        _service = service;
    }

    /// <summary>Get all timeline activities for a case.</summary>
    [HttpGet]
    public async Task<IActionResult> GetActivities(int caseId)
    {
        var activities = await _service.GetActivitiesAsync(GetUserId(), GetUserRole(), caseId);
        return Ok(ApiResponse<List<CaseActivityDto>>.Ok(activities));
    }

    /// <summary>Add a new activity/note to the case timeline.</summary>
    [HttpPost]
    public async Task<IActionResult> AddActivity(int caseId, [FromBody] AddCaseActivityDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var (success, message, data) = await _service.AddActivityAsync(
            GetUserId(), GetUserRole(), GetUserName(), caseId, dto);

        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse<CaseActivityDto>.Ok(data!, message));
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException());

    private string GetUserRole() =>
        User.FindFirstValue(ClaimTypes.Role)
        ?? User.FindFirstValue("role")
        ?? "Client";

    private string GetUserName() =>
        User.FindFirstValue(ClaimTypes.Name)
        ?? User.FindFirstValue("fullName")
        ?? "Unknown";
}
