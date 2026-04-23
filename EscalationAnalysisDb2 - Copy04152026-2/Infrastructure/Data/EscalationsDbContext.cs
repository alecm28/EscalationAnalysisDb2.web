using Microsoft.EntityFrameworkCore;
using EscalationAnalysisDb2.Domain.Entities;

namespace EscalationAnalysisDb2.Infrastructure.Data
{
    // este contexto representa la conexión con la base de datos
    // aquí defino las tablas y cómo se relacionan entre sí
    public class EscalationsDbContext : DbContext
    {
        public EscalationsDbContext(DbContextOptions<EscalationsDbContext> options)
            : base(options)
        {
        }

        // tablas principales del sistema
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
            // aseguro que el username sea único
            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // un usuario puede subir varios reportes
            modelBuilder.Entity<Report>()
                .HasOne(r => r.UploadedByUser)
                .WithMany(u => u.UploadedReports)
                .HasForeignKey(r => r.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // cada caso pertenece a una cuenta
            modelBuilder.Entity<CaseRecord>()
                .HasOne(cr => cr.Account)
                .WithMany(a => a.CaseRecords)
                .HasForeignKey(cr => cr.AccountId);

            // cada caso tiene un owner asignado
            modelBuilder.Entity<CaseRecord>()
                .HasOne(cr => cr.CaseOwner)
                .WithMany(co => co.CaseRecords)
                .HasForeignKey(cr => cr.CaseOwnerId);

            // cada caso tiene una severidad base
            modelBuilder.Entity<CaseRecord>()
                .HasOne(cr => cr.Severity)
                .WithMany(s => s.CaseRecords)
                .HasForeignKey(cr => cr.SeverityId)
                .OnDelete(DeleteBehavior.Restrict);

            // cada caso tiene un estado base
            modelBuilder.Entity<CaseRecord>()
                .HasOne(cr => cr.Status)
                .WithMany(st => st.CaseRecords)
                .HasForeignKey(cr => cr.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // cada caso viene de un reporte
            modelBuilder.Entity<CaseRecord>()
                .HasOne(cr => cr.Report)
                .WithMany(r => r.CaseRecords)
                .HasForeignKey(cr => cr.ReportId);

            // índice para buscar casos más rápido por número
            modelBuilder.Entity<CaseRecord>()
                .HasIndex(cr => cr.CaseNumber);

            // un caso puede tener varias escalaciones
            modelBuilder.Entity<Escalation>()
                .HasOne(e => e.CaseRecord)
                .WithMany(cr => cr.Escalations)
                .HasForeignKey(e => e.CaseRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            // cada escalación tiene su propia severidad
            modelBuilder.Entity<Escalation>()
                .HasOne(e => e.Severity)
                .WithMany()
                .HasForeignKey(e => e.SeverityId)
                .OnDelete(DeleteBehavior.Restrict);

            // cada escalación tiene su propio estado
            modelBuilder.Entity<Escalation>()
                .HasOne(e => e.Status)
                .WithMany()
                .HasForeignKey(e => e.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // índice para buscar escalaciones más rápido
            modelBuilder.Entity<Escalation>()
                .HasIndex(e => e.EscalationTask);

            base.OnModelCreating(modelBuilder);
        }
    }
}