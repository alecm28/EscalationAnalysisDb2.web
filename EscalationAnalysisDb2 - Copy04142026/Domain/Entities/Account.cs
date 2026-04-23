namespace EscalationAnalysisDb2.Domain.Entities
{
    // Representa una cuenta/cliente
    public class Account
    {
        public int AccountId { get; set; }

        // Nombre de la cuenta (ej: IBM)
        public string AccountName { get; set; }

        // Relación: una cuenta puede tener muchos casos
        public ICollection<CaseRecord> CaseRecords { get; set; }
    }
}