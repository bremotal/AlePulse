using AlePulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlePulse.Infrastructure.Persistence;

public class AlePulseDbContext : DbContext
{
    public AlePulseDbContext(DbContextOptions<AlePulseDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }

    public DbSet<Exercise> Exercises { get; set; }
    public DbSet<ExerciseMedia> ExerciseMedias { get; set; }

    public DbSet<WorkoutProgram> WorkoutPrograms { get; set; }
    public DbSet<Workout> Workouts { get; set; }
    public DbSet<WorkoutExercise> WorkoutExercises { get; set; }

    public DbSet<WorkoutSession> WorkoutSessions { get; set; }
    public DbSet<ExerciseSet> ExerciseSets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Profile)
            .WithOne(p => p.User)
            .HasForeignKey<UserProfile>(p => p.UserId);

        modelBuilder.Entity<Exercise>()
            .HasMany(e => e.Medias)
            .WithOne(m => m.Exercise)
            .HasForeignKey(m => m.ExerciseId);

        modelBuilder.Entity<WorkoutProgram>()
            .HasMany(p => p.Workouts)
            .WithOne(w => w.WorkoutProgram)
            .HasForeignKey(w => w.WorkoutProgramId);

        modelBuilder.Entity<Workout>()
            .HasMany(w => w.Exercises)
            .WithOne(we => we.Workout)
            .HasForeignKey(we => we.WorkoutId);

        modelBuilder.Entity<WorkoutSession>()
            .HasMany(s => s.Sets)
            .WithOne(es => es.WorkoutSession)
            .HasForeignKey(es => es.WorkoutSessionId);
    }
}