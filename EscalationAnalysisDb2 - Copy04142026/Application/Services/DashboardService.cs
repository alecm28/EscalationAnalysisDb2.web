using EscalationAnalysisDb2.Infrastructure.Data;
using EscalationAnalysisDb2.Models;

namespace EscalationAnalysisDb2.Application.Services
{
    public class DashboardService
    {
        // contexto de base de datos para obtener la información del dashboard
        private readonly EscalationsDbContext _context;

        // constructor donde recibo el contexto
        public DashboardService(EscalationsDbContext context)
        {
            _context = context;
        }

        public DashboardViewModel GetDashboardData()
        {
            // creo el modelo que voy a devolver al dashboard
            var model = new DashboardViewModel();

            // obtengo totales generales de escalaciones y casos
            model.TotalEscalations = _context.Escalations.Count();
            model.TotalCases = _context.CaseRecords.Count();

            // calculo la fecha de hace 6 meses para filtrar los datos
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);

            // obtengo las escalaciones de los últimos 6 meses y las agrupo por año y mes
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

            // genero las etiquetas del gráfico (meses en formato corto, ej: Jan, Feb)
            model.TrendLabels = trends
                .Select(x => System.Globalization.CultureInfo.CurrentCulture
                    .DateTimeFormat.GetAbbreviatedMonthName(x.Month))
                .ToList();

            // genero los valores del gráfico (cantidad de escalaciones por mes)
            model.TrendValues = trends
                .Select(x => x.Count)
                .ToList();

            // retorno el modelo completo listo para el dashboard
            return model;
        }
    }
}