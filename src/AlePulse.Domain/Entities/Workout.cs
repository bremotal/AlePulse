using System;
using System.Collections.Generic;
using System.Text;

namespace AlePulse.Domain.Entities;

public class Workout : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Name { get; set; } = string.Empty; // Ex: Treino A - Peito e Tríceps
    public string? Description { get; set; }

    // Relacionamento 1 para N
    public ICollection<WorkoutExercise> Exercises { get; set; } = new List<WorkoutExercise>();
}