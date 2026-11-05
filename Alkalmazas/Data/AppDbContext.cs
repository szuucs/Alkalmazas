using Microsoft.EntityFrameworkCore;
using Models;

namespace Alkalmazas.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<CatGirl> CatGirls { get; set; }
        public DbSet<User> Users { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(u => u.Name).IsUnique();
        }
    }
}
