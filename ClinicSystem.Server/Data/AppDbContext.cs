using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ClinicSystem.Server.Models;

namespace ClinicSystem.Server.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Visit> Visits => Set<Visit>();
        public DbSet<Vitals> Vitals => Set<Vitals>();
        public DbSet<PatientHistory> PatientHistories => Set<PatientHistory>();
        public DbSet<Prescription> Prescriptions => Set<Prescription>();
        public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();
        public DbSet<LabRequest> LabRequests => Set<LabRequest>();
        public DbSet<LabResult> LabResults => Set<LabResult>();
        public DbSet<LabResultAttachment> LabResultAttachments => Set<LabResultAttachment>();
        public DbSet<Bill> Bills => Set<Bill>();
        public DbSet<BillItem> BillItems => Set<BillItem>();
        public DbSet<AISuggestion> AISuggestions => Set<AISuggestion>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Explicit keys for entities whose key names do not match EF conventions
            builder.Entity<PatientHistory>().HasKey(h => h.HistoryId);
            builder.Entity<PrescriptionItem>().HasKey(i => i.ItemId);
            builder.Entity<LabRequest>().HasKey(r => r.RequestId);
            builder.Entity<LabResult>().HasKey(r => r.ResultId);
            builder.Entity<LabResultAttachment>().HasKey(a => a.AttachmentId);
            builder.Entity<BillItem>().HasKey(i => i.ItemId);
            builder.Entity<AISuggestion>().HasKey(a => a.SuggestionId);

            // Visit - Patient
            builder.Entity<Visit>()
                .HasOne(v => v.Patient)
                .WithMany(p => p.Visits)
                .HasForeignKey(v => v.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Visit - Nurse (ApplicationUser) no cascade
            builder.Entity<Visit>()
                .HasOne(v => v.Nurse)
                .WithMany()
                .HasForeignKey(v => v.NurseId)
                .OnDelete(DeleteBehavior.SetNull);

            // Visit - Doctor (ApplicationUser) no cascade
            builder.Entity<Visit>()
                .HasOne(v => v.Doctor)
                .WithMany()
                .HasForeignKey(v => v.DoctorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Vitals - Visit one-to-one
            builder.Entity<Vitals>()
                .HasOne(v => v.Visit)
                .WithOne(v => v.Vitals)
                .HasForeignKey<Vitals>(v => v.VisitId)
                .OnDelete(DeleteBehavior.Cascade);

            // Vitals - Nurse no cascade
            builder.Entity<Vitals>()
                .HasOne(v => v.RecordedByNurse)
                .WithMany()
                .HasForeignKey(v => v.RecordedByNurseId)
                .OnDelete(DeleteBehavior.SetNull);

            // PatientHistory - Visit one-to-one
            builder.Entity<PatientHistory>()
                .HasOne(h => h.Visit)
                .WithOne(v => v.PatientHistory)
                .HasForeignKey<PatientHistory>(h => h.VisitId)
                .OnDelete(DeleteBehavior.Cascade);

            // PatientHistory - Patient no extra cascade (Visit already cascades)
            builder.Entity<PatientHistory>()
                .HasOne(h => h.Patient)
                .WithMany()
                .HasForeignKey(h => h.PatientId)
                .OnDelete(DeleteBehavior.NoAction);

            // PatientHistory - Doctor no cascade
            builder.Entity<PatientHistory>()
                .HasOne(h => h.Doctor)
                .WithMany()
                .HasForeignKey(h => h.DoctorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Prescription - Visit
            builder.Entity<Prescription>()
                .HasOne(p => p.Visit)
                .WithMany(v => v.Prescriptions)
                .HasForeignKey(p => p.VisitId)
                .OnDelete(DeleteBehavior.Cascade);

            // Prescription - Patient no extra cascade
            builder.Entity<Prescription>()
                .HasOne(p => p.Patient)
                .WithMany()
                .HasForeignKey(p => p.PatientId)
                .OnDelete(DeleteBehavior.NoAction);

            // Prescription - Doctor no cascade
            builder.Entity<Prescription>()
                .HasOne(p => p.Doctor)
                .WithMany()
                .HasForeignKey(p => p.DoctorId)
                .OnDelete(DeleteBehavior.SetNull);

            // LabRequest - Visit
            builder.Entity<LabRequest>()
                .HasOne(lr => lr.Visit)
                .WithMany(v => v.LabRequests)
                .HasForeignKey(lr => lr.VisitId)
                .OnDelete(DeleteBehavior.Cascade);

            // LabRequest - Patient no extra cascade
            builder.Entity<LabRequest>()
                .HasOne(lr => lr.Patient)
                .WithMany()
                .HasForeignKey(lr => lr.PatientId)
                .OnDelete(DeleteBehavior.NoAction);

            // LabRequest - Doctor no cascade
            builder.Entity<LabRequest>()
                .HasOne(lr => lr.RequestedByDoctor)
                .WithMany()
                .HasForeignKey(lr => lr.RequestedByDoctorId)
                .OnDelete(DeleteBehavior.SetNull);

            // LabResult - LabRequest one-to-one
            builder.Entity<LabResult>()
                .HasOne(r => r.Request)
                .WithOne(req => req.Result)
                .HasForeignKey<LabResult>(r => r.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // LabResult - LabTech no cascade
            builder.Entity<LabResult>()
                .HasOne(r => r.LabTech)
                .WithMany()
                .HasForeignKey(r => r.LabTechId)
                .OnDelete(DeleteBehavior.SetNull);

            // Bill - Visit one-to-one
            builder.Entity<Bill>()
                .HasOne(b => b.Visit)
                .WithOne(v => v.Bill)
                .HasForeignKey<Bill>(b => b.VisitId)
                .OnDelete(DeleteBehavior.Cascade);

            // Bill - Patient no extra cascade
            builder.Entity<Bill>()
                .HasOne(b => b.Patient)
                .WithMany()
                .HasForeignKey(b => b.PatientId)
                .OnDelete(DeleteBehavior.NoAction);

            // Bill - Cashier no cascade
            builder.Entity<Bill>()
                .HasOne(b => b.Cashier)
                .WithMany()
                .HasForeignKey(b => b.CashierId)
                .OnDelete(DeleteBehavior.SetNull);

            // AISuggestion - Visit
            builder.Entity<AISuggestion>()
                .HasOne(a => a.Visit)
                .WithMany(v => v.AISuggestions)
                .HasForeignKey(a => a.VisitId)
                .OnDelete(DeleteBehavior.Cascade);

            // AISuggestion - Patient no extra cascade
            builder.Entity<AISuggestion>()
                .HasOne(a => a.Patient)
                .WithMany()
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.NoAction);

            // AISuggestion - Doctor no cascade
            builder.Entity<AISuggestion>()
                .HasOne(a => a.RequestedByDoctor)
                .WithMany()
                .HasForeignKey(a => a.RequestedByDoctorId)
                .OnDelete(DeleteBehavior.SetNull);

            // AISuggestion - EditedBy user no cascade
            builder.Entity<AISuggestion>()
                .HasOne(a => a.EditedByUser)
                .WithMany()
                .HasForeignKey(a => a.EditedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Ignore computed properties not mapped to columns
            builder.Entity<Patient>().Ignore(p => p.FullName);
            builder.Entity<Patient>().Ignore(p => p.Age);
            builder.Entity<Vitals>().Ignore(v => v.Bmi);
            builder.Entity<Bill>().Ignore(b => b.Balance);
            builder.Entity<BillItem>().Ignore(bi => bi.TotalPrice);
        }
    }
}
