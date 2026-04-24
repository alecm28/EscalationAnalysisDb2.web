using Microsoft.EntityFrameworkCore;
using EscalationAnalysisDb2.Domain.Entities;

namespace EscalationAnalysisDb2.Infrastructure.Data
{
    // contexto principal de entity framework
    public class EscalationsDbContext : DbContext
    {
        public EscalationsDbContext(DbContextOptions<EscalationsDbContext> options)
            : base(options)
        {
        }

        // tablas del sistema
        public DbSet<Account> Accounts { get; set; }
        public DbSet<CaseOwner> CaseOwners { get; set; }
        public DbSet<Severity> Severities { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<CaseRecord> CaseRecords { get; set; }
        public DbSet<Escalation> Escalations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // username unico
            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // usuario puede subir muchos reportes
            modelBuilder.Entity<Report>()
                .HasOne(r => r.UploadedByUser)
                .WithMany(u => u.UploadedReports)
                .HasForeignKey(r => r.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // relaciones de caso
            modelBuilder.Entity<CaseRecord>()
                .HasOne(cr => cr.Account)
                .WithMany(a => a.CaseRecords)
                .HasForeignKey(cr => cr.AccountId);

            modelBuilder.Entity<CaseRecord>()
                .HasOne(cr => cr.CaseOwner)
                .WithMany(co => co.CaseRecords)
                .HasForeignKey(cr => cr.CaseOwnerId);

            modelBuilder.Entity<CaseRecord>()
                .HasOne(cr => cr.Severity)
                .WithMany(s => s.CaseRecords)
                .HasForeignKey(cr => cr.SeverityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CaseRecord>()
                .HasOne(cr => cr.Status)
                .WithMany(st => st.CaseRecords)
                .HasForeignKey(cr => cr.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CaseRecord>()
                .HasOne(cr => cr.Report)
                .WithMany(r => r.CaseRecords)
                .HasForeignKey(cr => cr.ReportId);

            // indice para busquedas por numero de caso
            modelBuilder.Entity<CaseRecord>()
                .HasIndex(cr => cr.CaseNumber);

            // relaciones de escalacion
            modelBuilder.Entity<Escalation>()
                .HasOne(e => e.CaseRecord)
                .WithMany(cr => cr.Escalations)
                .HasForeignKey(e => e.CaseRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Escalation>()
                .HasOne(e => e.Severity)
                .WithMany()
                .HasForeignKey(e => e.SeverityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Escalation>()
                .HasOne(e => e.Status)
                .WithMany()
                .HasForeignKey(e => e.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // indice para escalacion
            modelBuilder.Entity<Escalation>()
                .HasIndex(e => e.EscalationTask);

            base.OnModelCreating(modelBuilder);
        }
    }
}