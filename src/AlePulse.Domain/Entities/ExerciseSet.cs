using System;
using System.Collections.Generic;
using System.Text;

namespace AlePulse.Domain.Entities;

public class ExerciseSet : BaseEntity
{
    public Guid WorkoutSessionId { get; set; }
    public WorkoutSession WorkoutSession { get; set; } = null!;

    public Guid ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;

    public int SetNumber { get; set; }
    public decimal Weight { get; set; } // Carga real realizada
    public int Repetitions { get; set; } // Reps reais realizadas
    public int RestSeconds { get; set; } // Descanso realizado

    public DateTime? CompletedAt { get; set; }
    public bool IsCompleted { get; set; } = false;
}