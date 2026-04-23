using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EscalationAnalysisDb2.Controllers
{
    [Authorize]
    public class HelpController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}