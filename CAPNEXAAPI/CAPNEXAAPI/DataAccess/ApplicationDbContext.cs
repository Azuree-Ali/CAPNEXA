using CAPNEXAAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CAPNEXAAPI.DataAccess;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
    public DbSet<SupervisorProfile> SupervisorProfiles => Set<SupervisorProfile>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Milestone> Milestones => Set<Milestone>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        });

        builder.Entity<StudentProfile>(entity =>
        {
            entity.HasIndex(x => x.UserId).IsUnique();
            entity.HasIndex(x => x.StudentNumber).IsUnique();
            entity.Property(x => x.StudentNumber).HasMaxLength(50).IsRequired();
            entity.HasOne(x => x.User).WithOne(x => x.StudentProfile).HasForeignKey<StudentProfile>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<SupervisorProfile>(entity =>
        {
            entity.HasIndex(x => x.UserId).IsUnique();
            entity.HasOne(x => x.User).WithOne(x => x.SupervisorProfile).HasForeignKey<SupervisorProfile>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<Team>(entity =>
        {
            entity.HasIndex(x => x.TeamCode).IsUnique();
            entity.Property(x => x.TeamCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.HasOne(x => x.CreatedByStudent).WithMany(x => x.CreatedTeams).HasForeignKey(x => x.CreatedByStudentId).OnDelete(DeleteBehavior.SetNull);
        });
        builder.Entity<TeamMember>(entity =>
        {
            entity.HasIndex(x => new { x.TeamId, x.StudentId }).IsUnique();
            entity.HasOne(x => x.Team).WithMany(x => x.Members).HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Student).WithMany(x => x.TeamMemberships).HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<Project>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(250).IsRequired();
            entity.HasOne(x => x.Team).WithMany(x => x.Projects).HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Supervisor).WithMany(x => x.Projects).HasForeignKey(x => x.SupervisorId).OnDelete(DeleteBehavior.SetNull);
        });
        builder.Entity<Milestone>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(250).IsRequired();
            entity.HasOne(x => x.Project).WithMany(x => x.Milestones).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<Submission>(entity =>
        {
            entity.HasOne(x => x.Milestone).WithMany(x => x.Submissions).HasForeignKey(x => x.MilestoneId).OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<Review>(entity =>
        {
            entity.Property(x => x.Grade).HasPrecision(5, 2);
            entity.HasOne(x => x.Submission).WithMany(x => x.Reviews).HasForeignKey(x => x.SubmissionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Supervisor).WithMany(x => x.Reviews).HasForeignKey(x => x.SupervisorId).OnDelete(DeleteBehavior.SetNull);
        });
        builder.Entity<Notification>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.HasOne(x => x.User).WithMany(x => x.Notifications).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<AuditLog>(entity =>
        {
            entity.HasOne(x => x.User).WithMany(x => x.AuditLogs).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        });
    }
}
