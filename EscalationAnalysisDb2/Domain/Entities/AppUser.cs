using System;
using System.Collections.Generic;

namespace EscalationAnalysisDb2.Domain.Entities
{
    public class AppUser
    {
        // id principal del usuario
        public int AppUserId { get; set; }

        // nombre interno de usuario
        public string? Username { get; set; }

        // correo para login
        public string Email { get; set; }

        // password guardada en hash
        public string Password { get; set; }

        // rol del sistema
        public string Role { get; set; }

        // activo o inactivo
        public bool IsActive { get; set; }

        // token para reset password
        public string? ResetToken { get; set; }

        // fecha limite del token
        public DateTime? ResetTokenExpiry { get; set; }

        // intentos fallidos de login
        public int FailedLoginAttempts { get; set; }

        // fecha hasta cuando queda bloqueado
        public DateTime? LockoutEnd { get; set; }

        // reportes subidos por usuario
        public ICollection<Report> UploadedReports { get; set; } = new List<Report>();

        // reportes exportados
        public ICollection<Report> ExportedReports { get; set; } = new List<Report>();
    }
}