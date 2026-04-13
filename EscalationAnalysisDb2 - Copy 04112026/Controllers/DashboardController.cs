using Microsoft.AspNetCore.Authorization;
using EscalationAnalysisDb2.Application.Services;
using EscalationAnalysisDb2.Models;
using Microsoft.AspNetCore.Mvc;

namespace EscalationAnalysisDb2.Controllers
{
    [Authorize] // nueva autoriza
    public class DashboardController : Controller
    {
        private readonly CaseService _caseService;

        public DashboardController(CaseService caseService)
        {
            _caseService = caseService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel();

            model.TotalCases = await _caseService.GetTotalCases();
            model.TotalEscalations = await _caseService.GetTotalEscalations();

            model.EscalationsBySeverity = await _caseService.GetEscalationsBySeverity();
            model.EscalationsByStatus = await _caseService.GetEscalationsByStatus();

            model.TrendValues = await _caseService.GetTrendValues();
            model.TrendLabels = _caseService.GetTrendLabels();

            model.TopAccounts = await _caseService.GetTopAccounts();
            model.TopOwners = await _caseService.GetTopOwners();

            model.Insight1 = await _caseService.GetMainInsight1();
            model.Insight2 = await _caseService.GetMainInsight2();
            model.Insight3 = await _caseService.GetMainInsight3();

            return View(model);
        }
    }
}





/*
using Microsoft.AspNetCore.Authorization;//nuevo
using EscalationAnalysisDb2.Application.Services;
using EscalationAnalysisDb2.Models;
using Microsoft.AspNetCore.Mvc;

namespace EscalationAnalysisDb2.Controllers
{
    public class DashboardController : Controller
    {
        // servicio que contiene toda la lógica para obtener los datos del dashboard
        private readonly CaseService _caseService;

        // constructor donde inyecto el servicio
        public DashboardController(CaseService caseService)
        {
            _caseService = caseService;
        }

        public async Task<IActionResult> Index()
        {
            // creo el modelo que voy a enviar a la vista
            var model = new DashboardViewModel();

            // obtengo los totales principales
            model.TotalCases = await _caseService.GetTotalCases();
            model.TotalEscalations = await _caseService.GetTotalEscalations();

            // datos para gráficos de severidad y estado
            model.EscalationsBySeverity = await _caseService.GetEscalationsBySeverity();
            model.EscalationsByStatus = await _caseService.GetEscalationsByStatus();

            // datos de tendencia en el tiempo
            model.TrendValues = await _caseService.GetTrendValues();
            model.TrendLabels = _caseService.GetTrendLabels();

            // top cuentas y owners
            model.TopAccounts = await _caseService.GetTopAccounts();
            model.TopOwners = await _caseService.GetTopOwners();

            // insights que se muestran como texto en el dashboard
            model.Insight1 = await _caseService.GetMainInsight1();
            model.Insight2 = await _caseService.GetMainInsight2();
            model.Insight3 = await _caseService.GetMainInsight3();

            // retorno la vista con toda la información lista
            return View(model);
        }
    }
}
*/