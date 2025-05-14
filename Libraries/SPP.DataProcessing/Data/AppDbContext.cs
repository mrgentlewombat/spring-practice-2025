using Microsoft.EntityFrameworkCore;
using SPP.DataProcessing.Models;

namespace SPP.DataProcessing.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Region> Regions { get; set; }
    }
}
