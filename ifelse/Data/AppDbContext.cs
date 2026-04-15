using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ifelse.Data
{
    public class AppDbContext : DbContext
    {
        AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Models.MenuModel> Menu { get; set; }
    }
}
