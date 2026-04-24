using EscalationAnalysisDb2.Domain.Entities;
using EscalationAnalysisDb2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace EscalationAnalysisDb2.Application.Services
{
    public class UserService
    {
        // acceso a bd
        private readonly EscalationsDbContext _context;

        // se usa para encriptar y validar passwords
        private readonly PasswordHasher<AppUser> _passwordHasher;

        public UserService(EscalationsDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<AppUser>();
        }

        // valida login del usuario
        public async Task<AppUser?> ValidateUser(string email, string password)
        {
            // limpia formato del correo
            email = email?.Trim().ToLower();

            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

            // no existe
            if (user == null)
                return null;

            // usuario desactivado
            if (!user.IsActive)
                return null;

            // bloqueado temporalmente
            if (user.LockoutEnd != null &&
                user.LockoutEnd > DateTime.UtcNow)
            {
                return null;
            }

            try
            {
                // compara password encriptado
                var result = _passwordHasher
                    .VerifyHashedPassword(user, user.Password, password);

                if (result == PasswordVerificationResult.Success)
                {
                    // limpia intentos fallidos
                    user.FailedLoginAttempts = 0;
                    user.LockoutEnd = null;

                    await _context.SaveChangesAsync();

                    return user;
                }
            }
            catch
            {
                // soporte para passwords viejos en texto plano
                if (user.Password == password)
                {
                    // lo convierte a hash
                    user.Password =
                        _passwordHasher.HashPassword(user, password);

                    user.FailedLoginAttempts = 0;
                    user.LockoutEnd = null;

                    await _context.SaveChangesAsync();

                    return user;
                }
            }

            // suma intento fallido
            user.FailedLoginAttempts++;

            // despues de 5 intentos bloquea 15 min
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                user.FailedLoginAttempts = 0;
            }

            await _context.SaveChangesAsync();

            return null;
        }

        // crea token para reset password
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

        // lista todos los usuarios
        public async Task<List<AppUser>> GetAllUsers()
        {
            return await _context.AppUsers.ToListAsync();
        }

        // crea usuario nuevo
        public async Task CreateUser(AppUser user)
        {
            user.Email = user.Email.Trim().ToLower();

            // deja fijo el username admin
            if (user.Email == "escalationanalysis@gmail.com")
            {
                user.Username = "admin";
            }

            // guarda password encriptado
            user.Password =
                _passwordHasher.HashPassword(user, user.Password);

            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;

            _context.AppUsers.Add(user);

            await _context.SaveChangesAsync();
        }

        // busca usuario por id
        public async Task<AppUser?> GetUserById(int id)
        {
            return await _context.AppUsers.FindAsync(id);
        }

        // actualiza datos del usuario
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

        // desactiva usuario
        public async Task DeactivateUser(int id)
        {
            var user = await _context.AppUsers.FindAsync(id);

            if (user == null)
                return;

            // no deja desactivar admin principal
            if (user.Email.ToLower() == "escalationanalysis@gmail.com")
                return;

            // evita dejar sistema sin admins
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