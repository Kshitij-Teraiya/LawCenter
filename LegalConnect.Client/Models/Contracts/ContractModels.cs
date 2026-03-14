namespace LegalConnect.Client.Models.Contracts;

public class LegalContractModel
{
    public int      Id            { get; set; }
    public string   ContractType  { get; set; } = string.Empty;
    public string   Title         { get; set; } = string.Empty;
    public string   FileName      { get; set; } = string.Empty;
    public long     FileSize      { get; set; }
    public DateTime GeneratedAt   { get; set; }
    public bool     IsAccepted    { get; set; }
    public DateTime? AcceptedAt   { get; set; }
    public string?  LawyerName    { get; set; }
    public string?  ProposalTitle { get; set; }
    public string?  Notes         { get; set; }

    public string FileSizeFormatted => FileSize switch
    {
        < 1024              => $"{FileSize} B",
        < 1024 * 1024       => $"{FileSize / 1024.0:F1} KB",
        _                   => $"{FileSize / (1024.0 * 1024):F1} MB"
    };
}

public static class ContractTypeNames
{
    public const string RegistrationTnC = "RegistrationTnC";
    public const string ProposalDraft   = "ProposalDraft";
    public const string ProposalFinal   = "ProposalFinal";
    public const string RefundInvoice   = "RefundInvoice";

    public static string Display(string type) => type switch
    {
        RegistrationTnC => "Registration T&C",
        ProposalDraft   => "Proposal Draft",
        ProposalFinal   => "Proposal Contract",
        RefundInvoice   => "Refund Invoice",
        _               => type
    };
}

public class ContractFilterModel
{
    public string?   ContractType { get; set; }
    public string?   LawyerName   { get; set; }
    public DateTime? DateFrom     { get; set; }
    public DateTime? DateTo       { get; set; }
    public int       Page         { get; set; } = 1;
    public int       PageSize     { get; set; } = 20;
}
