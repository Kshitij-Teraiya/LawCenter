using System.ComponentModel.DataAnnotations;

namespace LegalConnect.API.DTOs.Deals;

// ── HireRequest DTOs ─────────────────────────────────────────────────
public class HireRequestDto
{
    public int Id { get; set; }
    public int ClientProfileId { get; set; }
    public string ClientFullName { get; set; } = string.Empty;
    public string? ClientPicture { get; set; }
    public int LawyerProfileId { get; set; }
    public string LawyerFullName { get; set; } = string.Empty;
    public string? LawyerPicture { get; set; }
    public string? LawyerCategoryName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CaseType { get; set; } = string.Empty;
    public string Court { get; set; } = string.Empty;
    public string? Message { get; set; }
    public int MessageCount { get; set; }
    public int UnreadCount { get; set; }
    public bool HasDeal { get; set; }
    public int? DealId { get; set; }
    public int? LinkedCaseId { get; set; }
    public string? LinkedCaseTitle { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class HireRequestDetailDto : HireRequestDto
{
    public DealDto? Deal { get; set; }
    /// <summary>Basic case info visible to the lawyer before they are hired. No documents.</summary>
    public LinkedCasePreviewDto? LinkedCasePreview { get; set; }
    public List<HireRequestDocumentDto> Documents { get; set; } = [];
}

/// <summary>Limited case info exposed to a lawyer on a pending hire request.</summary>
public class LinkedCasePreviewDto
{
    public int    Id          { get; set; }
    public string CaseTitle   { get; set; } = string.Empty;
    public string CaseType    { get; set; } = string.Empty;
    public string Court       { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status      { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class HireRequestDocumentDto
{
    public int    Id            { get; set; }
    public int    HireRequestId { get; set; }
    public string FileName      { get; set; } = string.Empty;
    public long   FileSize      { get; set; }
    public string ContentType   { get; set; } = string.Empty;
    public DateTime UploadedAt  { get; set; }
    /// <summary>"HireRequest" = uploaded directly to this request; "Case" = auto-shared from linked case.</summary>
    public string SourceType    { get; set; } = "HireRequest";
    public string FileSizeFormatted => FileSize switch
    {
        >= 1_048_576 => $"{FileSize / 1_048_576.0:F1} MB",
        >= 1_024     => $"{FileSize / 1_024.0:F0} KB",
        _            => $"{FileSize} B"
    };
}

public class CreateHireRequestDto
{
    [Required]
    public int LawyerProfileId { get; set; }

    [Required]
    [MaxLength(3000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string CaseType { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Court { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Message { get; set; }

    /// <summary>
    /// Optional. If provided, links the hire request to an existing case.
    /// When the deal is finalised the lawyer will be added to that case
    /// instead of a new case being created.
    /// </summary>
    public int? LinkedCaseId { get; set; }
}

// ── Deal DTOs ────────────────────────────────────────────────────────
public class DealDto
{
    public int Id { get; set; }
    public int HireRequestId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public decimal AcceptedProposalAmount { get; set; }
    public decimal TotalInvoicedAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public List<ProposalDto> Proposals { get; set; } = [];
    public List<InvoiceDto> Invoices { get; set; } = [];
}

// ── Message DTOs ─────────────────────────────────────────────────────
public class HireRequestMessageDto
{
    public int Id { get; set; }
    public int HireRequestId { get; set; }
    public int SenderUserId { get; set; }
    public string SenderRole { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string? SenderPicture { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
}

public class SendMessageDto
{
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
}

// ── Proposal DTOs ────────────────────────────────────────────────────
public class ProposalDto
{
    public int Id { get; set; }
    public int DealId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsNegotiable { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ClientNote { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<InvoiceDto> Invoices { get; set; } = [];
}

public class CreateProposalDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(3000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    public bool IsNegotiable { get; set; } = false;
}

public class RespondToProposalDto
{
    [MaxLength(500)]
    public string? Note { get; set; }
}

// ── Invoice DTOs ─────────────────────────────────────────────────────
public class InvoiceDto
{
    public int Id { get; set; }
    public int? ProposalId { get; set; }
    public int DealId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string? ChargeType { get; set; }
    public decimal Amount { get; set; }
    public decimal? GstRate { get; set; }
    public decimal GstAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    // Populated for print view
    public string? LawyerFirmName { get; set; }
    public string? LawyerFullName { get; set; }
    public string? ClientFullName { get; set; }
    public string? LawyerGSTNumber { get; set; }
    public string? LawyerAddress { get; set; }
    public string? LawyerPhone { get; set; }
    public string? LawyerEmail { get; set; }
    public string? LawyerFirmLogoPath { get; set; }
    public string? LawyerBankDetails { get; set; }
    public string? LawyerNotesForInvoice { get; set; }
    public string? LawyerTermsAndConditions { get; set; }
    public string? LawyerAuthorizedSignImagePath { get; set; }
}

// Slim DTO for client invoice dropdown (dispute form)
public class ClientInvoiceSummaryDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string LawyerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class LawyerPaidInvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string ClientName   { get; set; } = string.Empty;
    public string ChargeType   { get; set; } = string.Empty;
    public decimal Amount      { get; set; }
    public decimal GstAmount   { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime PaidAt     { get; set; }   // UpdatedAt when Status == Paid
}

public class CreateInvoiceDto
{
    [Required]
    [MaxLength(100)]
    public string ChargeType { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public decimal Amount { get; set; }

    [Range(0, 100, ErrorMessage = "GST rate must be between 0 and 100.")]
    public decimal? GstRate { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }
}

// ── Invoice Settings DTOs ─────────────────────────────────────────────
public class LawyerInvoiceSettingsDto
{
    public int Id { get; set; }
    public int LawyerProfileId { get; set; }
    public string? FirmName { get; set; }
    public string? FirmLogoPath { get; set; }
    public string? FirmAddress { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? GSTNumber { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? AuthorizedSignImagePath { get; set; }
    public string? BankDetails { get; set; }
    public string? NotesForInvoice { get; set; }
    public string? TermsAndConditions { get; set; }
}

public class UpsertLawyerInvoiceSettingsDto
{
    [MaxLength(200)]
    public string? FirmName { get; set; }

    [MaxLength(500)]
    public string? FirmAddress { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? State { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [MaxLength(50)]
    public string? GSTNumber { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(200)]
    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(300)]
    public string? Website { get; set; }

    [MaxLength(1000)]
    public string? BankDetails { get; set; }

    [MaxLength(1000)]
    public string? NotesForInvoice { get; set; }

    [MaxLength(2000)]
    public string? TermsAndConditions { get; set; }
}
