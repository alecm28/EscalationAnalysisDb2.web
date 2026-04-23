namespace EscalationAnalysisDb2.Models
{
    public class DashboardViewModel
    {
        public int TotalCases { get; set; }
        public int TotalEscalations { get; set; }
        public string MostImpactedVersion { get; set; } = "11.5";

        public List<object> EscalationsBySeverity { get; set; } = new();
        public List<object> EscalationsByStatus { get; set; } = new();

        public List<string> TopAccounts { get; set; } = new();
        public List<int> TopAccountValues { get; set; } = new();

        public List<string> TopOwners { get; set; } = new();
        public List<int> TopOwnerValues { get; set; } = new();

        public string Insight1 { get; set; } = "";
        public string Insight2 { get; set; } = "";
        public string Insight3 { get; set; } = "";

        public List<int> TrendValues { get; set; } = new();
        public List<string> TrendLabels { get; set; } = new();
    }
}