using LegalConnect.API.DTOs.Auth;
using LegalConnect.API.Helpers;
using LegalConnect.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace LegalConnect.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var (success, message, data) = await _authService.LoginAsync(dto);
        if (!success)
            return Unauthorized(ApiResponse.Fail(message));

        return Ok(ApiResponse<AuthResponseDto>.Ok(data!, message));
    }

    [HttpPost("register/client")]
    public async Task<IActionResult> RegisterClient([FromBody] RegisterClientDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var (success, message, data) = await _authService.RegisterClientAsync(dto);
        if (!success)
            return BadRequest(ApiResponse.Fail(message));

        return Ok(ApiResponse<AuthResponseDto>.Ok(data!, message));
    }

    [HttpPost("register/lawyer")]
    public async Task<IActionResult> RegisterLawyer([FromBody] RegisterLawyerDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var (success, message) = await _authService.RegisterLawyerAsync(dto);
        if (!success)
            return BadRequest(ApiResponse.Fail(message));

        return Ok(ApiResponse.Ok(message));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed"));

        var (success, message) = await _authService.ForgotPasswordAsync(dto);
        return Ok(ApiResponse.Ok(message));
    }

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed"));

        var (success, message, data) = await _authService.GoogleLoginAsync(dto.Credential);
        if (!success)
            return BadRequest(ApiResponse.Fail(message));

        return Ok(ApiResponse<AuthResponseDto>.Ok(data!, message));
    }
}
