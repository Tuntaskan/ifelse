using Microsoft.EntityFrameworkCore;
using ifelse.Models;

namespace ifelse.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Models.User> Users { get; set; }
        public DbSet<Models.MenuModel> Menus { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
    }
}