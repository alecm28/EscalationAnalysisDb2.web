using Microsoft.AspNetCore.Authorization;
using EscalationAnalysisDb2.Application.Services;
using EscalationAnalysisDb2.Models;
using Microsoft.AspNetCore.Mvc;

namespace EscalationAnalysisDb2.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly CaseService _caseService;

        public DashboardController(CaseService caseService)
        {
            _caseService = caseService;
        }

        public async Task<IActionResult> Index(
            int? month,
            List<int> severity,
            string region,
            string version)
        {
            // Si no se selecciona severidad, se inicializa vacío
            severity ??= new List<int>();

            var model = new DashboardViewModel();

            // Totales principales
            model.TotalCases = await _caseService.GetTotalCases(month, severity, region, version);
            model.TotalEscalations = await _caseService.GetTotalEscalations(month, severity, region, version);

            // Tendencia
            model.TrendValues = await _caseService.GetTrendValues(month, severity, region, version);
            model.TrendLabels = _caseService.GetTrendLabels();

            // Rankings
            model.TopAccounts = await _caseService.GetTopAccounts(month, severity, region, version);
            model.TopOwners = await _caseService.GetTopOwners(month, severity, region, version);

            // Insights
            model.Insight1 = await _caseService.GetMainInsight1(month, severity, region, version);
            model.Insight2 = await _caseService.GetMainInsight2(month, severity, region, version);
            model.Insight3 = await _caseService.GetMainInsight3(month, severity, region, version);

            return View(model);
        }
    }
}