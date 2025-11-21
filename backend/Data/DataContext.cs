using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace backend.Data;

public class DataContext(
    DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Post> Posts { get; set; } = null!;
    public DbSet<MuteUser> UserMutes { get; set; } = null!;

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<TimedModel>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // === Constraints ===

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        // === Relationships === 
        // ==> See docs/models for relationship diagram (model.)

        modelBuilder.Entity<Post>()
            .HasOne(p => p.User)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MuteUser>()
            .HasOne(u => u.User)
            .WithMany(mu => mu.MutedUsers)
            .HasForeignKey(mu => mu.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MuteUser>()
            .HasOne(mu => mu.MutedUser)
            .WithMany()
            .HasForeignKey(mu => mu.MutedUserId);
    }
}
