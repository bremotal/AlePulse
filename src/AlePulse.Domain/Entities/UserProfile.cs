using System;
using System.Collections.Generic;
using System.Text;
using AlePulse.Domain.Enums;

namespace AlePulse.Domain.Entities;

public class UserProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime BirthDate { get; set; }
    public Sex Sex { get; set; }

    public decimal Height { get; set; }
    public decimal Weight { get; set; }

    public string ExperienceLevel { get; set; } = string.Empty;
    public string TrainingGoal { get; set; } = string.Empty;

    public int TrainingFrequency { get; set; }

    public ExerciseRepresentation ExerciseRepresentationPreference { get; set; } = ExerciseRepresentation.Neutral;

    public string? AvatarUrl { get; set; }
}