using System.Security.Claims;
using LegalConnect.API.DTOs.TimeSlots;
using Microsoft.EntityFrameworkCore;
using LegalConnect.API.Helpers;
using LegalConnect.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LegalConnect.API.Controllers;

[ApiController]
[Route("api/availability")]
public class LawyerAvailabilityController : ControllerBase
{
    private readonly ILawyerTimeSlotConfigurationService _configService;
    private readonly ILawyerWorkingHoursService _workingHoursService;
    private readonly ILawyerBlackoutBlockService _blackoutService;
    private readonly IHolidayManagementService _holidayService;

    public LawyerAvailabilityController(
        ILawyerTimeSlotConfigurationService configService,
        ILawyerWorkingHoursService workingHoursService,
        ILawyerBlackoutBlockService blackoutService,
        IHolidayManagementService holidayService)
    {
        _configService = configService;
        _workingHoursService = workingHoursService;
        _blackoutService = blackoutService;
        _holidayService = holidayService;
    }

    // ── Session Duration & Buffer ─────────────────────────────────────────────

    [HttpGet("configuration")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> GetConfiguration()
    {
        var lawyerProfileId = await GetLawyerProfileIdAsync();
        if (lawyerProfileId == 0) return NotFound(ApiResponse.Fail("Lawyer profile not found."));

        var config = await _configService.GetConfigurationAsync(lawyerProfileId);
        if (config == null)
        {
            await _configService.CreateDefaultConfigurationAsync(lawyerProfileId);
            config = (await _configService.GetConfigurationAsync(lawyerProfileId))!;
        }
        return Ok(ApiResponse<TimeSlotConfigurationDto>.Ok(config));
    }

    [HttpPost("configuration")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> UpdateConfiguration([FromBody] UpdateTimeSlotConfigurationDto dto)
    {
        var lawyerProfileId = await GetLawyerProfileIdAsync();
        if (lawyerProfileId == 0) return NotFound(ApiResponse.Fail("Lawyer profile not found."));

        var (success, message) = await _configService.UpdateConfigurationAsync(lawyerProfileId, dto.SessionDurationMinutes, dto.BufferTimeMinutes);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    // ── Working Hours ─────────────────────────────────────────────────────────

    [HttpGet("working-hours")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> GetWorkingHours()
    {
        var lawyerProfileId = await GetLawyerProfileIdAsync();
        if (lawyerProfileId == 0) return NotFound(ApiResponse.Fail("Lawyer profile not found."));

        var hours = await _workingHoursService.GetWorkingHoursAsync(lawyerProfileId);
        return Ok(ApiResponse<List<WorkingHoursDto>>.Ok(hours));
    }

    [HttpPut("working-hours/{dayOfWeek:int}")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> UpdateWorkingHours(int dayOfWeek, [FromBody] UpdateWorkingHoursDto dto)
    {
        var lawyerProfileId = await GetLawyerProfileIdAsync();
        if (lawyerProfileId == 0) return NotFound(ApiResponse.Fail("Lawyer profile not found."));

        var (success, message) = await _workingHoursService.UpdateWorkingHoursAsync(
            lawyerProfileId, dayOfWeek, dto.StartTime, dto.EndTime, dto.IsWorking);

        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    // ── Blackout Blocks ───────────────────────────────────────────────────────

    [HttpGet("blackout-blocks")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> GetBlackoutBlocks()
    {
        var lawyerProfileId = await GetLawyerProfileIdAsync();
        if (lawyerProfileId == 0) return NotFound(ApiResponse.Fail("Lawyer profile not found."));

        var blocks = await _blackoutService.GetBlackoutBlocksAsync(lawyerProfileId);
        return Ok(ApiResponse<List<BlackoutBlockDto>>.Ok(blocks));
    }

    [HttpPost("blackout-blocks")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> CreateBlackoutBlock([FromBody] CreateBlackoutBlockDto dto)
    {
        var lawyerProfileId = await GetLawyerProfileIdAsync();
        if (lawyerProfileId == 0) return NotFound(ApiResponse.Fail("Lawyer profile not found."));

        var (success, message, id) = await _blackoutService.CreateBlackoutBlockAsync(lawyerProfileId, dto);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse<object>.Ok(new { id }, message));
    }

    [HttpDelete("blackout-blocks/{id:int}")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> DeleteBlackoutBlock(int id)
    {
        var lawyerProfileId = await GetLawyerProfileIdAsync();
        if (lawyerProfileId == 0) return NotFound(ApiResponse.Fail("Lawyer profile not found."));

        var (success, message) = await _blackoutService.DeleteBlackoutBlockAsync(lawyerProfileId, id);
        if (!success) return NotFound(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    // ── Personal Holidays ─────────────────────────────────────────────────────

    [HttpGet("holidays/personal")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> GetPersonalHolidays()
    {
        var lawyerProfileId = await GetLawyerProfileIdAsync();
        if (lawyerProfileId == 0) return NotFound(ApiResponse.Fail("Lawyer profile not found."));

        var holidays = await _holidayService.GetPersonalHolidaysAsync(lawyerProfileId);
        return Ok(ApiResponse<List<PersonalHolidayDto>>.Ok(holidays));
    }

    [HttpPost("holidays/personal")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> CreatePersonalHoliday([FromBody] CreatePersonalHolidayDto dto)
    {
        var lawyerProfileId = await GetLawyerProfileIdAsync();
        if (lawyerProfileId == 0) return NotFound(ApiResponse.Fail("Lawyer profile not found."));

        var (success, message, id) = await _holidayService.CreatePersonalHolidayAsync(lawyerProfileId, dto);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse<object>.Ok(new { id }, message));
    }

    [HttpDelete("holidays/personal/{id:int}")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> DeletePersonalHoliday(int id)
    {
        var lawyerProfileId = await GetLawyerProfileIdAsync();
        if (lawyerProfileId == 0) return NotFound(ApiResponse.Fail("Lawyer profile not found."));

        var (success, message) = await _holidayService.DeletePersonalHolidayAsync(lawyerProfileId, id);
        if (!success) return NotFound(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    // ── Holiday Preferences (Master Holidays) ─────────────────────────────────

    [HttpGet("holidays/preferences")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> GetHolidayPreferences()
    {
        var lawyerProfileId = await GetLawyerProfileIdAsync();
        if (lawyerProfileId == 0) return NotFound(ApiResponse.Fail("Lawyer profile not found."));

        var prefs = await _holidayService.GetLawyerHolidayPreferencesAsync(lawyerProfileId);
        return Ok(ApiResponse<List<HolidayPreferenceDto>>.Ok(prefs));
    }

    [HttpPut("holidays/preferences/{masterHolidayId:int}")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> SetHolidayPreference(int masterHolidayId, [FromBody] SetHolidayPreferenceDto dto)
    {
        var lawyerProfileId = await GetLawyerProfileIdAsync();
        if (lawyerProfileId == 0) return NotFound(ApiResponse.Fail("Lawyer profile not found."));

        var (success, message) = await _holidayService.SetHolidayPreferenceAsync(lawyerProfileId, masterHolidayId, dto.IsEnabled);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    // ── Admin: Master Holidays ────────────────────────────────────────────────

    [HttpGet("holidays/master")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetMasterHolidays()
    {
        var holidays = await _holidayService.GetMasterHolidaysAsync();
        return Ok(ApiResponse<List<MasterHolidayDto>>.Ok(holidays));
    }

    [HttpPost("holidays/master")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateMasterHoliday([FromBody] CreateMasterHolidayDto dto)
    {
        var (success, message, id) = await _holidayService.CreateMasterHolidayAsync(dto);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse<object>.Ok(new { id }, message));
    }

    [HttpPut("holidays/master/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateMasterHoliday(int id, [FromBody] UpdateMasterHolidayDto dto)
    {
        var (success, message) = await _holidayService.UpdateMasterHolidayAsync(id, dto);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    [HttpDelete("holidays/master/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteMasterHoliday(int id)
    {
        var (success, message) = await _holidayService.DeleteMasterHolidayAsync(id);
        if (!success) return NotFound(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<int> GetLawyerProfileIdAsync()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? "0");
        if (userId == 0) return 0;

        // Resolve via HttpContext RequestServices to avoid circular dependency
        var db = HttpContext.RequestServices.GetRequiredService<LegalConnect.API.Data.AppDbContext>();
        var profile = await db.LawyerProfiles
            .Where(l => l.UserId == userId)
            .Select(l => new { l.Id })
            .FirstOrDefaultAsync();

        return profile?.Id ?? 0;
    }
}
