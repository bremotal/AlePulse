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

    public int Order { get; set; } // Ordem do exercício na ficha
    public int Sets { get; set; } // Séries planejadas
    public int Repetitions { get; set; } // Repetições planejadas
    public decimal Weight { get; set; } // Carga planejada
    public int RestSeconds { get; set; } // Descanso planejado
    public string? Notes { get; set; }
}