using System;
using System.Collections.Generic;
using System.Text;
using AlePulse.Domain.Enums;

namespace AlePulse.Domain.Entities;

public class WorkoutSession : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid WorkoutId { get; set; }
    public Workout Workout { get; set; } = null!;

    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.InProgress;
    public string? Notes { get; set; }

    // Relacionamento 1 para N
    public ICollection<ExerciseSet> Sets { get; set; } = new List<ExerciseSet>();
}