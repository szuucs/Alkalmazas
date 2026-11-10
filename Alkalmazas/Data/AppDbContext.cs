using Microsoft.EntityFrameworkCore;
using Models;

namespace Alkalmazas.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<CatGirl> CatGirls { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Egy felhasználónév csak egyszer szerepelhet
            modelBuilder.Entity<User>().HasIndex(u => u.Name).IsUnique();

            // Rendelés törlésekor a tételei is törlődnek
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Lines)
                .WithOne(l => l.Order)
                .HasForeignKey(l => l.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
