using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class DataContext(
    DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
}
