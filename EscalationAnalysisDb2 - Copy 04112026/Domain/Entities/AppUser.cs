namespace EscalationAnalysisDb2.Domain.Entities
{
    // Usuario del sistema (admin o viewer)
    public class AppUser
    {
        public int AppUserId { get; set; }

        // Usuario para login
        public string Username { get; set; }

        // Contraseña (luego se puede encriptar)
        public string Password { get; set; }

        // Rol (admin / viewer)
        public string Role { get; set; }

        // Permite activar o desactivar usuarios
        public bool IsActive { get; set; }

        // Reportes que este usuario ha subido
        public ICollection<Report> UploadedReports { get; set; } = new List<Report>();

        // (Opcional / revisar si se usa)
        public ICollection<Report> ExportedReports { get; set; } = new List<Report>();
    }
}





/*viejo
namespace EscalationAnalysisDb2.Domain.Entities
{
    // Usuario del sistema (admin o viewer)
    public class AppUser

    {
        public int AppUserId { get; set; }

        // Usuario para login
        public string Username { get; set; }

        // Contraseña (luego se puede encriptar)
        public string Password { get; set; }

        // Rol (admin / viewer)
        public string Role { get; set; }

        // Permite activar o desactivar usuarios
        public bool IsActive { get; set; }

        // Reportes que este usuario ha subido
        public ICollection<Report> UploadedReports { get; set; }

        // (Opcional / revisar si se usa)
        public ICollection<Report> ExportedReports { get; set; }
    }
}
*/