using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using wep_programlama_odev.Models;

namespace wep_programlama_odev.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Project> Projects => Set<Project>();
        public DbSet<TaskItem> TaskItems => Set<TaskItem>();

        // ✅ Yeni
        public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
        public DbSet<TaskComment> TaskComments => Set<TaskComment>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ProjectMember ilişkileri
            builder.Entity<ProjectMember>()
                .HasOne(pm => pm.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProjectMember>()
                .HasOne(pm => pm.User)
                .WithMany()
                .HasForeignKey(pm => pm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // TaskItem - AssignedUser ilişkisi
            builder.Entity<TaskItem>()
                .HasOne(t => t.AssignedUser)
                .WithMany()
                .HasForeignKey(t => t.AssignedUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // TaskComment ilişkileri
            builder.Entity<TaskComment>()
                .HasOne(c => c.TaskItem)
                .WithMany()
                .HasForeignKey(c => c.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TaskComment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
