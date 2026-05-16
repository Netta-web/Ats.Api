using Ats.Api.Enums;
using Ats.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ats.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<ApplicationNote> ApplicationNotes => Set<ApplicationNote>();
    public DbSet<StageHistory> StageHistories => Set<StageHistory>();
    public DbSet<ApplicationScore> ApplicationScores => Set<ApplicationScore>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TeamMember>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired();
            e.Property(x => x.Email).IsRequired();
            e.Property(x => x.Role).HasConversion<string>().IsRequired();
        });

        modelBuilder.Entity<Job>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired();
            e.Property(x => x.Description).IsRequired();
            e.Property(x => x.Location).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().IsRequired();
        });

        modelBuilder.Entity<Application>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.CandidateName).IsRequired();
            e.Property(x => x.CandidateEmail).IsRequired();
            e.Property(x => x.CoverLetter).IsRequired();
            e.Property(x => x.CurrentStage).HasConversion<string>().IsRequired();

            // Prevents same candidate from applying to same job twice
            e.HasIndex(x => new { x.JobId, x.CandidateEmail }).IsUnique();

            e.HasOne(x => x.Job)
                .WithMany(x => x.Applications)
                .HasForeignKey(x => x.JobId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApplicationNote>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Description).IsRequired();
            e.Property(x => x.Type).HasConversion<string>().IsRequired();

            e.HasIndex(x => x.ApplicationId);

            e.HasOne(x => x.Application)
                .WithMany(x => x.Notes)
                .HasForeignKey(x => x.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => x.CreatedByTeamMemberId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StageHistory>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FromStage).HasConversion<string>().IsRequired();
            e.Property(x => x.ToStage).HasConversion<string>().IsRequired();
            e.Property(x => x.Reason).IsRequired();

            e.HasIndex(x => x.ApplicationId);

            e.HasOne(x => x.Application)
                .WithMany(x => x.StageHistories)
                .HasForeignKey(x => x.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.ChangedBy)
                .WithMany()
                .HasForeignKey(x => x.ChangedByTeamMemberId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ApplicationScore>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Dimension).HasConversion<string>().IsRequired();
            e.Property(x => x.Comment).IsRequired();

            // One row per dimension per application
            e.HasIndex(x => new { x.ApplicationId, x.Dimension }).IsUnique();

            e.HasOne(x => x.Application)
                .WithMany(x => x.Scores)
                .HasForeignKey(x => x.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.UpdatedBy)
                .WithMany()
                .HasForeignKey(x => x.UpdatedByTeamMemberId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
