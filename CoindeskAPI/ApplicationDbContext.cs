using CoindeskAPI.Models;
using Microsoft.EntityFrameworkCore;
namespace CoindeskAPI
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Currency> Currencies { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Currency>()
                .HasKey(c => new { c.Code, c.Language });
        }
    }
}