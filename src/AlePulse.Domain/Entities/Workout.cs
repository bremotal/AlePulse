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
    public ICollection<WorkoutExercise> Exercises { get; set; } = new List<WorkoutExercise>();
    public Guid WorkoutProgramId { get; set; }
    public WorkoutProgram WorkoutProgram { get; set; } = null!;
}