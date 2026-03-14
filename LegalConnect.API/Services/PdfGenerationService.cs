using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LegalConnect.API.Services;

// ── Data classes passed into PDF generation ─────────────────────────────────

public class ProposalContractData
{
    public int    ProposalId      { get; set; }
    public string ProposalTitle   { get; set; } = string.Empty;
    public string Description     { get; set; } = string.Empty;
    public decimal Amount         { get; set; }
    public bool   IsNegotiable    { get; set; }
    public DateTime CreatedAt     { get; set; }
    public bool   IsFinal         { get; set; }

    // Lawyer
    public string LawyerName      { get; set; } = string.Empty;
    public string LawyerEmail     { get; set; } = string.Empty;
    public string LawyerCity      { get; set; } = string.Empty;
    public string? FirmName       { get; set; }
    public string? SignImagePath  { get; set; }   // absolute path

    // Client
    public string ClientName      { get; set; } = string.Empty;
    public string ClientEmail     { get; set; } = string.Empty;
}

public class RefundInvoiceData
{
    public string RefundInvoiceNumber { get; set; } = string.Empty;
    public decimal Amount             { get; set; }
    public string Reason              { get; set; } = string.Empty;
    public DateTime GeneratedAt       { get; set; }
    public string LawyerName          { get; set; } = string.Empty;
    public string LawyerEmail         { get; set; } = string.Empty;
    public string? FirmName           { get; set; }
    public string GeneratedByName     { get; set; } = string.Empty;
}

// ── Service interface and implementation ─────────────────────────────────────

public interface IPdfGenerationService
{
    byte[] GenerateRegistrationTnC(string lawyerFullName, string email, string tncText, DateTime acceptedAt);
    byte[] GenerateProposalContract(ProposalContractData data);
    byte[] GenerateRefundInvoicePdf(RefundInvoiceData data);
}

public class PdfGenerationService : IPdfGenerationService
{
    private const string PlatformName   = "LegalConnect";
    private const string PlatformTagline = "Connecting Clients with Trusted Legal Professionals";

    public byte[] GenerateRegistrationTnC(string lawyerFullName, string email, string tncText, DateTime acceptedAt)
    {
        return Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                page.Header().Element(c => RenderHeader(c, "Lawyer Registration — Terms & Conditions"));

                page.Content().Column(col =>
                {
                    col.Item().PaddingTop(10).Text(t =>
                    {
                        t.Span("Lawyer: ").Bold();
                        t.Span(lawyerFullName);
                    });
                    col.Item().Text(t =>
                    {
                        t.Span("Email: ").Bold();
                        t.Span(email);
                    });
                    col.Item().Text(t =>
                    {
                        t.Span("Accepted On: ").Bold();
                        t.Span(acceptedAt.ToString("dd MMM yyyy, HH:mm UTC"));
                    });

                    col.Item().PaddingTop(16).BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(4)
                       .Text("Terms and Conditions").FontSize(13).Bold();

                    foreach (var line in tncText.Split('\n'))
                    {
                        var trimmed = line.Trim();
                        if (string.IsNullOrWhiteSpace(trimmed))
                            col.Item().Height(6);
                        else
                            col.Item().PaddingTop(2).Text(trimmed).FontSize(10);
                    }

                    col.Item().PaddingTop(24).Text(t =>
                    {
                        t.Span("✓ Accepted by ").Bold();
                        t.Span(lawyerFullName);
                        t.Span($" on {acceptedAt:dd MMM yyyy, HH:mm} UTC");
                    });
                });

                page.Footer().Element(c => RenderFooter(c));
            });
        }).GeneratePdf();
    }

    public byte[] GenerateProposalContract(ProposalContractData data)
    {
        var docType = data.IsFinal ? "Proposal Contract — Final" : "Proposal Contract — Draft";

        return Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                page.Header().Element(c => RenderHeader(c, docType));

                page.Content().Column(col =>
                {
                    // Proposal info section
                    col.Item().PaddingTop(10).Table(t =>
                    {
                        t.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });

                        t.Cell().Text(tx => { tx.Span("Proposal ID: ").Bold(); tx.Span($"#{data.ProposalId}"); });
                        t.Cell().Text(tx => { tx.Span("Date: ").Bold(); tx.Span(data.CreatedAt.ToString("dd MMM yyyy")); });

                        t.Cell().ColumnSpan(2).PaddingTop(4).Text(tx =>
                        {
                            tx.Span("Title: ").Bold();
                            tx.Span(data.ProposalTitle);
                        });
                    });

                    col.Item().PaddingTop(12).BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(4)
                        .Text("Parties").FontSize(11).Bold();

                    col.Item().PaddingTop(6).Table(t =>
                    {
                        t.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });

                        t.Cell().Column(c =>
                        {
                            c.Item().Text("LAWYER (Service Provider)").Bold().FontSize(9).FontColor(Colors.Grey.Darken2);
                            c.Item().Text(data.LawyerName).Bold();
                            if (!string.IsNullOrWhiteSpace(data.FirmName))
                                c.Item().Text(data.FirmName).FontSize(9);
                            c.Item().Text(data.LawyerEmail).FontSize(9);
                            c.Item().Text(data.LawyerCity).FontSize(9);
                        });

                        t.Cell().Column(c =>
                        {
                            c.Item().Text("CLIENT").Bold().FontSize(9).FontColor(Colors.Grey.Darken2);
                            c.Item().Text(data.ClientName).Bold();
                            c.Item().Text(data.ClientEmail).FontSize(9);
                        });
                    });

                    col.Item().PaddingTop(12).BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(4)
                        .Text("Proposal Details").FontSize(11).Bold();

                    col.Item().PaddingTop(6).Text(data.Description).FontSize(10);

                    col.Item().PaddingTop(10).Table(t =>
                    {
                        t.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(1); });

                        t.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Description").Bold().FontSize(9);
                        t.Cell().Background(Colors.Grey.Lighten3).Padding(6).AlignRight().Text("Amount").Bold().FontSize(9);

                        t.Cell().Padding(6).Text(data.ProposalTitle);
                        t.Cell().Padding(6).AlignRight().Text($"₹{data.Amount:N2}");
                    });

                    if (data.IsNegotiable)
                        col.Item().PaddingTop(4).Text("* Amount is negotiable").FontSize(9).Italic().FontColor(Colors.Grey.Darken1);

                    if (data.IsFinal)
                    {
                        col.Item().PaddingTop(20).BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(4)
                            .Text("Authorised Signature").FontSize(11).Bold();

                        col.Item().PaddingTop(8).Row(r =>
                        {
                            r.RelativeItem().Column(c =>
                            {
                                // Signature image or typed name
                                if (!string.IsNullOrWhiteSpace(data.SignImagePath) && File.Exists(data.SignImagePath))
                                {
                                    try
                                    {
                                        c.Item().MaxHeight(60).Image(data.SignImagePath);
                                    }
                                    catch
                                    {
                                        c.Item().Text(data.LawyerName).Bold().FontSize(12).Italic();
                                    }
                                }
                                else
                                {
                                    c.Item().Text(data.LawyerName).Bold().FontSize(12).Italic();
                                }
                                c.Item().PaddingTop(4).Text(data.LawyerName).FontSize(9);
                                c.Item().Text("Lawyer / Service Provider").FontSize(9).FontColor(Colors.Grey.Darken1);
                            });

                            r.RelativeItem().Column(c =>
                            {
                                c.Item().Height(60).BorderBottom(1).BorderColor(Colors.Grey.Lighten1);
                                c.Item().PaddingTop(4).Text(data.ClientName).FontSize(9);
                                c.Item().Text("Client").FontSize(9).FontColor(Colors.Grey.Darken1);
                            });
                        });
                    }
                    else
                    {
                        col.Item().PaddingTop(20).Background(Colors.Yellow.Lighten4).Padding(8)
                            .Text("DRAFT — This document is not yet finalised. It will be updated upon client acceptance.")
                            .FontSize(9).Italic().FontColor(Colors.Orange.Darken2);
                    }
                });

                page.Footer().Element(c => RenderFooter(c));
            });
        }).GeneratePdf();
    }

    public byte[] GenerateRefundInvoicePdf(RefundInvoiceData data)
    {
        return Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                page.Header().Element(c => RenderHeader(c, "Refund Invoice"));

                page.Content().Column(col =>
                {
                    col.Item().PaddingTop(10).Table(t =>
                    {
                        t.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });

                        t.Cell().Text(tx => { tx.Span("Invoice No: ").Bold(); tx.Span(data.RefundInvoiceNumber); });
                        t.Cell().Text(tx => { tx.Span("Date: ").Bold(); tx.Span(data.GeneratedAt.ToString("dd MMM yyyy")); });

                        t.Cell().PaddingTop(4).Text(tx => { tx.Span("Lawyer: ").Bold(); tx.Span(data.LawyerName); });
                        t.Cell().PaddingTop(4).Text(tx => { tx.Span("Issued By: ").Bold(); tx.Span(data.GeneratedByName); });

                        if (!string.IsNullOrWhiteSpace(data.FirmName))
                        {
                            t.Cell().Text(tx => { tx.Span("Firm: ").Bold(); tx.Span(data.FirmName); });
                            t.Cell();
                        }
                    });

                    col.Item().PaddingTop(16).Table(t =>
                    {
                        t.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(1); });

                        t.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Description").Bold();
                        t.Cell().Background(Colors.Grey.Lighten3).Padding(6).AlignRight().Text("Amount").Bold();

                        t.Cell().Padding(6).Text(data.Reason);
                        t.Cell().Padding(6).AlignRight().Text($"₹{data.Amount:N2}");

                        t.Cell().BorderTop(1).BorderColor(Colors.Grey.Lighten1).Padding(6).Text("Total Refund").Bold();
                        t.Cell().BorderTop(1).BorderColor(Colors.Grey.Lighten1).Padding(6).AlignRight().Text($"₹{data.Amount:N2}").Bold();
                    });

                    col.Item().PaddingTop(16).Text("This refund credit has been applied to the lawyer's dues account on the LegalConnect platform.")
                        .FontSize(9).Italic().FontColor(Colors.Grey.Darken1);
                });

                page.Footer().Element(c => RenderFooter(c));
            });
        }).GeneratePdf();
    }

    // ── Shared header/footer ─────────────────────────────────────────────────

    private static void RenderHeader(IContainer container, string documentTitle)
    {
        container.Column(col =>
        {
            col.Item().Row(r =>
            {
                r.RelativeItem().Column(c =>
                {
                    c.Item().Text(PlatformName).Bold().FontSize(18).FontColor(Colors.Blue.Darken3);
                    c.Item().Text(PlatformTagline).FontSize(8).FontColor(Colors.Grey.Darken1);
                });
                r.AutoItem().AlignRight().AlignBottom()
                    .Text(documentTitle).FontSize(13).Bold().FontColor(Colors.Grey.Darken2);
            });
            col.Item().PaddingTop(4).BorderBottom(2).BorderColor(Colors.Blue.Darken3);
        });
    }

    private static void RenderFooter(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(4).Row(r =>
            {
                r.RelativeItem().Text(t =>
                {
                    t.Span("Generated by ").FontSize(8).FontColor(Colors.Grey.Darken1);
                    t.Span(PlatformName).Bold().FontSize(8).FontColor(Colors.Grey.Darken1);
                    t.Span($" · {DateTime.UtcNow:dd MMM yyyy, HH:mm} UTC").FontSize(8).FontColor(Colors.Grey.Darken1);
                });
                r.AutoItem().Text(t =>
                {
                    t.Span("Page ").FontSize(8).FontColor(Colors.Grey.Darken1);
                    t.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken1);
                    t.Span(" of ").FontSize(8).FontColor(Colors.Grey.Darken1);
                    t.TotalPages().FontSize(8).FontColor(Colors.Grey.Darken1);
                });
            });
        });
    }
}
