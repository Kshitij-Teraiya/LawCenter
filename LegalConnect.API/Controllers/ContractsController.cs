using LegalConnect.API.DTOs.Contracts;
using LegalConnect.API.Helpers;
using LegalConnect.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LegalConnect.API.Controllers;

[ApiController]
[Route("api/contracts")]
[Authorize(Roles = "Admin,Lawyer")]
public class ContractsController : ControllerBase
{
    private readonly IContractService _contractService;

    public ContractsController(IContractService contractService)
    {
        _contractService = contractService;
    }

    private int    CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string CurrentRole   => User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    /// <summary>GET api/contracts — Paged list with filters.</summary>
    [HttpGet]
    public async Task<IActionResult> GetContracts([FromQuery] ContractFilterDto filter)
    {
        var result = await _contractService.GetContractsAsync(CurrentUserId, CurrentRole, filter);
        return Ok(ApiResponse<PagedResult<LegalContractDto>>.Ok(result));
    }

    /// <summary>GET api/contracts/{id} — Contract details.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetContract(int id)
    {
        var contract = await _contractService.GetContractByIdAsync(id, CurrentUserId, CurrentRole);
        if (contract == null) return NotFound(ApiResponse<string>.Fail("Contract not found."));

        return Ok(ApiResponse<LegalContractDto>.Ok(new LegalContractDto
        {
            Id           = contract.Id,
            ContractType = contract.ContractType,
            Title        = contract.Title,
            FileName     = contract.FileName,
            FileSize     = contract.FileSize,
            GeneratedAt  = contract.GeneratedAt,
            IsAccepted   = contract.IsAccepted,
            AcceptedAt   = contract.AcceptedAt,
            Notes        = contract.Notes,
            LawyerName   = contract.LawyerProfile != null
                ? $"{contract.LawyerProfile.User.FirstName} {contract.LawyerProfile.User.LastName}"
                : null,
            ProposalTitle = contract.Proposal?.Title
        }));
    }

    /// <summary>GET api/contracts/{id}/download — Stream the PDF file.</summary>
    [HttpGet("{id:int}/download")]
    public async Task<IActionResult> Download(int id)
    {
        var (stream, fileName) = await _contractService.GetContractStreamAsync(id, CurrentUserId, CurrentRole);
        if (stream == null) return NotFound(ApiResponse<string>.Fail("Contract file not found."));

        return File(stream, "application/pdf", fileName);
    }
}
