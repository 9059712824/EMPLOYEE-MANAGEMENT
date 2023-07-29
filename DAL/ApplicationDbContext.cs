using EMPLOYEE_MANAGEMENT.DTO;
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

        public DbSet<Department> Departments { get; set; }

        public DbSet<AcademicDetails> AcademicDetails { get; set; }

        public DbSet<Experience> Experience { get; set; }

        public DbSet<EmployeeDetails> employeeDetails{ get; set; }

        public DbSet<AcademicDetailsView> academicDetailsViews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.UserDetails)
                .WithOne(up => up.User)
                .HasForeignKey<UserDetails>(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Ensure cascaded deletion of UserDetails when User is deleted

            modelBuilder.Entity<UserDetails>()
                .HasKey(up => up.Id);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<AcademicDetails>()
                .HasKey(up => up.Id);

            modelBuilder.Entity<AcademicDetails>()
                .HasOne(ad => ad.User)
                .WithOne(u => u.AcademicDetails)
                .HasForeignKey<AcademicDetails>(ad => ad.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Experience>()
                .HasKey(up => up.Id);

            modelBuilder.Entity<Experience>()
                .HasOne(ex => ex.User)
                .WithOne(u => u.Experience)
                .HasForeignKey<Experience>(ex => ex.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeDetails>()
                .ToView("EmployeeDetails");
            modelBuilder.Entity<EmployeeDetails>().HasNoKey();

            modelBuilder.Entity<AcademicDetailsView>()
                .ToView("academicDetailsView");
            modelBuilder.Entity<AcademicDetailsView>().HasNoKey();

        }

    }
}
