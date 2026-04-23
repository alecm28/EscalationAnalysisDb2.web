using EscalationAnalysisDb2.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EscalationAnalysisDb2.Controllers
{
    public class HomeController : Controller
    {
        // logger para registrar información o errores en la aplicación
        private readonly ILogger<HomeController> _logger;

        // constructor donde recibo el logger
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // retorna la vista principal
            return View();
        }

        public IActionResult Privacy()
        {
            // retorna la vista de privacidad
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // retorna la vista de error con el id de la petición para poder rastrearlo
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}