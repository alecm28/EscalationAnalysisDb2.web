using EscalationAnalysisDb2.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EscalationAnalysisDb2.Controllers
{
    public class HomeController : Controller
    {
        // logger para mensajes y errores
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // vista principal
        public IActionResult Index()
        {
            return View();
        }

        // vista privacy
        public IActionResult Privacy()
        {
            return View();
        }

        // evita cache en errores
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // envia id de solicitud para rastreo
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}