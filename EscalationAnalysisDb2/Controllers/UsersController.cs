using EscalationAnalysisDb2.Application.Services;
using EscalationAnalysisDb2.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;

namespace EscalationAnalysisDb2.Controllers
{
    // solo admin puede manejar usuarios
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

        // lista usuarios
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsers();
            return View(users);
        }

        // formulario crear usuario
        public IActionResult Create()
        {
            var model = new AppUser
            {
                IsActive = true,
                Role = "User"
            };

            return View(model);
        }

        // guardar usuario nuevo
        [HttpPost]
        public async Task<IActionResult> Create(AppUser user)
        {
            // limpia correo
            user.Email = user.Email?.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(user.Email))
                ModelState.AddModelError("", "Email is required.");

            if (string.IsNullOrWhiteSpace(user.Password))
                ModelState.AddModelError("", "Password is required.");

            // valida formato correo
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

                if (!emailRegex.IsMatch(user.Email))
                {
                    ModelState.AddModelError("", "Please enter a valid email address.");
                }
            }

            // valida rol permitido
            if (user.Role != "Admin" && user.Role != "User")
                ModelState.AddModelError("", "Invalid role selected.");

            var users = await _userService.GetAllUsers();

            // evita correos repetidos
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                if (users.Any(x => x.Email.ToLower() == user.Email))
                {
                    ModelState.AddModelError("", "This email is already registered.");
                }
            }

            // valida seguridad password
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

            // crea username automatico
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

        // formulario editar
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

        // guardar cambios
        [HttpPost]
        public async Task<IActionResult> Edit(AppUser user, string NewPassword)
        {
            // evita validar campos no usados aqui
            ModelState.Remove("Password");
            ModelState.Remove("Username");

            user.Email = user.Email?.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(user.Email))
                ModelState.AddModelError("", "Email is required.");

            // valida formato correo
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

                if (!emailRegex.IsMatch(user.Email))
                {
                    ModelState.AddModelError("", "Please enter a valid email address.");
                }
            }

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

            // valida email duplicado
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

            // actualiza username desde correo
            if (!string.IsNullOrWhiteSpace(user.Email) &&
                user.Email.Contains("@"))
            {
                existingUser.Username = user.Email.Split('@')[0];
            }

            // password nueva opcional
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

        // desactivar usuario
        public async Task<IActionResult> Deactivate(int id)
        {
            var user = await _userService.GetUserById(id);

            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Index");
            }

            // no permite desactivarse solo
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