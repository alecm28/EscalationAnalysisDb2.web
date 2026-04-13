using EscalationAnalysisDb2.Application.Services;
using EscalationAnalysisDb2.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EscalationAnalysisDb2.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        // LISTA DE USUARIOS
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsers();
            return View(users);
        }

        // CREAR USUARIO (GET)
        public IActionResult Create()
        {
            return View();
        }

        // CREAR USUARIO (POST)
        [HttpPost]
        public async Task<IActionResult> Create(AppUser user)
        {
            if (!ModelState.IsValid)
                return View(user);

            await _userService.CreateUser(user);
            return RedirectToAction("Index");
        }

        // EDITAR (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userService.GetUserById(id);
            if (user == null) return NotFound();

            return View(user);
        }

        // EDITAR (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(AppUser user)
        {
            if (!ModelState.IsValid)
                return View(user);

            await _userService.UpdateUser(user);
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