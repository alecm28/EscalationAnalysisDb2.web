using EscalationAnalysisDb2.Domain.Entities;
using EscalationAnalysisDb2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace EscalationAnalysisDb2.Application.Services
{
    public class UserService
    {
        private readonly EscalationsDbContext _context;

        // 🔐 HASHER
        private readonly PasswordHasher<AppUser> _passwordHasher = new PasswordHasher<AppUser>();

        public UserService(EscalationsDbContext context)
        {
            _context = context;
        }

        // 🔐 LOGIN SEGURO
        public async Task<AppUser?> ValidateUser(string username, string password)
        {
            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Email == username && u.IsActive);

            if (user == null)
                return null;

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);

            if (result == PasswordVerificationResult.Success)
                return user;

            return null;
        }

        // 🔑 RESET TOKEN
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

        public async Task<List<AppUser>> GetAllUsers()
        {
            return await _context.AppUsers.ToListAsync();
        }

        public async Task CreateAdmin()
        {
            var user = new AppUser
            {
                Username = "admin",
                Role = "Admin",
                IsActive = true
            };

            user.Password = _passwordHasher.HashPassword(user, "Admin123!");

            _context.AppUsers.Add(user);
            await _context.SaveChangesAsync();
        }


        // CREATE CON HASH
        public async Task CreateUser(AppUser user)
        {
            user.Password = _passwordHasher.HashPassword(user, user.Password);

            _context.AppUsers.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<AppUser?> GetUserById(int id)
        {
            return await _context.AppUsers.FindAsync(id);
        }

        public async Task UpdateUser(AppUser user)
        {
            _context.AppUsers.Update(user);
            await _context.SaveChangesAsync();
        }

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

