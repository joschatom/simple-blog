using backend.Data;
using backend.Helpers;
using backend.Interfaces;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;

namespace backend.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    private readonly DataContext context;

    public UserRepository(DataContext _context, IConfiguration config, IHostEnvironment env) : base(_context)
    {
        this.context = _context;

        Task.WaitAny(Task.Run(async () =>
        {
            if (!env.IsDevelopment())
                return;

            var adminPassword = config.GetValue<string>("AdminPassword")
                ?? throw new Exception("Admin password not set in configuration.");

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
                await SaveChangesAsync();
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

    public async Task<IEnumerable<Post>> GetPosts(Guid id)
    {
        var user = await GetByIdAsync(id)
            ?? throw new KeyNotFoundException(id.ToString());

        var posts = context.Entry(user)
            .Collection(p => p.Posts)
            .Query();

        await posts.LoadAsync();

        return posts;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MuteUser>> GetMutedUsers(Guid id)
    {
        var user = await GetByIdAsync(id)
            ?? throw new KeyNotFoundException(id.ToString());

        var posts = context.Entry(user)
            .Collection(p => p.MutedUsers)
            .Query();

        await posts.LoadAsync();

        return posts;
    }

    public async Task<bool> UnmuteUser(Guid muter, Guid mutee)
    {
        var record = await context.UserMutes.FindAsync(muter, mutee);

        if (record == null) return false;

        context.UserMutes.Remove(record);


        return true;
    }

    public async Task<bool> IsMuted(Guid muter, Guid mutee)
        => await context.UserMutes.AnyAsync(m => m.UserId == muter
        && m.MutedUserId == mutee);

    public  bool IsMutedBlocking(Guid muter, Guid mutee)
        => context.UserMutes.Any(m => m.UserId == muter
        && m.MutedUserId == mutee);
}



