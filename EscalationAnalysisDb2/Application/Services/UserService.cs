using EscalationAnalysisDb2.Domain.Entities;
using EscalationAnalysisDb2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace EscalationAnalysisDb2.Application.Services
{
    public class UserService
    {
        private readonly EscalationsDbContext _context;
        private readonly PasswordHasher<AppUser> _passwordHasher;

        public UserService(EscalationsDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<AppUser>();
        }

        // LOGIN usando correo electrónico
        public async Task<AppUser?> ValidateUser(string email, string password)
        {
            // TRIM + LOWERCASE
            email = email?.Trim().ToLower();

            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

            // usuario no existe
            if (user == null)
                return null;

            // usuario inactivo
            if (!user.IsActive)
                return null;

            // LOCKOUT ACTIVO
            if (user.LockoutEnd != null &&
                user.LockoutEnd > DateTime.UtcNow)
            {
                return null;
            }

            try
            {
                var result = _passwordHasher
                    .VerifyHashedPassword(user, user.Password, password);

                if (result == PasswordVerificationResult.Success)
                {
                    // reset intentos fallidos
                    user.FailedLoginAttempts = 0;
                    user.LockoutEnd = null;

                    await _context.SaveChangesAsync();

                    return user;
                }
            }
            catch
            {
                // soporte usuarios viejos password texto plano
                if (user.Password == password)
                {
                    user.Password =
                        _passwordHasher.HashPassword(user, password);

                    user.FailedLoginAttempts = 0;
                    user.LockoutEnd = null;

                    await _context.SaveChangesAsync();

                    return user;
                }
            }

            // password incorrecta = sumar intento
            user.FailedLoginAttempts++;

            // 5 intentos = bloqueo 15 min
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                user.FailedLoginAttempts = 0;
            }

            await _context.SaveChangesAsync();

            return null;
        }

        // generar token recuperación
        public async Task<string?> GeneratePasswordResetToken(string email)
        {
            email = email?.Trim().ToLower();

            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == email &&
                    u.IsActive);

            if (user == null)
                return null;

            var token = Guid.NewGuid().ToString();

            user.ResetToken = token;
            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);

            await _context.SaveChangesAsync();

            return token;
        }

        // obtener lista usuarios
        public async Task<List<AppUser>> GetAllUsers()
        {
            return await _context.AppUsers.ToListAsync();
        }

        // crear usuario
        public async Task CreateUser(AppUser user)
        {
            user.Email = user.Email.Trim().ToLower();

            if (user.Email == "escalationanalysis@gmail.com")
            {
                user.Username = "admin";
            }

            user.Password =
                _passwordHasher.HashPassword(user, user.Password);

            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;

            _context.AppUsers.Add(user);

            await _context.SaveChangesAsync();
        }

        // buscar por id
        public async Task<AppUser?> GetUserById(int id)
        {
            return await _context.AppUsers.FindAsync(id);
        }

        // actualizar usuario
        public async Task UpdateUser(AppUser user)
        {
            user.Email = user.Email.Trim().ToLower();

            if (user.Email == "escalationanalysis@gmail.com")
            {
                user.Username = "admin";
            }

            _context.AppUsers.Update(user);

            await _context.SaveChangesAsync();
        }

        // desactivar usuario
        public async Task DeactivateUser(int id)
        {
            var user = await _context.AppUsers.FindAsync(id);

            if (user == null)
                return;

            if (user.Email.ToLower() == "escalationanalysis@gmail.com")
                return;

            if (user.Role == "Admin")
            {
                var activeAdmins = await _context.AppUsers
                    .CountAsync(x =>
                        x.Role == "Admin" &&
                        x.IsActive);

                if (activeAdmins <= 1)
                    return;
            }

            user.IsActive = false;

            await _context.SaveChangesAsync();
        }
    }
}