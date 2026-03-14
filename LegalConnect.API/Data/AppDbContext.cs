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
    public DbSet<CaseActivityDocument> CaseActivityDocuments => Set<CaseActivityDocument>();
    public DbSet<CaseDocument> CaseDocuments => Set<CaseDocument>();
    // ── New: Multi-Lawyer & Request System ────────────────────────────────────────────
    public DbSet<ClientLawyerRequest> ClientLawyerRequests => Set<ClientLawyerRequest>();
    public DbSet<CaseLawyer> CaseLawyers => Set<CaseLawyer>();
    public DbSet<CaseDocumentLawyerShare> CaseDocumentLawyerShares => Set<CaseDocumentLawyerShare>();
    // ── Deal Workflow ───────────────────────────────────────────────────────────────
    public DbSet<HireRequest> HireRequests => Set<HireRequest>();
    public DbSet<HireRequestMessage> HireRequestMessages => Set<HireRequestMessage>();
    public DbSet<HireRequestDocument> HireRequestDocuments => Set<HireRequestDocument>();
    public DbSet<Deal> Deals => Set<Deal>();
    public DbSet<Proposal> Proposals => Set<Proposal>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<LawyerInvoiceSettings> LawyerInvoiceSettings => Set<LawyerInvoiceSettings>();
    // ── Staff Management ────────────────────────────────────────────────────────
    public DbSet<StaffProfile> StaffProfiles => Set<StaffProfile>();
    public DbSet<CaseStaff> CaseStaffs => Set<CaseStaff>();
    public DbSet<StaffTask> StaffTasks => Set<StaffTask>();
    // ── Contracts & Billing ──────────────────────────────────────────────────────
    public DbSet<LegalContract> LegalContracts => Set<LegalContract>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<LitigationDispute> LitigationDisputes => Set<LitigationDispute>();
    public DbSet<DuesEntry> DuesEntries => Set<DuesEntry>();
    public DbSet<RefundInvoice> RefundInvoices => Set<RefundInvoice>();
    // ── Appointment Slot Management ─────────────────────────────────────────────
    public DbSet<LawyerTimeSlotConfiguration> LawyerTimeSlotConfigurations => Set<LawyerTimeSlotConfiguration>();
    public DbSet<LawyerWorkingHours> LawyerWorkingHours => Set<LawyerWorkingHours>();
    public DbSet<MasterHoliday> MasterHolidays => Set<MasterHoliday>();
    public DbSet<LawyerHolidayPreference> LawyerHolidayPreferences => Set<LawyerHolidayPreference>();
    public DbSet<LawyerPersonalHoliday> LawyerPersonalHolidays => Set<LawyerPersonalHoliday>();
    public DbSet<LawyerBlackoutBlock> LawyerBlackoutBlocks => Set<LawyerBlackoutBlock>();
    // ── Support Tickets ──────────────────────────────────────────────────────
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<SupportMessage> SupportMessages => Set<SupportMessage>();
    // ── Admin Staff Management ───────────────────────────────────────────────
    public DbSet<AdminStaffProfile> AdminStaffProfiles => Set<AdminStaffProfile>();
    public DbSet<AdminStaffRoleAssignment> AdminStaffRoleAssignments => Set<AdminStaffRoleAssignment>();
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
            e.Property(ca => ca.Title).HasMaxLength(200);
            e.Property(ca => ca.Description).HasMaxLength(1000).IsRequired();
            e.Property(ca => ca.Category).HasMaxLength(100);
            e.Property(ca => ca.CreatedByRole).HasMaxLength(50);
            e.Property(ca => ca.CreatedByName).HasMaxLength(200);
            e.Property(ca => ca.ActivityType).HasConversion<string>();
        });

        // ── CaseActivityDocument ─────────────────────────────────────────────────
        builder.Entity<CaseActivityDocument>(e =>
        {
            e.HasKey(d => new { d.CaseActivityId, d.CaseDocumentId });
            e.HasOne(d => d.CaseActivity).WithMany(a => a.LinkedDocuments).HasForeignKey(d => d.CaseActivityId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(d => d.CaseDocument).WithMany().HasForeignKey(d => d.CaseDocumentId).OnDelete(DeleteBehavior.Restrict);
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
            e.HasOne(i => i.Proposal).WithMany(p => p.Invoices).HasForeignKey(i => i.ProposalId)
                .OnDelete(DeleteBehavior.Restrict).IsRequired(false);
            e.HasOne(i => i.Deal).WithMany(d => d.Invoices).HasForeignKey(i => i.DealId).OnDelete(DeleteBehavior.Restrict);
            e.Property(i => i.InvoiceNumber).HasMaxLength(50).IsRequired();
            e.Property(i => i.ChargeType).HasMaxLength(100);
            e.Property(i => i.Amount).HasColumnType("decimal(18,2)");
            e.Property(i => i.GstRate).HasColumnType("decimal(5,2)");
            e.Property(i => i.GstAmount).HasColumnType("decimal(18,2)");
            e.Property(i => i.TotalAmount).HasColumnType("decimal(18,2)");
            e.Property(i => i.Description).HasMaxLength(2000);
            e.Property(i => i.Status).HasConversion<string>();
            e.HasIndex(i => i.InvoiceNumber).IsUnique();
        });

        // ── LawyerInvoiceSettings ──────────────────────────────────────────────
        builder.Entity<LawyerInvoiceSettings>(e =>
        {
            e.HasOne(s => s.LawyerProfile).WithOne(l => l.InvoiceSettings)
                .HasForeignKey<LawyerInvoiceSettings>(s => s.LawyerProfileId).OnDelete(DeleteBehavior.Cascade);
            e.Property(s => s.FirmName).HasMaxLength(200);
            e.Property(s => s.FirmLogoPath).HasMaxLength(500);
            e.Property(s => s.FirmAddress).HasMaxLength(500);
            e.Property(s => s.City).HasMaxLength(100);
            e.Property(s => s.State).HasMaxLength(100);
            e.Property(s => s.Country).HasMaxLength(100);
            e.Property(s => s.PostalCode).HasMaxLength(20);
            e.Property(s => s.GSTNumber).HasMaxLength(50);
            e.Property(s => s.Phone).HasMaxLength(20);
            e.Property(s => s.Email).HasMaxLength(200);
            e.Property(s => s.Website).HasMaxLength(300);
            e.Property(s => s.AuthorizedSignImagePath).HasMaxLength(500);
            e.Property(s => s.BankDetails).HasMaxLength(1000);
            e.Property(s => s.NotesForInvoice).HasMaxLength(1000);
            e.Property(s => s.TermsAndConditions).HasMaxLength(2000);
        });

        // ── LawyerTimeSlotConfiguration ─────────────────────────────────────────
        builder.Entity<LawyerTimeSlotConfiguration>(e =>
        {
            e.HasOne(c => c.LawyerProfile).WithOne(l => l.TimeSlotConfiguration).HasForeignKey<LawyerTimeSlotConfiguration>(c => c.LawyerProfileId).OnDelete(DeleteBehavior.Cascade);
            e.Property(c => c.SessionDurationMinutes).IsRequired();
            e.Property(c => c.BufferTimeMinutes).IsRequired();
        });

        // ── LawyerWorkingHours ──────────────────────────────────────────────────
        builder.Entity<LawyerWorkingHours>(e =>
        {
            e.HasOne(w => w.LawyerProfile).WithMany(l => l.WorkingHours).HasForeignKey(w => w.LawyerProfileId).OnDelete(DeleteBehavior.Cascade);
            e.Property(w => w.DayOfWeek).IsRequired();
            e.Property(w => w.StartTime).IsRequired();
            e.Property(w => w.EndTime).IsRequired();
            e.Property(w => w.IsWorking).IsRequired();
            e.HasIndex(w => new { w.LawyerProfileId, w.DayOfWeek });
        });

        // ── MasterHoliday ──────────────────────────────────────────────────────
        builder.Entity<MasterHoliday>(e =>
        {
            e.Property(m => m.HolidayName).HasMaxLength(200).IsRequired();
            e.Property(m => m.Description).HasMaxLength(500);
            e.Property(m => m.AppliesYearly).IsRequired();
        });

        // ── LawyerHolidayPreference ────────────────────────────────────────────
        builder.Entity<LawyerHolidayPreference>(e =>
        {
            e.HasOne(p => p.LawyerProfile).WithMany(l => l.HolidayPreferences).HasForeignKey(p => p.LawyerProfileId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(p => p.MasterHoliday).WithMany(m => m.LawyerPreferences).HasForeignKey(p => p.MasterHolidayId).OnDelete(DeleteBehavior.Cascade);
            e.Property(p => p.IsEnabled).IsRequired();
            e.HasIndex(p => new { p.LawyerProfileId, p.MasterHolidayId }).IsUnique();
        });

        // ── LawyerPersonalHoliday ──────────────────────────────────────────────
        builder.Entity<LawyerPersonalHoliday>(e =>
        {
            e.HasOne(h => h.LawyerProfile).WithMany(l => l.PersonalHolidays).HasForeignKey(h => h.LawyerProfileId).OnDelete(DeleteBehavior.Cascade);
            e.Property(h => h.HolidayDate).IsRequired();
            e.Property(h => h.Reason).HasMaxLength(200).IsRequired();
            e.Property(h => h.RecurringPattern).HasMaxLength(20).IsRequired();
        });

        // ── LawyerBlackoutBlock ────────────────────────────────────────────────
        builder.Entity<LawyerBlackoutBlock>(e =>
        {
            e.HasOne(b => b.LawyerProfile).WithMany(l => l.BlackoutBlocks).HasForeignKey(b => b.LawyerProfileId).OnDelete(DeleteBehavior.Cascade);
            e.Property(b => b.DayOfWeek).IsRequired();
            e.Property(b => b.StartTime).IsRequired();
            e.Property(b => b.EndTime).IsRequired();
            e.Property(b => b.Reason).HasMaxLength(200);
            e.Property(b => b.RecurringPattern).HasMaxLength(20).IsRequired();
            e.HasIndex(b => new { b.LawyerProfileId, b.DayOfWeek });
        });

        // ── StaffProfile ─────────────────────────────────────────────────────────
        builder.Entity<StaffProfile>(e =>
        {
            e.HasOne(s => s.User).WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.LawyerProfile).WithMany(l => l.StaffMembers).HasForeignKey(s => s.LawyerProfileId).OnDelete(DeleteBehavior.Restrict);
            e.Property(s => s.StaffRole).HasMaxLength(50).IsRequired();
        });

        // ── CaseStaff ─────────────────────────────────────────────────────────
        builder.Entity<CaseStaff>(e =>
        {
            e.HasOne(cs => cs.Case).WithMany(c => c.CaseStaffs).HasForeignKey(cs => cs.CaseId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(cs => cs.StaffProfile).WithMany(s => s.CaseStaffs).HasForeignKey(cs => cs.StaffProfileId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(cs => new { cs.CaseId, cs.StaffProfileId }).IsUnique();
        });

        // ── StaffTask ─────────────────────────────────────────────────────────
        builder.Entity<StaffTask>(e =>
        {
            e.HasOne(t => t.LawyerProfile).WithMany().HasForeignKey(t => t.LawyerProfileId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.AssignedTo).WithMany(s => s.AssignedTasks).HasForeignKey(t => t.AssignedToStaffProfileId).OnDelete(DeleteBehavior.SetNull).IsRequired(false);
            e.HasOne(t => t.Case).WithMany().HasForeignKey(t => t.CaseId).OnDelete(DeleteBehavior.SetNull).IsRequired(false);
            e.Property(t => t.Title).HasMaxLength(300).IsRequired();
            e.Property(t => t.Description).HasMaxLength(2000);
            e.Property(t => t.StaffNote).HasMaxLength(1000);
            e.Property(t => t.Priority).HasConversion<string>();
            e.Property(t => t.Status).HasConversion<string>();
            e.HasQueryFilter(t => !t.IsDeleted);
        });

        // ── LegalContract ─────────────────────────────────────────────────────
        builder.Entity<LegalContract>(e =>
        {
            e.HasOne(c => c.LawyerProfile).WithMany().HasForeignKey(c => c.LawyerProfileId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
            e.HasOne(c => c.Proposal).WithMany().HasForeignKey(c => c.ProposalId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
            e.Property(c => c.ContractType).HasMaxLength(50).IsRequired();
            e.Property(c => c.Title).HasMaxLength(300).IsRequired();
            e.Property(c => c.FileName).HasMaxLength(255).IsRequired();
            e.Property(c => c.FilePath).HasMaxLength(500).IsRequired();
        });

        // ── SystemSetting ─────────────────────────────────────────────────────
        builder.Entity<SystemSetting>(e =>
        {
            e.HasIndex(s => s.Key).IsUnique();
            e.Property(s => s.Key).HasMaxLength(100).IsRequired();
            e.Property(s => s.Value).HasColumnType("nvarchar(max)").IsRequired();
            e.Property(s => s.Description).HasMaxLength(500);
        });

        // ── LitigationDispute ─────────────────────────────────────────────────
        builder.Entity<LitigationDispute>(e =>
        {
            e.HasOne(d => d.Invoice).WithMany().HasForeignKey(d => d.InvoiceId).OnDelete(DeleteBehavior.Restrict);
            e.Property(d => d.DisputeType).HasMaxLength(50).IsRequired();
            e.Property(d => d.Reason).HasMaxLength(2000).IsRequired();
            e.Property(d => d.Status).HasMaxLength(50).IsRequired();
            e.Property(d => d.DisputedAmount).HasColumnType("decimal(18,2)");
        });

        // ── DuesEntry ─────────────────────────────────────────────────────────
        builder.Entity<DuesEntry>(e =>
        {
            e.HasOne(de => de.LawyerProfile).WithMany().HasForeignKey(de => de.LawyerProfileId).OnDelete(DeleteBehavior.Restrict);
            e.Property(de => de.EntryType).HasMaxLength(50).IsRequired();
            e.Property(de => de.Description).HasMaxLength(1000).IsRequired();
            e.Property(de => de.Amount).HasColumnType("decimal(18,2)");
        });

        // ── RefundInvoice ─────────────────────────────────────────────────────
        builder.Entity<RefundInvoice>(e =>
        {
            e.HasOne(r => r.LawyerProfile).WithMany().HasForeignKey(r => r.LawyerProfileId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.DuesEntry).WithMany().HasForeignKey(r => r.DuesEntryId).OnDelete(DeleteBehavior.SetNull).IsRequired(false);
            e.HasOne(r => r.Contract).WithMany().HasForeignKey(r => r.ContractId).OnDelete(DeleteBehavior.SetNull).IsRequired(false);
            e.Property(r => r.RefundInvoiceNumber).HasMaxLength(50).IsRequired();
            e.Property(r => r.Reason).HasMaxLength(1000).IsRequired();
            e.Property(r => r.Status).HasMaxLength(50).IsRequired();
            e.Property(r => r.Amount).HasColumnType("decimal(18,2)");
            e.HasIndex(r => r.RefundInvoiceNumber).IsUnique();
        });

        // ── Support Tickets ──────────────────────────────────────────────────
        builder.Entity<SupportTicket>(e =>
        {
            e.HasOne(t => t.User).WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.ClosedByUser).WithMany().HasForeignKey(t => t.ClosedByUserId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
            e.Property(t => t.UserRole).HasMaxLength(50).IsRequired();
            e.Property(t => t.Subject).HasMaxLength(300).IsRequired();
            e.Property(t => t.Description).HasMaxLength(3000).IsRequired();
            e.Property(t => t.Category).HasConversion<string>().HasMaxLength(50);
            e.Property(t => t.Priority).HasConversion<string>().HasMaxLength(50);
            e.Property(t => t.Status).HasConversion<string>().HasMaxLength(50);
        });

        builder.Entity<SupportMessage>(e =>
        {
            e.HasOne(m => m.SupportTicket).WithMany(t => t.Messages).HasForeignKey(m => m.SupportTicketId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(m => m.Sender).WithMany().HasForeignKey(m => m.SenderUserId).OnDelete(DeleteBehavior.Restrict);
            e.Property(m => m.SenderName).HasMaxLength(200).IsRequired();
            e.Property(m => m.SenderRole).HasMaxLength(50).IsRequired();
            e.Property(m => m.Message).HasMaxLength(4000).IsRequired();
        });

        // ── Admin Staff ──────────────────────────────────────────────────────
        builder.Entity<AdminStaffProfile>(e =>
        {
            e.HasOne(a => a.User).WithOne(u => u.AdminStaffProfile).HasForeignKey<AdminStaffProfile>(a => a.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.CreatedBy).WithMany().HasForeignKey(a => a.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
            e.Property(a => a.Department).HasMaxLength(100);
            e.HasIndex(a => a.UserId).IsUnique();
        });

        builder.Entity<AdminStaffRoleAssignment>(e =>
        {
            e.HasOne(r => r.AdminStaffProfile).WithMany(a => a.RoleAssignments).HasForeignKey(r => r.AdminStaffProfileId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.AssignedBy).WithMany().HasForeignKey(r => r.AssignedByUserId).OnDelete(DeleteBehavior.Restrict);
            e.Property(r => r.Role).HasConversion<string>().HasMaxLength(50);
            e.HasIndex(r => new { r.AdminStaffProfileId, r.Role }).IsUnique();
        });

        // ── Seed Data ─────────────────────────────────────────────────────────
        builder.Entity<CommissionSetting>().HasData(new CommissionSetting { Id = 1, DefaultCommissionPercentage = 10, LastUpdatedAt = new DateTime(2024, 1, 1), UpdatedBy = "System" });

        builder.Entity<SystemSetting>().HasData(
            new SystemSetting
            {
                Id = 1,
                Key = "LawyerRegistrationTnC",
                Value = "TERMS AND CONDITIONS FOR LAWYER REGISTRATION\n\n1. By registering on LegalConnect, you agree to abide by all platform policies and applicable laws.\n2. You confirm that all information provided is accurate and complete.\n3. You agree to maintain confidentiality of client information.\n4. The platform reserves the right to suspend or terminate accounts for policy violations.\n5. Disputes are subject to the platform's arbitration process.\n6. These terms may be updated periodically; continued use constitutes acceptance.",
                Description = "Terms and Conditions shown to lawyers during registration",
                UpdatedAt = new DateTime(2024, 1, 1),
                UpdatedByUserId = 1
            }
        );

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
