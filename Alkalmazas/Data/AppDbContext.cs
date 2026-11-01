using Microsoft.EntityFrameworkCore;
using Models;

namespace Alkalmazas.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<CatGirl> CatGirls { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }
    }
}
