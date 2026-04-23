using EscalationAnalysisDb2.Application.Services;
using EscalationAnalysisDb2.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace EscalationAnalysisDb2.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserService _userService;
        private readonly PasswordHasher<AppUser> _passwordHasher;

        public UsersController(UserService userService)
        {
            _userService = userService;
            _passwordHasher = new PasswordHasher<AppUser>();
        }

        // LISTA DE USUARIOS
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsers();
            return View(users);
        }

        // CREATE GET
        public IActionResult Create()
        {
            var model = new AppUser
            {
                IsActive = true,
                Role = "User"
            };

            return View(model);
        }

        // CREATE POST
        [HttpPost]
        public async Task<IActionResult> Create(AppUser user)
        {
            // LIMPIAR EMAIL
            user.Email = user.Email?.Trim().ToLower();

            // EMAIL REQUERIDO
            if (string.IsNullOrWhiteSpace(user.Email))
                ModelState.AddModelError("", "Email is required.");

            // PASSWORD REQUERIDA
            if (string.IsNullOrWhiteSpace(user.Password))
                ModelState.AddModelError("", "Password is required.");

            // ROLE VÁLIDO
            if (user.Role != "Admin" && user.Role != "User")
                ModelState.AddModelError("", "Invalid role selected.");

            // VALIDAR EMAIL DUPLICADO
            var users = await _userService.GetAllUsers();

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                if (users.Any(x => x.Email.ToLower() == user.Email))
                {
                    ModelState.AddModelError("", "This email is already registered.");
                }
            }

            // VALIDAR PASSWORD
            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                if (user.Password.Length < 8 ||
                    !user.Password.Any(char.IsUpper) ||
                    !user.Password.Any(char.IsLower) ||
                    !user.Password.Any(char.IsDigit))
                {
                    ModelState.AddModelError("",
                        "Password must be at least 8 characters and include uppercase, lowercase and a number.");
                }
            }

            // USERNAME AUTOMÁTICO
            if (!string.IsNullOrWhiteSpace(user.Email) &&
                user.Email.Contains("@"))
            {
                var baseUsername = user.Email.Split('@')[0];
                var username = baseUsername;
                int counter = 1;

                while (users.Any(x => x.Username == username))
                {
                    username = baseUsername + counter;
                    counter++;
                }

                user.Username = username;
            }

            if (!ModelState.IsValid)
                return View(user);

            user.IsActive = true;

            await _userService.CreateUser(user);

            TempData["Success"] = "User created successfully.";

            return RedirectToAction("Index");
        }

        // EDIT GET
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userService.GetUserById(id);

            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // EDIT POST
        [HttpPost]
        public async Task<IActionResult> Edit(AppUser user, string NewPassword)
        {
            ModelState.Remove("Password");
            ModelState.Remove("Username");

            user.Email = user.Email?.Trim().ToLower();

            if (user.Role != "Admin" && user.Role != "User")
                ModelState.AddModelError("", "Invalid role selected.");

            if (!ModelState.IsValid)
                return View(user);

            var existingUser = await _userService.GetUserById(user.AppUserId);

            if (existingUser == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Index");
            }

            var users = await _userService.GetAllUsers();

            if (users.Any(x =>
                x.AppUserId != user.AppUserId &&
                x.Email.ToLower() == user.Email))
            {
                ModelState.AddModelError("", "This email is already registered.");
                return View(user);
            }

            existingUser.Email = user.Email;
            existingUser.Role = user.Role;
            existingUser.IsActive = user.IsActive;

            if (!string.IsNullOrWhiteSpace(user.Email) &&
                user.Email.Contains("@"))
            {
                existingUser.Username = user.Email.Split('@')[0];
            }

            // PASSWORD OPCIONAL
            if (!string.IsNullOrWhiteSpace(NewPassword))
            {
                if (NewPassword.Length < 8 ||
                    !NewPassword.Any(char.IsUpper) ||
                    !NewPassword.Any(char.IsLower) ||
                    !NewPassword.Any(char.IsDigit))
                {
                    ModelState.AddModelError("",
                        "Password must be at least 8 characters and include uppercase, lowercase and a number.");

                    return View(user);
                }

                existingUser.Password =
                    _passwordHasher.HashPassword(existingUser, NewPassword);
            }

            await _userService.UpdateUser(existingUser);

            TempData["Success"] = "User updated successfully.";

            return RedirectToAction("Index");
        }

        // DEACTIVATE USER
        public async Task<IActionResult> Deactivate(int id)
        {
            var user = await _userService.GetUserById(id);

            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Index");
            }

            // NO DESACTIVARSE A SÍ MISMO
            if (User.Identity.Name.ToLower() == user.Email.ToLower())
            {
                TempData["Error"] = "You cannot deactivate your own account.";
                return RedirectToAction("Index");
            }

            var wasActive = user.IsActive;

            await _userService.DeactivateUser(id);

            var updatedUser = await _userService.GetUserById(id);

            if (wasActive && updatedUser.IsActive == false)
            {
                TempData["Success"] = "User deactivated successfully.";
            }
            else
            {
                TempData["Error"] = "User could not be deactivated.";
            }

            return RedirectToAction("Index");
        }
    }
}