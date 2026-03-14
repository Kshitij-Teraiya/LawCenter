using LegalConnect.API.DTOs.Dues;
using LegalConnect.API.Helpers;
using LegalConnect.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LegalConnect.API.Controllers;

[ApiController]
[Route("api/system-settings")]
public class SystemSettingsController : ControllerBase
{
    private readonly ISystemSettingsService _settingsService;

    public SystemSettingsController(ISystemSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>GET api/system-settings — Get all settings (Admin only).</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var settings = await _settingsService.GetAllAsync();
        return Ok(ApiResponse<List<SystemSettingDto>>.Ok(settings));
    }

    /// <summary>GET api/system-settings/{key} — Get single setting value (public, used on registration page).</summary>
    [HttpGet("{key}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByKey(string key)
    {
        var value = await _settingsService.GetValueAsync(key);
        if (value == null)
            return NotFound(ApiResponse<string>.Fail($"Setting '{key}' not found."));

        return Ok(ApiResponse<SystemSettingDto>.Ok(new SystemSettingDto { Key = key, Value = value }));
    }

    /// <summary>PUT api/system-settings/{key} — Upsert a setting (Admin only).</summary>
    [HttpPut("{key}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Upsert(string key, [FromBody] UpdateSystemSettingDto dto)
    {
        await _settingsService.UpsertAsync(key, dto.Value, CurrentUserId);
        return Ok(ApiResponse.Ok($"Setting '{key}' updated successfully."));
    }
}
