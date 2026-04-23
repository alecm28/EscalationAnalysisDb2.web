namespace EscalationAnalysisDb2.Models
{
    public class DashboardViewModel
    {
        // total de casos únicos
        public int TotalCases { get; set; }

        // total de escalaciones (pueden ser varias por caso)
        public int TotalEscalations { get; set; }

        // versión más impactada (por ahora con valor fijo)
        public string MostImpactedVersion { get; set; } = "11.5";

        // datos para gráfico de escalaciones por severidad
        public List<object> EscalationsBySeverity { get; set; } = new();

        // datos para gráfico de escalaciones por estado
        public List<object> EscalationsByStatus { get; set; } = new();

        // top cuentas con más casos
        public List<string> TopAccounts { get; set; } = new();

        // top owners con más casos
        public List<string> TopOwners { get; set; } = new();

        // insights que se muestran como texto en el dashboard
        public string Insight1 { get; set; } = "";
        public string Insight2 { get; set; } = "";
        public string Insight3 { get; set; } = "";

        // valores seleccionados para filtros (por ahora definidos por defecto)
        public string SelectedMonth { get; set; } = "March 2026";
        public string SelectedRegion { get; set; } = "Americas";
        public string SelectedVersion { get; set; } = "11.5";

        // valores para el gráfico de tendencia
        public List<int> TrendValues { get; set; } = new();

        // etiquetas del gráfico de tendencia (meses)
        public List<string> TrendLabels { get; set; } = new();
    }
}