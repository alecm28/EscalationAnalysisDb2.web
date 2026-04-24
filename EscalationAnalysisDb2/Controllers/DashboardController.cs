using Microsoft.AspNetCore.Authorization;
using EscalationAnalysisDb2.Application.Services;
using EscalationAnalysisDb2.Infrastructure.Data;
using EscalationAnalysisDb2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EscalationAnalysisDb2.Controllers
{
    // requiere sesion iniciada
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly CaseService _caseService;
        private readonly EscalationsDbContext _context;

        public DashboardController(
            CaseService caseService,
            EscalationsDbContext context)
        {
            _caseService = caseService;
            _context = context;
        }

        public async Task<IActionResult> Index(
            int? month,
            List<int> severity,
            string region,
            string version)
        {
            // evita null en lista
            severity ??= new List<int>();

            // correo del usuario logueado
            var userEmail = User.Identity.Name;

            var currentUser = await _context.AppUsers
                .FirstOrDefaultAsync(x => x.Email == userEmail);

            // si no encuentra usuario vuelve al login
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // admin ve todo, user solo sus datos
            int? userId = null;

            if (!User.IsInRole("Admin"))
            {
                userId = currentUser.AppUserId;
            }

            var model = new DashboardViewModel();

            // tarjetas principales
            model.TotalCases = await _caseService.GetTotalCases(
                month, severity, region, version, userId);

            model.TotalEscalations = await _caseService.GetTotalEscalations(
                month, severity, region, version, userId);

            model.MostImpactedVersion = await _caseService.GetMostImpactedVersion(
                month, severity, region, version, userId);

            // datos grafico tendencia
            model.TrendValues = await _caseService.GetTrendValues(
                month, severity, region, version, userId);

            model.TrendLabels = _caseService.GetTrendLabels();

            // rankings
            model.TopAccounts = await _caseService.GetTopAccounts(
                month, severity, region, version, userId);

            model.TopAccountValues = await _caseService.GetTopAccountValues(
                month, severity, region, version, userId);

            model.TopOwners = await _caseService.GetTopOwners(
                month, severity, region, version, userId);

            model.TopOwnerValues = await _caseService.GetTopOwnerValues(
                month, severity, region, version, userId);

            // insights automativos
            model.Insight1 = await _caseService.GetMainInsight1(
                month, severity, region, version, userId);

            model.Insight2 = await _caseService.GetMainInsight2(
                month, severity, region, version, userId);

            model.Insight3 = await _caseService.GetMainInsight3(
                month, severity, region, version, userId);

            return View(model);
        }
    }
}