using backend.Data;
using backend.Helpers;
using backend.Interfaces;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;

namespace backend.Repositories;

public class UserRepository: BaseRepository<User>, IUserRepository
{
    private readonly DataContext context;

    public UserRepository(DataContext _context, IConfiguration config, IHostEnvironment env) : base(_context)
    {
        this.context = _context;

        Task.WaitAny(Task.Run(async () =>
        {
            if (!env.IsDevelopment())
                return;

            var adminPassword = config.GetValue<string>("AdminPassword");
            if (adminPassword is null)
            {
                throw new Exception("Admin password not set in configuration.");
            }

            var hashedPassword = PasswordHasher.HashPassword(adminPassword);

            var adminAcc = await GetByNameAsync("admin");

            if (adminAcc is null)
            {
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "admin",
                    Email = "admin@simple-blog.invalid",
                    CreatedAt = DateTime.UtcNow,
                    PasswordHash = hashedPassword,
                };

                await CreateAsync(user);
                await context.SaveChangesAsync();
            }
            else
            {
                var user = await GetByNameAsync("admin");

                if (user != null && user.PasswordHash != hashedPassword)
                {
                    user.PasswordHash = hashedPassword;
                    await UpdateAsync(user.Id, user);
                    await context.SaveChangesAsync();
                }
                else
                {
                    Console.WriteLine("Admin user already up to date.");
                }
            }
        }));
    }

    public async Task<User?> GetByNameAsync(string username)
    {
        return await _dbSet
            .Where(u => u.Username == username)
            .FirstOrDefaultAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .Where(u => u.Email == email)
            .FirstOrDefaultAsync();
    }

    public async Task MuteUser(Guid muter, Guid mutee)
    {
        await context.UserMutes.AddAsync(
            new Models.MuteUser
        {
            UserId = muter,
            MutedUserId = mutee
        });
    }
}


