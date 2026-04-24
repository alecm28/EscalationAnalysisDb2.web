namespace EscalationAnalysisDb2.Models
{
    public class DashboardViewModel
    {
        // tarjetas principales
        public int TotalCases { get; set; }
        public int TotalEscalations { get; set; }

        // version con mas impacto
        public string MostImpactedVersion { get; set; } = "11.5";

        // datos extra para graficos futuros
        public List<object> EscalationsBySeverity { get; set; } = new();
        public List<object> EscalationsByStatus { get; set; } = new();

        // top cuentas
        public List<string> TopAccounts { get; set; } = new();
        public List<int> TopAccountValues { get; set; } = new();

        // top owners
        public List<string> TopOwners { get; set; } = new();
        public List<int> TopOwnerValues { get; set; } = new();

        // insights del dashboard
        public string Insight1 { get; set; } = "";
        public string Insight2 { get; set; } = "";
        public string Insight3 { get; set; } = "";

        // grafico de tendencia
        public List<int> TrendValues { get; set; } = new();
        public List<string> TrendLabels { get; set; } = new();
    }
}