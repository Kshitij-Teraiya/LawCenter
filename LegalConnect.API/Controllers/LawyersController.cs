using System.Security.Claims;
using LegalConnect.API.DTOs.Lawyer;
using LegalConnect.API.Helpers;
using LegalConnect.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LegalConnect.API.Controllers;

[ApiController]
[Route("api/lawyers")]
public class LawyersController : ControllerBase
{
    private readonly ILawyerService _lawyerService;

    public LawyersController(ILawyerService lawyerService)
    {
        _lawyerService = lawyerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLawyers([FromQuery] LawyerFilterDto filter)
    {
        var result = await _lawyerService.GetLawyersAsync(filter);
        return Ok(ApiResponse<PagedResult<LawyerSummaryDto>>.Ok(result));
    }

    [HttpGet("cities")]
    public async Task<IActionResult> GetCities()
    {
        var cities = await _lawyerService.GetCitiesAsync();
        return Ok(ApiResponse<List<string>>.Ok(cities));
    }

    [HttpGet("courts")]
    public async Task<IActionResult> GetCourts()
    {
        var courts = await _lawyerService.GetCourtsAsync();
        return Ok(ApiResponse<List<string>>.Ok(courts));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetLawyerById(int id)
    {
        var lawyer = await _lawyerService.GetLawyerByIdAsync(id);
        if (lawyer == null)
            return NotFound(ApiResponse.Fail("Lawyer not found."));

        return Ok(ApiResponse<LawyerDto>.Ok(lawyer));
    }

    [HttpGet("me")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetUserId();
        var profile = await _lawyerService.GetMyProfileAsync(userId);
        if (profile == null)
            return NotFound(ApiResponse.Fail("Profile not found."));

        return Ok(ApiResponse<LawyerDto>.Ok(profile));
    }

    [HttpPut("me")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateLawyerProfileDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed"));

        var (success, message) = await _lawyerService.UpdateMyProfileAsync(GetUserId(), dto);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    [HttpPost("me/experiences")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> AddExperience([FromBody] AddExperienceDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed"));

        var (success, message) = await _lawyerService.AddExperienceAsync(GetUserId(), dto);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    [HttpDelete("me/experiences/{experienceId:int}")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> DeleteExperience(int experienceId)
    {
        var (success, message) = await _lawyerService.DeleteExperienceAsync(GetUserId(), experienceId);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    [HttpPost("me/case-results")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> AddCaseResult([FromBody] AddCaseResultDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed"));

        var (success, message) = await _lawyerService.AddCaseResultAsync(GetUserId(), dto);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    [HttpDelete("me/case-results/{caseResultId:int}")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> DeleteCaseResult(int caseResultId)
    {
        var (success, message) = await _lawyerService.DeleteCaseResultAsync(GetUserId(), caseResultId);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    [HttpPut("me/service-charge")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> SetServiceCharge([FromBody] SetServiceChargeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed"));

        var (success, message) = await _lawyerService.SetServiceChargeAsync(GetUserId(), dto);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException());
}
