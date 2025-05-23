using Microsoft.EntityFrameworkCore;
using SPP.Domain.Entities;
namespace SPP.Domain.Data;

// Database context used to access tables in the database
public class AppDbContext : DbContext
{
    // Constructor to pass options to the base DbContext
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Table for regions (used to validate region codes)
    public DbSet<Region> Regions { get; set; }
    public DbSet<AgentEntity> Agents { get; set; } = null!;
    }

