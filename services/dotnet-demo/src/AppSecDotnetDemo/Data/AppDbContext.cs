using Microsoft.EntityFrameworkCore;

namespace AppSecDotnetDemo.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AssemblyInfo> Assemblies { get; set; } = null!;
}
