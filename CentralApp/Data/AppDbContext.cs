using Microsoft.EntityFrameworkCore;
using CentralApp.Models;

namespace CentralApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Region> Regions { get; set; }
    }
}
