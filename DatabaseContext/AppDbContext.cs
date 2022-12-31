using Microsoft.EntityFrameworkCore;
using LOi.Models;

namespace LOi.DatabaseContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {


        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
        }

        public DbSet<User> UserDataTable { get; set; }
        public DbSet<Order> OrderTable { get; set; }
        public DbSet<Admin> AdminDataTable { get; set; }


    }
}