using System.Security.Claims;
using LegalConnect.API.DTOs.Appointment;
using LegalConnect.API.Helpers;
using LegalConnect.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LegalConnect.API.Controllers;

[ApiController]
[Route("api/appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpGet("slots")]
    public async Task<IActionResult> GetAvailableSlots([FromQuery] int lawyerId, [FromQuery] string date)
    {
        if (!DateTime.TryParse(date, out var parsedDate))
            return BadRequest(ApiResponse.Fail("Invalid date format. Use yyyy-MM-dd."));

        var slots = await _appointmentService.GetAvailableSlotsAsync(lawyerId, parsedDate);
        return Ok(ApiResponse<List<TimeSlotDto>>.Ok(slots));
    }

    [HttpPost]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed"));

        var (success, message, data) = await _appointmentService.BookAppointmentAsync(GetUserId(), dto);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse<AppointmentDto>.Ok(data!, message));
    }

    [HttpGet("my")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetMyAppointments([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _appointmentService.GetMyAppointmentsAsync(GetUserId(), page, pageSize);
        return Ok(ApiResponse<PagedResult<AppointmentDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetAppointmentById(int id)
    {
        var role = GetUserRole();
        var appointment = await _appointmentService.GetAppointmentByIdAsync(GetUserId(), role, id);
        if (appointment == null)
            return NotFound(ApiResponse.Fail("Appointment not found."));

        return Ok(ApiResponse<AppointmentDto>.Ok(appointment));
    }

    [HttpPut("{id:int}/cancel")]
    [Authorize(Roles = "Client,Lawyer")]
    public async Task<IActionResult> CancelAppointment(int id, [FromBody] CancelAppointmentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed"));

        var (success, message) = await _appointmentService.CancelAppointmentAsync(GetUserId(), GetUserRole(), id, dto);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    [HttpGet("lawyer")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> GetLawyerAppointments([FromQuery] string? from, [FromQuery] string? to)
    {
        DateTime? fromDate = null, toDate = null;
        if (!string.IsNullOrEmpty(from) && DateTime.TryParse(from, out var parsedFrom))
            fromDate = parsedFrom;
        if (!string.IsNullOrEmpty(to) && DateTime.TryParse(to, out var parsedTo))
            toDate = parsedTo;

        var appointments = await _appointmentService.GetLawyerAppointmentsAsync(GetUserId(), fromDate, toDate);
        return Ok(ApiResponse<List<AppointmentDto>>.Ok(appointments));
    }

    [HttpPut("{id:int}/confirm")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> ConfirmAppointment(int id)
    {
        var (success, message) = await _appointmentService.ConfirmAppointmentAsync(GetUserId(), id);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    [HttpPut("{id:int}/complete")]
    [Authorize(Roles = "Lawyer")]
    public async Task<IActionResult> CompleteAppointment(int id)
    {
        var (success, message) = await _appointmentService.CompleteAppointmentAsync(GetUserId(), id);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    [HttpPut("{id:int}/reschedule")]
    [Authorize(Roles = "Client,Lawyer")]
    public async Task<IActionResult> RescheduleAppointment(int id, [FromBody] RescheduleAppointmentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.Fail("Validation failed"));

        var (success, message) = await _appointmentService.RescheduleAppointmentAsync(GetUserId(), GetUserRole(), id, dto);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return Ok(ApiResponse.Ok(message));
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException());

    private string GetUserRole() =>
        User.FindFirstValue(ClaimTypes.Role)
        ?? User.FindFirstValue("role")
        ?? "Client";
}
