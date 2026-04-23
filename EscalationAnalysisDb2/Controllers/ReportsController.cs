using EscalationAnalysisDb2.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EscalationAnalysisDb2.Controllers
{
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
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var userEmail = User.Identity.Name;

            var currentUser = await _context.AppUsers
                .FirstOrDefaultAsync(x => x.Email == userEmail);

            if (currentUser == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var query = _context.Reports
                .Include(r => r.UploadedByUser)
                .AsQueryable();

            if (userRole != "Admin")
            {
                query = query.Where(r => r.UploadedByUserId == currentUser.AppUserId);
            }

            var reports = await query
                .OrderByDescending(r => r.UploadDate)
                .ToListAsync();

            return View(reports);
        }
    }
} 