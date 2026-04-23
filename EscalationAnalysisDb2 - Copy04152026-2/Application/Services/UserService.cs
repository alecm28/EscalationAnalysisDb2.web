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

        //LOGIN (USA EMAIL)
        public async Task<AppUser?> ValidateUser(string email, string password)
        {
            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

            if (user == null)
                return null;

            try
            {
                var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);

                if (result == PasswordVerificationResult.Success)
                    return user;
            }
            catch
            {
                // fallback para passwords en texto plano (usuarios viejos)
                if (user.Password == password)
                {
                    // convertir automáticamente a hash
                    user.Password = _passwordHasher.HashPassword(user, password);
                    await _context.SaveChangesAsync();

                    return user;
                }
            }

            return null;
        }

        // GENERAR TOKEN RESET
        public async Task<string?> GeneratePasswordResetToken(string email)
        {
            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

            if (user == null)
                return null;

            var token = Guid.NewGuid().ToString();

            user.ResetToken = token;
            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);

            await _context.SaveChangesAsync();

            return token;
        }

        // LISTA DE USUARIOS
        public async Task<List<AppUser>> GetAllUsers()
        {
            return await _context.AppUsers.ToListAsync();
        }

        // CREAR USUARIO (CON HASH)
        public async Task CreateUser(AppUser user)
        {
            user.Password = _passwordHasher.HashPassword(user, user.Password);

            _context.AppUsers.Add(user);
            await _context.SaveChangesAsync();
        }

        // OBTENER POR ID
        public async Task<AppUser?> GetUserById(int id)
        {
            return await _context.AppUsers.FindAsync(id);
        }

        // ✏ACTUALIZAR USUARIO
        public async Task UpdateUser(AppUser user)
        {
            _context.AppUsers.Update(user);
            await _context.SaveChangesAsync();
        }

        // DESACTIVAR
        public async Task DeactivateUser(int id)
        {
            var user = await _context.AppUsers.FindAsync(id);

            if (user != null)
            {
                user.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }
    }
}