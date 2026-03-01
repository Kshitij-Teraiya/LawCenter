using LegalConnect.API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LegalConnect.API.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── Existing Sets ────────────────────────────────────────────
    public DbSet<LawyerProfile> LawyerProfiles => Set<LawyerProfile>();
    public DbSet<ClientProfile> ClientProfiles => Set<ClientProfile>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Experience> Experiences => Set<Experience>();
    public DbSet<CaseResult> CaseResults => Set<CaseResult>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Certification> Certifications => Set<Certification>();
    public DbSet<Publication> Publications => Set<Publication>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<CommissionSetting> CommissionSettings => Set<CommissionSetting>();
    // ── Case Management & CRM Sets ────────────────────────────────────────────
    public DbSet<LawyerClient> LawyerClients => Set<LawyerClient>();
    public DbSet<Case> Cases => Set<Case>();
    public DbSet<CaseActivity> CaseActivities => Set<CaseActivity>();
    public DbSet<CaseDocument> CaseDocuments => Set<CaseDocument>();
    // ── New: Multi-Lawyer & Request System ────────────────────────────────────────────
    public DbSet<ClientLawyerRequest> ClientLawyerRequests => Set<ClientLawyerRequest>();
    public DbSet<CaseLawyer> CaseLawyers => Set<CaseLawyer>();
    public DbSet<CaseDocumentLawyerShare> CaseDocumentLawyerShares => Set<CaseDocumentLawyerShare>();
    // ── Deal Workflow ───────────────────────────────────────────────────────────────
    public DbSet<HireRequest> HireRequests => Set<HireRequest>();
    public DbSet<HireRequestMessage> HireRequestMessages => Set<HireRequestMessage>();
    public DbSet<Deal> Deals => Set<Deal>();
    public DbSet<Proposal> Proposals => Set<Proposal>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(e =>
        {
            e.Property(u => u.FirstName).HasMaxLength(50).IsRequired();
            e.Property(u => u.LastName).HasMaxLength(50).IsRequired();
            e.Property(u => u.ProfilePictureUrl).HasMaxLength(500);
        });

        builder.Entity<LawyerProfile>(e =>
        {
            e.HasOne(l => l.User).WithOne(u => u.LawyerProfile).HasForeignKey<LawyerProfile>(l => l.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(l => l.Category).WithMany(c => c.Lawyers).HasForeignKey(l => l.CategoryId).OnDelete(DeleteBehavior.Restrict);
            e.Property(l => l.BarCouncilNumber).HasMaxLength(100).IsRequired();
            e.Property(l => l.City).HasMaxLength(100).IsRequired();
            e.Property(l => l.Court).HasMaxLength(200).IsRequired();
            e.Property(l => l.Bio).HasMaxLength(2000);
            e.Property(l => l.ConsultationFee).HasColumnType("decimal(18,2)");
            e.Ignore(l => l.AverageRating);
            e.Ignore(l => l.TotalReviews);
            e.Ignore(l => l.ProfileCompletionPercentage);
        });

        builder.Entity<ClientProfile>(e =>
        {
            e.HasOne(c => c.User).WithOne(u => u.ClientProfile).HasForeignKey<ClientProfile>(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
            e.Property(c => c.City).HasMaxLength(100);
        });

        builder.Entity<Category>(e =>
        {
            e.Property(c => c.Name).HasMaxLength(100).IsRequired();
            e.Property(c => c.Description).HasMaxLength(500);
            e.Property(c => c.IconClass).HasMaxLength(100);
            e.Ignore(c => c.LawyerCount);
        });

        builder.Entity<Experience>(e =>
        {
            e.HasOne(ex => ex.LawyerProfile).WithMany(l => l.Experiences).HasForeignKey(ex => ex.LawyerProfileId).OnDelete(DeleteBehavior.Cascade);
            e.Property(ex => ex.Title).HasMaxLength(200).IsRequired();
            e.Property(ex => ex.Organization).HasMaxLength(200).IsRequired();
            e.Property(ex => ex.Description).HasMaxLength(1000);
        });

        builder.Entity<CaseResult>(e =>
        {
            e.HasOne(cr => cr.LawyerProfile).WithMany(l => l.CaseResults).HasForeignKey(cr => cr.LawyerProfileId).OnDelete(DeleteBehavior.Cascade);
            e.Property(cr => cr.CaseTitle).HasMaxLength(300).IsRequired();
            e.Property(cr => cr.Court).HasMaxLength(200).IsRequired();
            e.Property(cr => cr.Outcome).HasMaxLength(100).IsRequired();
            e.Property(cr => cr.Description).HasMaxLength(1000);
        });
        builder.Entity<Review>(e =>
        {
            e.HasOne(r => r.LawyerProfile).WithMany(l => l.Reviews).HasForeignKey(r => r.LawyerProfileId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.ClientProfile).WithMany(c => c.Reviews).HasForeignKey(r => r.ClientProfileId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Appointment).WithOne(a => a.Review).HasForeignKey<Review>(r => r.AppointmentId).OnDelete(DeleteBehavior.Restrict);
            e.Property(r => r.Comment).HasMaxLength(2000).IsRequired();
        });

        builder.Entity<Certification>(e =>
        {
            e.HasOne(c => c.LawyerProfile).WithMany(l => l.Certifications).HasForeignKey(c => c.LawyerProfileId).OnDelete(DeleteBehavior.Cascade);
            e.Property(c => c.Title).HasMaxLength(200).IsRequired();
            e.Property(c => c.IssuingOrganization).HasMaxLength(200).IsRequired();
            e.Property(c => c.CertificateUrl).HasMaxLength(500);
        });

        builder.Entity<Publication>(e =>
        {
            e.HasOne(p => p.LawyerProfile).WithMany(l => l.Publications).HasForeignKey(p => p.LawyerProfileId).OnDelete(DeleteBehavior.Cascade);
            e.Property(p => p.Title).HasMaxLength(300).IsRequired();
            e.Property(p => p.Publisher).HasMaxLength(200).IsRequired();
            e.Property(p => p.Url).HasMaxLength(500);
        });

        builder.Entity<Appointment>(e =>
        {
            e.HasOne(a => a.LawyerProfile).WithMany(l => l.Appointments).HasForeignKey(a => a.LawyerProfileId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.ClientProfile).WithMany(c => c.Appointments).HasForeignKey(a => a.ClientProfileId).OnDelete(DeleteBehavior.Restrict);
            e.Property(a => a.ConsultationFee).HasColumnType("decimal(18,2)");
            e.Property(a => a.PlatformCommission).HasColumnType("decimal(18,2)");
            e.Property(a => a.TotalAmount).HasColumnType("decimal(18,2)");
            e.Property(a => a.Notes).HasMaxLength(1000);
            e.Property(a => a.CancellationReason).HasMaxLength(500);
            e.Property(a => a.Status).HasConversion<string>();
        });

        builder.Entity<CommissionSetting>(e =>
        {
            e.Property(c => c.DefaultCommissionPercentage).HasColumnType("decimal(5,2)");
            e.Property(c => c.UpdatedBy).HasMaxLength(200);
        });
        // ── LawyerClient ─────────────────────────────────────────────────────────
        builder.Entity<LawyerClient>(e =>
        {
            e.HasOne(lc => lc.LawyerProfile).WithMany(l => l.LawyerClients).HasForeignKey(lc => lc.LawyerProfileId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(lc => lc.ClientProfile).WithMany(c => c.LawyerClients).HasForeignKey(lc => lc.ClientProfileId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(lc => lc.FirstAppointment).WithMany().HasForeignKey(lc => lc.FirstAppointmentId).OnDelete(DeleteBehavior.SetNull).IsRequired(false);
            e.Property(lc => lc.Notes).HasMaxLength(2000);
            e.HasIndex(lc => new { lc.LawyerProfileId, lc.ClientProfileId }).IsUnique();
        });

        // ── Case (LawyerProfileId now nullable) ────────────────────────────────────────────
        builder.Entity<Case>(e =>
        {
            e.HasOne(c => c.LawyerProfile).WithMany(l => l.Cases).HasForeignKey(c => c.LawyerProfileId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
            e.HasOne(c => c.ClientProfile).WithMany(cp => cp.Cases).HasForeignKey(c => c.ClientProfileId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.LawyerClient).WithMany(lc => lc.Cases).HasForeignKey(c => c.LawyerClientId).OnDelete(DeleteBehavior.SetNull).IsRequired(false);
            e.HasOne(c => c.Appointment).WithMany().HasForeignKey(c => c.AppointmentId).OnDelete(DeleteBehavior.SetNull).IsRequired(false);
            e.HasOne(c => c.Deal).WithMany().HasForeignKey(c => c.DealId).OnDelete(DeleteBehavior.SetNull).IsRequired(false);
            e.Property(c => c.CaseTitle).HasMaxLength(300).IsRequired();
            e.Property(c => c.CaseNumber).HasMaxLength(100);
            e.Property(c => c.CaseType).HasMaxLength(100).IsRequired();
            e.Property(c => c.Court).HasMaxLength(200).IsRequired();
            e.Property(c => c.Description).HasMaxLength(3000).IsRequired();
            e.Property(c => c.Outcome).HasMaxLength(2000);
            e.Property(c => c.Status).HasConversion<string>();
            e.HasQueryFilter(c => !c.IsDeleted);
        });

        // ── CaseActivity ─────────────────────────────────────────────────────────
        builder.Entity<CaseActivity>(e =>
        {
            e.HasOne(ca => ca.Case).WithMany(c => c.Activities).HasForeignKey(ca => ca.CaseId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ca => ca.CreatedBy).WithMany().HasForeignKey(ca => ca.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
            e.Property(ca => ca.Description).HasMaxLength(1000).IsRequired();
            e.Property(ca => ca.CreatedByRole).HasMaxLength(50);
            e.Property(ca => ca.CreatedByName).HasMaxLength(200);
            e.Property(ca => ca.ActivityType).HasConversion<string>();
        });

        // ── CaseDocument ─────────────────────────────────────────────────────────
        builder.Entity<CaseDocument>(e =>
        {
            e.HasOne(cd => cd.Case).WithMany(c => c.Documents).HasForeignKey(cd => cd.CaseId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(cd => cd.UploadedBy).WithMany().HasForeignKey(cd => cd.UploadedByUserId).OnDelete(DeleteBehavior.Restrict);
            e.Property(cd => cd.DocumentType).HasMaxLength(100).IsRequired();
            e.Property(cd => cd.FileName).HasMaxLength(255).IsRequired();
            e.Property(cd => cd.StoredFileName).HasMaxLength(255).IsRequired();
            e.Property(cd => cd.FilePath).HasMaxLength(500).IsRequired();
            e.Property(cd => cd.ContentType).HasMaxLength(100);
            e.HasQueryFilter(cd => !cd.IsDeleted);
        });
        // ── ClientLawyerRequest ─────────────────────────────────────────────────────────
        builder.Entity<ClientLawyerRequest>(e =>
        {
            e.HasOne(r => r.ClientProfile).WithMany(c => c.LawyerRequests).HasForeignKey(r => r.ClientProfileId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.LawyerProfile).WithMany(l => l.IncomingRequests).HasForeignKey(r => r.LawyerProfileId).OnDelete(DeleteBehavior.Restrict);
            e.Property(r => r.Message).HasMaxLength(1000);
            e.Property(r => r.LawyerNote).HasMaxLength(500);
            e.Property(r => r.Status).HasConversion<string>();
            e.HasIndex(r => new { r.ClientProfileId, r.LawyerProfileId }).IsUnique();
            e.HasQueryFilter(r => !r.IsDeleted);
        });

        // ── CaseLawyer ────────────────────────────────────────────────────────────────
        builder.Entity<CaseLawyer>(e =>
        {
            e.HasOne(cl => cl.Case).WithMany(c => c.CaseLawyers).HasForeignKey(cl => cl.CaseId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(cl => cl.LawyerProfile).WithMany(l => l.CaseLawyers).HasForeignKey(cl => cl.LawyerProfileId).OnDelete(DeleteBehavior.Restrict);
            e.Property(cl => cl.AddedByRole).HasMaxLength(20);
            e.HasIndex(cl => new { cl.CaseId, cl.LawyerProfileId }).IsUnique();
        });

        // ── CaseDocumentLawyerShare ─────────────────────────────────────────────────────────
        builder.Entity<CaseDocumentLawyerShare>(e =>
        {
            e.HasOne(s => s.CaseDocument).WithMany(d => d.LawyerShares).HasForeignKey(s => s.CaseDocumentId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.LawyerProfile).WithMany().HasForeignKey(s => s.LawyerProfileId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(s => new { s.CaseDocumentId, s.LawyerProfileId }).IsUnique();
        });

        // ── HireRequest ────────────────────────────────────────────────────────────────
        builder.Entity<HireRequest>(e =>
        {
            e.HasOne(h => h.ClientProfile).WithMany(c => c.HireRequests).HasForeignKey(h => h.ClientProfileId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(h => h.LawyerProfile).WithMany(l => l.HireRequests).HasForeignKey(h => h.LawyerProfileId).OnDelete(DeleteBehavior.Restrict);
            e.Property(h => h.Description).HasMaxLength(3000).IsRequired();
            e.Property(h => h.CaseType).HasMaxLength(100).IsRequired();
            e.Property(h => h.Court).HasMaxLength(200).IsRequired();
            e.Property(h => h.Message).HasMaxLength(1000);
            e.Property(h => h.Status).HasConversion<string>();
            e.HasQueryFilter(h => !h.IsDeleted);
        });

        // ── HireRequestMessage ─────────────────────────────────────────────────────
        builder.Entity<HireRequestMessage>(e =>
        {
            e.HasOne(m => m.HireRequest).WithMany(h => h.Messages).HasForeignKey(m => m.HireRequestId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(m => m.Sender).WithMany().HasForeignKey(m => m.SenderUserId).OnDelete(DeleteBehavior.Restrict);
            e.Property(m => m.SenderRole).HasMaxLength(20);
            e.Property(m => m.Content).HasMaxLength(2000).IsRequired();
        });

        // ── Deal ──────────────────────────────────────────────────────────────────
        builder.Entity<Deal>(e =>
        {
            e.HasOne(d => d.HireRequest).WithOne(h => h.Deal).HasForeignKey<Deal>(d => d.HireRequestId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(d => d.ClientProfile).WithMany().HasForeignKey(d => d.ClientProfileId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(d => d.LawyerProfile).WithMany().HasForeignKey(d => d.LawyerProfileId).OnDelete(DeleteBehavior.Restrict);
            e.Property(d => d.Status).HasConversion<string>();
            e.HasIndex(d => d.HireRequestId).IsUnique();
        });

        // ── Proposal ───────────────────────────────────────────────────────────────
        builder.Entity<Proposal>(e =>
        {
            e.HasOne(p => p.Deal).WithMany(d => d.Proposals).HasForeignKey(p => p.DealId).OnDelete(DeleteBehavior.Cascade);
            e.Property(p => p.Title).HasMaxLength(200).IsRequired();
            e.Property(p => p.Description).HasMaxLength(3000).IsRequired();
            e.Property(p => p.Amount).HasColumnType("decimal(18,2)");
            e.Property(p => p.Status).HasConversion<string>();
            e.Property(p => p.ClientNote).HasMaxLength(500);
        });

        // ── Invoice ────────────────────────────────────────────────────────────────
        builder.Entity<Invoice>(e =>
        {
            e.HasOne(i => i.Proposal).WithOne(p => p.Invoice).HasForeignKey<Invoice>(i => i.ProposalId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(i => i.Deal).WithMany(d => d.Invoices).HasForeignKey(i => i.DealId).OnDelete(DeleteBehavior.Restrict);
            e.Property(i => i.InvoiceNumber).HasMaxLength(50).IsRequired();
            e.Property(i => i.Amount).HasColumnType("decimal(18,2)");
            e.Property(i => i.Description).HasMaxLength(2000);
            e.Property(i => i.Status).HasConversion<string>();
            e.HasIndex(i => i.InvoiceNumber).IsUnique();
        });

        // ── Seed Data ─────────────────────────────────────────────────────────
        builder.Entity<CommissionSetting>().HasData(new CommissionSetting { Id = 1, DefaultCommissionPercentage = 10, LastUpdatedAt = new DateTime(2024, 1, 1), UpdatedBy = "System" });

        builder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Criminal Law", IconClass = "bi bi-shield", Description = "Criminal defense and prosecution", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new Category { Id = 2, Name = "Family Law", IconClass = "bi bi-people", Description = "Divorce, custody, adoption", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new Category { Id = 3, Name = "Corporate Law", IconClass = "bi bi-building", Description = "Business and corporate legal services", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new Category { Id = 4, Name = "Civil Law", IconClass = "bi bi-file-text", Description = "Civil disputes and litigation", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new Category { Id = 5, Name = "Property Law", IconClass = "bi bi-house", Description = "Real estate and property matters", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new Category { Id = 6, Name = "Labour Law", IconClass = "bi bi-briefcase", Description = "Employment and labour disputes", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }
        );
    }
}
