using EscalationAnalysisDb2.Application.Services;
using EscalationAnalysisDb2.Application.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EscalationAnalysisDb2.Infrastructure.Data;
using EscalationAnalysisDb2.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace EscalationAnalysisDb2.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly UserService _userService;
        private readonly EscalationsDbContext _context;
        private readonly EmailService _emailService;

        public AuthController(UserService userService, EscalationsDbContext context, EmailService emailService)
        {
            _userService = userService;
            _context = context;
            _emailService = emailService;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult ForgotPassword()
        {
            return View("~/Views/Auth/ForgotPassword.cshtml");
        }

        // GET RESET
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            ViewBag.Token = token;
            return View();
        }

        // POST RESET
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string token, string newPassword)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(newPassword))
            {
                ModelState.AddModelError("", "Invalid request");
                return View();
            }

            // VALIDACIÓN DE SEGURIDAD DE PASSWORD
            if (newPassword.Length < 8 ||
                !newPassword.Any(char.IsUpper) ||
                !newPassword.Any(char.IsLower) ||
                !newPassword.Any(char.IsDigit))
            {
                ModelState.AddModelError("", "Password must be at least 8 characters and include uppercase, lowercase and a number.");
                return View();
            }

            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.ResetToken == token && u.ResetTokenExpiry > DateTime.UtcNow);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid or expired token");
                return View();
            }

            //user.Password = newPassword;
            var hasher = new PasswordHasher<AppUser>();
            user.Password = hasher.HashPassword(user, newPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;

            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        // FORGOT PASSWORD
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Email is required");
                return View();
            }

            var token = await _userService.GeneratePasswordResetToken(email);

            if (token != null)
            {
                var resetLink = Url.Action(
                    "ResetPassword",
                    "Auth",
                    new { token = token },
                    Request.Scheme
                );

                // NUEVO: SOLO PASAMOS EL LINK
                await _emailService.SendEmail(
                    email,
                    "Reset your password - Escalation Analysis",
                    resetLink
                );
            }

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        // CONFIRMATION PAGE
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // LOGIN
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userService.ValidateUser(model.Email, model.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid credentials");
                return View(model);
            }

            var claims = new List<Claim>
            {
                // USAMOS EMAIL PARA MOSTRAR NOMBRE BONITO
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Dashboard");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}