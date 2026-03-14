using System.ComponentModel.DataAnnotations;

namespace LegalConnect.API.DTOs.Contracts;

public class LegalContractDto
{
    public int      Id             { get; set; }
    public string   ContractType   { get; set; } = string.Empty;
    public string   Title          { get; set; } = string.Empty;
    public string   FileName       { get; set; } = string.Empty;
    public long     FileSize       { get; set; }
    public DateTime GeneratedAt    { get; set; }
    public bool     IsAccepted     { get; set; }
    public DateTime? AcceptedAt    { get; set; }
    public string?  LawyerName     { get; set; }
    public string?  ProposalTitle  { get; set; }
    public string?  Notes          { get; set; }
}

public class ContractFilterDto
{
    public string?   ContractType { get; set; }
    public string?   LawyerName   { get; set; }
    public DateTime? DateFrom     { get; set; }
    public DateTime? DateTo       { get; set; }
    public int       Page         { get; set; } = 1;
    public int       PageSize     { get; set; } = 20;
}
