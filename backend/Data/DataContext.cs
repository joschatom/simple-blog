using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class DataContext(
    DbContextOptions<DataContext> options,
    IConfiguration configuration,
    IWebHostEnvironment env
) : DbContext(options)
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IWebHostEnvironment _env = env;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        string? connectionString = _env.IsDevelopment()
            ? _configuration.GetConnectionString("Development")
            : _configuration.GetConnectionString("Production");
        if (connectionString != null)
        {
            optionsBuilder.UseSqlServer(connectionString);
        }
        else
        {
            throw new Exception("there was no connectionstring provided or the provided connectionstring didn't work!!!");
        }
    }

    public DbSet<User> Users { get; set; } = null!;
}
