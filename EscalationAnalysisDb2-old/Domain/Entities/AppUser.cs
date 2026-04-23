namespace EscalationAnalysisDb2.Domain.Entities
{
    public class AppUser
    {
        public int AppUserId { get; set; }

        public string Username { get; set; }

        // 🔥 NUEVO (nullable para no romper DB)
        public string? Email { get; set; }

        public string Password { get; set; }

        public string Role { get; set; }

        public bool IsActive { get; set; }

        // 🔥 NUEVO
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }

        public ICollection<Report> UploadedReports { get; set; } = new List<Report>();
        public ICollection<Report> ExportedReports { get; set; } = new List<Report>();
    }
}