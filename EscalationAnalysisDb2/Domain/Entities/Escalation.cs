namespace EscalationAnalysisDb2.Domain.Entities
{
    public class Escalation
    {
        // id principal
        public int EscalationId { get; set; }

        // caso relacionado
        public int CaseRecordId { get; set; }

        // nombre o tarea de escalacion
        public string EscalationTask { get; set; }

        // fecha de escalacion
        public DateTime EscalationDate { get; set; }

        // relaciones por id
        public int SeverityId { get; set; }
        public int StatusId { get; set; }

        // navegacion a tablas relacionadas
        public CaseRecord CaseRecord { get; set; }
        public Severity Severity { get; set; }
        public Status Status { get; set; }
    }
}