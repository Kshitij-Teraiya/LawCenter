using System.Security.Claims;
using LegalConnect.API.DTOs.Cases;
using LegalConnect.API.Helpers;
using LegalConnect.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LegalConnect.API.Controllers;

[ApiController]
[Route("api/lawyer-requests")]
[Authorize(Roles = "Lawyer,Client")]
public class ClientLawyerRequestsController : ControllerBase
{
    private readonly IClientLawyerRequestService _service;

    public ClientLawyerRequestsController(IClientLawyerRequestService service)
    {
        _service = service;
    }

    [HttpPost]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> SendRequest([FromBody] CreateClientLawyerRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
        var (success, message, data) = await _service.SendRequestAsync(GetUserId(), dto);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse<ClientLawyerRequestDto>.Ok(data!, message));
    }

    [HttpGet("my")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetMyRequests()
    {
        var result = await _service.GetMyRequestsAsync(GetUserId());
        return Ok(ApiResponse<List<ClientLawyerRequestDto>>.Ok(result));
    }

    [HttpGet("incoming")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> GetIncomingRequests()
    {
        var result = await _service.GetIncomingRequestsAsync(GetUserId());
        return Ok(ApiResponse<List<ClientLawyerRequestDto>>.Ok(result));
    }

    [HttpPut("{id:int}/accept")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> AcceptRequest(int id, [FromBody] RespondToRequestDto dto)
    {
        var (success, message) = await _service.AcceptRequestAsync(GetUserId(), id, dto.Note);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    [HttpPut("{id:int}/reject")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> RejectRequest(int id, [FromBody] RespondToRequestDto dto)
    {
        var (success, message) = await _service.RejectRequestAsync(GetUserId(), id, dto.Note);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> CancelRequest(int id)
    {
        var (success, message) = await _service.CancelRequestAsync(GetUserId(), id);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException());
}
