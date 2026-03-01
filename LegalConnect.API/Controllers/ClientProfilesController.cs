using System.Security.Claims;
using LegalConnect.API.DTOs.Client;
using LegalConnect.API.Helpers;
using LegalConnect.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LegalConnect.API.Controllers;

[ApiController]
[Route("api/client-profiles")]
[Authorize(Roles = "Client")]
public class ClientProfilesController : ControllerBase
{
    private readonly IClientProfileService _service;

    public ClientProfilesController(IClientProfileService service)
    {
        _service = service;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var result = await _service.GetMyProfileAsync(GetUserId());
        if (result == null) return NotFound(ApiResponse.Fail("Profile not found."));
        return Ok(ApiResponse<ClientProfileDto>.Ok(result));
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateClientProfileDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var (success, message) = await _service.UpdateMyProfileAsync(GetUserId(), dto);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException());
}
