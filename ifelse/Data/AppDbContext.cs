using Microsoft.EntityFrameworkCore;

namespace ifelse.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Models.User> Users { get; set; }
        public DbSet<Models.MenuModel> Menu { get; set; }
    }
}