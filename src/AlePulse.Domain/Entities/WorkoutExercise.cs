using System;
using System.Collections.Generic;
using System.Text;

namespace AlePulse.Domain.Entities;

public class WorkoutExercise : BaseEntity
{
    public Guid WorkoutId { get; set; }
    public Workout Workout { get; set; } = null!;

    public Guid ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;

    public int Order { get; set; }
    public int Sets { get; set; }
    public int Repetitions { get; set; }
    public decimal Weight { get; set; }
    public int RestSeconds { get; set; }
    public string? Notes { get; set; }
}