using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasco.TaskService.Repository.Entities
{
    public class TaskManagementDbContext : DbContext
    {
        public TaskManagementDbContext(DbContextOptions<TaskManagementDbContext> options)
            : base(options)
        {
        }

        public TaskManagementDbContext()
        {
        }

        public DbSet<WorkArea> WorkAreas { get; set; }
        public DbSet<WorkTask> WorkTasks { get; set; }
        public DbSet<TaskObjective> TaskObjectives { get; set; }
        public DbSet<TaskMember> TaskMembers { get; set; }
        public DbSet<TaskAction> TaskActions { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // WorkArea relationships
            modelBuilder.Entity<WorkArea>()
                .HasMany(wa => wa.WorkTasks)
                .WithOne(wt => wt.WorkArea)
                .HasForeignKey(wt => wt.WorkAreaId)
                .OnDelete(DeleteBehavior.Cascade);

            // WorkTask relationships
            modelBuilder.Entity<WorkTask>()
                .HasMany(wt => wt.TaskObjectives)
                .WithOne(to => to.WorkTask)
                .HasForeignKey(to => to.WorkTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkTask>()
                .HasMany(wt => wt.TaskMembers)
                .WithOne(tm => tm.WorkTask)
                .HasForeignKey(tm => tm.WorkTaskId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<WorkTask>()
                .HasMany(wt => wt.TaskActions)
                .WithOne(ta => ta.WorkTask)
                .HasForeignKey(ta => ta.WorkTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkTask>()
                .HasMany(wt => wt.Comments)
                .WithOne(c => c.WorkTask)
                .HasForeignKey(c => c.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for better query performance
            modelBuilder.Entity<WorkTask>()
                .HasIndex(wt => wt.CreatedByUserId);

            modelBuilder.Entity<WorkTask>()
                .HasIndex(wt => wt.Status);

            modelBuilder.Entity<WorkTask>()
                .HasIndex(wt => wt.Priority);

            modelBuilder.Entity<WorkTask>()
                .HasIndex(wt => wt.DueDate);

            modelBuilder.Entity<TaskMember>()
                .HasIndex(tm => tm.UserId);

            modelBuilder.Entity<TaskMember>()
                .HasIndex(tm => new { tm.WorkTaskId, tm.UserId })
                .IsUnique();

            modelBuilder.Entity<TaskAction>()
                .HasIndex(ta => ta.UserId);

            modelBuilder.Entity<TaskAction>()
                .HasIndex(ta => ta.ActionDate);

            modelBuilder.Entity<Comment>()
                .HasIndex(c => c.UserId);

            modelBuilder.Entity<Comment>()
                .HasIndex(c => c.CreatedAt);
            // Seed data (optional)
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Không seed user data vì user được quản lý ở Auth service
            // Có thể seed một số data khác như default status, priority values
        }

        // Override SaveChanges để tự động thêm TaskAction khi có thay đổi
        public override int SaveChanges()
        {
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }

        // Method để track changes với user context
        public async Task<int> SaveChangesAsync(Guid currentUserId, string currentUserName, CancellationToken cancellationToken = default)
        {
            TrackChanges(currentUserId, currentUserName);
            return await base.SaveChangesAsync(cancellationToken);
        }

        public int SaveChanges(Guid currentUserId, string currentUserName)
        {
            TrackChanges(currentUserId, currentUserName);
            return base.SaveChanges();
        }

        private void TrackChanges(Guid currentUserId, string currentUserName)
        {
            var entries = ChangeTracker.Entries<WorkTask>()
                .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added)
                .ToList();

            foreach (var entry in entries)
            {
                var entity = entry.Entity;
                var actionType = entry.State == EntityState.Added ? "Created" : "Updated";

                // Tạo TaskAction để ghi lại thay đổi
                var taskAction = new TaskAction
                {
                    WorkTaskId = entity.Id,
                    UserId = currentUserId,
                    UserName = currentUserName,
                    ActionType = actionType,
                    Description = $"Task {actionType.ToLower()}",
                    ActionDate = DateTime.UtcNow
                };

                // Nếu là update, ghi lại các field đã thay đổi
                if (entry.State == EntityState.Modified)
                {
                    var changedProperties = entry.Properties
                        .Where(p => p.IsModified)
                        .Select(p => p.Metadata.Name)
                        .ToList();

                    taskAction.Description = $"Task updated: {string.Join(", ", changedProperties)}";
                }

                TaskActions.Add(taskAction);
            }
        }
    }
}