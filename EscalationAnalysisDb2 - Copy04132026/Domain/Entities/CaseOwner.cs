namespace EscalationAnalysisDb2.Domain.Entities
{
    // Persona dueña del caso
    public class CaseOwner
    {
        public int CaseOwnerId { get; set; }

        // Nombre del owner
        public string CaseOwnerName { get; set; }

        // Región a la que pertenece
        public string Region { get; set; }

        // Un owner puede tener varios casos
        public ICollection<CaseRecord> CaseRecords { get; set; }
    }
}