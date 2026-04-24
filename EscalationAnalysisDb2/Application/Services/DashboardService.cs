using EscalationAnalysisDb2.Infrastructure.Data;
using EscalationAnalysisDb2.Models;

namespace EscalationAnalysisDb2.Application.Services
{
    public class DashboardService
    {
        // acceso a base de datos
        private readonly EscalationsDbContext _context;

        public DashboardService(EscalationsDbContext context)
        {
            _context = context;
        }

        public DashboardViewModel GetDashboardData()
        {
            // modelo que se envia a la vista
            var model = new DashboardViewModel();

            // totales principales
            model.TotalEscalations = _context.Escalations.Count();
            model.TotalCases = _context.CaseRecords.Count();

            var sixMonthsAgo = DateTime.Now.AddMonths(-6);

            // agrupa escalaciones por ano y mes
            var trends = _context.Escalations
                .Where(e => e.EscalationDate >= sixMonthsAgo)
                .GroupBy(e => new { e.EscalationDate.Year, e.EscalationDate.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Count = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            // nombres de meses para grafico
            model.TrendLabels = trends
                .Select(x => System.Globalization.CultureInfo.CurrentCulture
                    .DateTimeFormat.GetAbbreviatedMonthName(x.Month))
                .ToList();

            // cantidades por mes
            model.TrendValues = trends
                .Select(x => x.Count)
                .ToList();

            return model;
        }
    }
}