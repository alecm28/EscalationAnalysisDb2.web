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

        // LISTA
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsers();
            return View(users);
        }

        // CREATE (GET)
        public IActionResult Create()
        {
            var model = new AppUser
            {
                IsActive = true,
                Role = "User"
            };

            return View(model);
        }

        // CREATE (POST)
        [HttpPost]
        public async Task<IActionResult> Create(AppUser user)
        {
            if (string.IsNullOrEmpty(user.Email))
                ModelState.AddModelError("", "Email is required");

            if (string.IsNullOrEmpty(user.Password))
                ModelState.AddModelError("", "Password is required");

            // 🔐 VALIDACIÓN DE PASSWORD
            if (!string.IsNullOrEmpty(user.Password))
            {
                if (user.Password.Length < 8 ||
                    !user.Password.Any(char.IsUpper) ||
                    !user.Password.Any(char.IsLower) ||
                    !user.Password.Any(char.IsDigit))
                {
                    ModelState.AddModelError("", "Password must be at least 8 characters and include uppercase, lowercase and a number.");
                }
            }

            // 🔥 USERNAME AUTOMÁTICO
            if (!string.IsNullOrEmpty(user.Email) && user.Email.Contains("@"))
            {
                var baseUsername = user.Email.Split('@')[0];
                var username = baseUsername;
                int counter = 1;

                var users = await _userService.GetAllUsers();

                while (users.Any(u => u.Username == username))
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

            return RedirectToAction("Index");
        }

        // EDIT (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userService.GetUserById(id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // EDIT (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(AppUser user, string NewPassword)
        {
            // 🔥 IGNORAR VALIDACIONES QUE YA NO USAMOS
            ModelState.Remove("Password");
            ModelState.Remove("Username");

            if (!ModelState.IsValid)
                return View(user);

            var existingUser = await _userService.GetUserById(user.AppUserId);
            if (existingUser == null)
                return NotFound();

            // 🔥 USERNAME AUTOMÁTICO
            if (!string.IsNullOrEmpty(user.Email) && user.Email.Contains("@"))
            {
                existingUser.Username = user.Email.Split('@')[0];
            }

            existingUser.Email = user.Email;
            existingUser.Role = user.Role;
            existingUser.IsActive = user.IsActive;

            // 🔐 PASSWORD OPCIONAL + HASH
            if (!string.IsNullOrEmpty(NewPassword))
            {
                if (NewPassword.Length < 8 ||
                    !NewPassword.Any(char.IsUpper) ||
                    !NewPassword.Any(char.IsLower) ||
                    !NewPassword.Any(char.IsDigit))
                {
                    ModelState.AddModelError("", "Password must be at least 8 characters and include uppercase, lowercase and a number.");
                    return View(user);
                }

                existingUser.Password = _passwordHasher.HashPassword(existingUser, NewPassword);
            }

            await _userService.UpdateUser(existingUser);

            return RedirectToAction("Index");
        }

        // DESACTIVAR
        public async Task<IActionResult> Deactivate(int id)
        {
            await _userService.DeactivateUser(id);
            return RedirectToAction("Index");
        }
    }
}