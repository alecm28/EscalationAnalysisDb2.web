using System;
using System.Collections.Generic;

namespace EscalationAnalysisDb2.Domain.Entities
{
    public class AppUser
    {
        public int AppUserId { get; set; }

        // 👇 OPCIONAL (se genera automático desde el email)
        public string? Username { get; set; }

        // 👇 OBLIGATORIO (se usa para login)
        public string Email { get; set; }

        // 👇 SIEMPRE se guarda hasheado
        public string Password { get; set; }

        public string Role { get; set; }

        public bool IsActive { get; set; }

        // 👇 Reset de contraseña
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }

        // 👇 Relaciones
        public ICollection<Report> UploadedReports { get; set; } = new List<Report>();
        public ICollection<Report> ExportedReports { get; set; } = new List<Report>();
    }
}