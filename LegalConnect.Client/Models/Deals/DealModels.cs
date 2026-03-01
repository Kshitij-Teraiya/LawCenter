using System.ComponentModel.DataAnnotations;

namespace LegalConnect.Client.Models.Deals;

// ── HireRequest ──────────────────────────────────────────────────────────────
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
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class HireRequestDetailDto : HireRequestDto
{
    public DealDto? Deal { get; set; }
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
}

// ── Deal ─────────────────────────────────────────────────────────────────────
public class DealDto
{
    public int Id { get; set; }
    public int HireRequestId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ProposalDto> Proposals { get; set; } = [];
    public List<InvoiceDto> Invoices { get; set; } = [];
}

// ── Message ──────────────────────────────────────────────────────────────────
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

// ── Proposal ─────────────────────────────────────────────────────────────────
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
    public InvoiceDto? Invoice { get; set; }
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
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public decimal Amount { get; set; }

    public bool IsNegotiable { get; set; } = false;
}

public class RespondToProposalDto
{
    [MaxLength(500)]
    public string? Note { get; set; }
}

// ── Invoice ──────────────────────────────────────────────────────────────────
public class InvoiceDto
{
    public int Id { get; set; }
    public int ProposalId { get; set; }
    public int DealId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateInvoiceDto
{
    [MaxLength(2000)]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }
}
