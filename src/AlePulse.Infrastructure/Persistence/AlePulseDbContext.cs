using System;
using System.Collections.Generic;
using System.Text;
using AlePulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlePulse.Infrastructure.Persistence;

public class AlePulseDbContext : DbContext
{
    public AlePulseDbContext(DbContextOptions<AlePulseDbContext> options) : base(options)
    {
    }

    // Usuários
    public DbSet<User> Users { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }

    // Exercícios
    public DbSet<Exercise> Exercises { get; set; }
    public DbSet<ExerciseMedia> ExerciseMedias { get; set; }

    // Treinos Planejados
    public DbSet<Workout> Workouts { get; set; }
    public DbSet<WorkoutExercise> WorkoutExercises { get; set; }

    // Treinos Executados (Histórico)
    public DbSet<WorkoutSession> WorkoutSessions { get; set; }
    public DbSet<ExerciseSet> ExerciseSets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração do User e UserProfile (Relação 1 para 1)
        modelBuilder.Entity<User>()
            .HasOne(u => u.Profile)
            .WithOne(p => p.User)
            .HasForeignKey<UserProfile>(p => p.UserId);

        // Configuração do Exercise e ExerciseMedia (Relação 1 para N)
        modelBuilder.Entity<Exercise>()
            .HasMany(e => e.Medias)
            .WithOne(m => m.Exercise)
            .HasForeignKey(m => m.ExerciseId);

        // Configuração do Workout e WorkoutExercise (Relação 1 para N)
        modelBuilder.Entity<Workout>()
            .HasMany(w => w.Exercises)
            .WithOne(we => we.Workout)
            .HasForeignKey(we => we.WorkoutId);

        // Configuração do WorkoutSession e ExerciseSet (Relação 1 para N)
        modelBuilder.Entity<WorkoutSession>()
            .HasMany(s => s.Sets)
            .WithOne(es => es.WorkoutSession)
            .HasForeignKey(es => es.WorkoutSessionId);
    }
}