using EscalationAnalysisDb2.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EscalationAnalysisDb2.Controllers
{
    // solo administradores pueden entrar
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly EscalationsDbContext _context;

        public AdminController(EscalationsDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // total de usuarios
            ViewBag.TotalUsers = await _context.AppUsers.CountAsync();

            // usuarios activos
            ViewBag.ActiveUsers = await _context.AppUsers
                .CountAsync(x => x.IsActive);

            // total de reportes cargados
            ViewBag.TotalReports = await _context.Reports.CountAsync();

            // ultima fecha de carga
            ViewBag.LastUpload = await _context.Reports
                .OrderByDescending(x => x.UploadDate)
                .Select(x => x.UploadDate)
                .FirstOrDefaultAsync();

            // ultimos 5 reportes
            ViewBag.RecentReports = await _context.Reports
                .Include(x => x.UploadedByUser)
                .OrderByDescending(x => x.UploadDate)
                .Take(5)
                .ToListAsync();

            // lista de usuarios
            ViewBag.Users = await _context.AppUsers
                .OrderBy(x => x.Email)
                .ToListAsync();

            return View();
        }
    }
}