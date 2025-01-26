using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Login
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string databasePath = Path.Combine(Environment.CurrentDirectory, "users.db");
            optionsBuilder.UseSqlite($"Data source={databasePath}");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(u => u.VerificationCode)
                .IsRequired(false);
        }
    }
}
