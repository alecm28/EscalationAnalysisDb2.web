using EscalationAnalysisDb2.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EscalationAnalysisDb2.Controllers
{
    // requiere sesion iniciada
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly EscalationsDbContext _context;

        public ReportsController(EscalationsDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // obtiene rol actual
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // correo del usuario logueado
            var userEmail = User.Identity.Name;

            var currentUser = await _context.AppUsers
                .FirstOrDefaultAsync(x => x.Email == userEmail);

            // si no existe vuelve al login
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // consulta base de reportes
            var query = _context.Reports
                .Include(r => r.UploadedByUser)
                .AsQueryable();

            // usuario normal solo ve sus reportes
            if (userRole != "Admin")
            {
                query = query.Where(r => r.UploadedByUserId == currentUser.AppUserId);
            }

            // ordena por fecha mas reciente
            var reports = await query
                .OrderByDescending(r => r.UploadDate)
                .ToListAsync();

            return View(reports);
        }
    }
}