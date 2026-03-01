using System.Security.Claims;
using LegalConnect.API.DTOs.Cases;
using LegalConnect.API.Helpers;
using LegalConnect.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LegalConnect.API.Controllers;

[ApiController]
[Route("api/lawyer-clients")]
[Authorize(Roles = "Lawyer")]
public class LawyerClientsController : ControllerBase
{
    private readonly ILawyerClientService _service;

    public LawyerClientsController(ILawyerClientService service)
    {
        _service = service;
    }

    /// <summary>Get all my clients with optional search/filter/pagination.</summary>
    [HttpGet]
    public async Task<IActionResult> GetMyClients([FromQuery] LawyerClientFilterDto filter)
    {
        var result = await _service.GetMyClientsAsync(GetUserId(), filter);
        return Ok(ApiResponse<PagedResult<LawyerClientDto>>.Ok(result));
    }

    /// <summary>Get eligible clients (completed appointments not yet added).</summary>
    [HttpGet("eligible")]
    public async Task<IActionResult> GetEligibleClients()
    {
        var result = await _service.GetEligibleClientsAsync(GetUserId());
        return Ok(ApiResponse<List<EligibleClientDto>>.Ok(result));
    }

    /// <summary>Get a specific client relationship by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetClientById(int id)
    {
        var result = await _service.GetClientByIdAsync(GetUserId(), id);
        if (result == null)
            return NotFound(ApiResponse.Fail("Client not found."));
        return Ok(ApiResponse<LawyerClientDto>.Ok(result));
    }

    /// <summary>Add a client to My Clients (from completed appointment).</summary>
    [HttpPost]
    public async Task<IActionResult> AddClient([FromBody] AddLawyerClientDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var (success, message, data) = await _service.AddClientAsync(GetUserId(), dto);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse<LawyerClientDto>.Ok(data!, message));
    }

    /// <summary>Update notes or active status for a client relationship.</summary>
    [HttpPut("{id:int}/notes")]
    public async Task<IActionResult> UpdateNotes(int id, [FromBody] UpdateLawyerClientNotesDto dto)
    {
        var (success, message) = await _service.UpdateNotesAsync(GetUserId(), id, dto);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    /// <summary>Remove (soft delete) a client from My Clients.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> RemoveClient(int id)
    {
        var (success, message) = await _service.RemoveClientAsync(GetUserId(), id);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException());
}
