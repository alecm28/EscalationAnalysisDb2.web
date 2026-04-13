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
            severity ??= new List<int>();

            var model = new DashboardViewModel();

            model.TotalCases = await _caseService.GetTotalCases(month, severity, region, version);
            model.TotalEscalations = await _caseService.GetTotalEscalations(month, severity, region, version);

            model.EscalationsBySeverity = await _caseService.GetEscalationsBySeverity();
            model.EscalationsByStatus = await _caseService.GetEscalationsByStatus();

            model.TrendValues = await _caseService.GetTrendValues(month, severity, region, version);
            model.TrendLabels = _caseService.GetTrendLabels();

            model.TopAccounts = await _caseService.GetTopAccounts(month, severity, region, version);
            model.TopOwners = await _caseService.GetTopOwners(month, severity, region, version);

            model.Insight1 = await _caseService.GetMainInsight1();
            model.Insight2 = await _caseService.GetMainInsight2();
            model.Insight3 = await _caseService.GetMainInsight3();

            return View(model);
        }
    }
}