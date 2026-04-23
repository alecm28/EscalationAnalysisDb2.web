namespace EscalationAnalysisDb2.Models
{
    public class DashboardViewModel
    {
        // Total de casos únicos
        public int TotalCases { get; set; }

        // Total de escalaciones (pueden existir varias por caso)
        public int TotalEscalations { get; set; }

        // Versión más impactada (por ahora valor fijo)
        public string MostImpactedVersion { get; set; } = "11.5";

        // Datos para gráfico de severidad
        public List<object> EscalationsBySeverity { get; set; } = new();

        // Datos para gráfico de estado
        public List<object> EscalationsByStatus { get; set; } = new();

        // Top cuentas
        public List<string> TopAccounts { get; set; } = new();

        // Top owners
        public List<string> TopOwners { get; set; } = new();

        // Insights del dashboard
        public string Insight1 { get; set; } = "";
        public string Insight2 { get; set; } = "";
        public string Insight3 { get; set; } = "";

        // Datos del gráfico de tendencia
        public List<int> TrendValues { get; set; } = new();

        // Etiquetas del gráfico de tendencia (meses)
        public List<string> TrendLabels { get; set; } = new();
    }
}