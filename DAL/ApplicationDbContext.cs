using EMPLOYEE_MANAGEMENT.Models;
using Microsoft.EntityFrameworkCore;

namespace EMPLOYEE_MANAGEMENT.DAL
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserDetails> UserDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.UserDetails)
                .WithOne(up => up.User)
                .HasForeignKey<UserDetails>(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Ensure cascaded deletion of UserDetails when User is deleted

            modelBuilder.Entity<UserDetails>()
                .HasKey(up => up.UserId);

            modelBuilder.Entity<UserDetails>()
                .HasOne(up => up.User)
                .WithOne(u => u.UserDetails)
                .HasForeignKey<UserDetails>(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Ensure cascaded deletion of User when UserDetails is deleted
        }
    }
}
