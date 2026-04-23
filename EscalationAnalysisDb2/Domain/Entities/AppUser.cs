using System;
using System.Collections.Generic;

namespace EscalationAnalysisDb2.Domain.Entities
{
    public class AppUser
    {
        public int AppUserId { get; set; }

        public string? Username { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Role { get; set; }

        public bool IsActive { get; set; }

        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }

        // NUEVO LOCKOUT
        public int FailedLoginAttempts { get; set; }
        public DateTime? LockoutEnd { get; set; }

        public ICollection<Report> UploadedReports { get; set; } = new List<Report>();
        public ICollection<Report> ExportedReports { get; set; } = new List<Report>();
    }
}