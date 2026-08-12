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

    public decimal Height { get; set; } // Em centímetros
    public decimal Weight { get; set; } // Em kg

    public string ExperienceLevel { get; set; } = string.Empty; // Iniciante, Intermediário, Avançado
    public string TrainingGoal { get; set; } = string.Empty; // Hipertrofia, Força, etc.

    public int TrainingFrequency { get; set; } // Dias por semana

    // Preferência de visualização da biblioteca
    public ExerciseRepresentation ExerciseRepresentationPreference { get; set; } = ExerciseRepresentation.Neutral;

    // Avatar/Foto (Opcional)
    public string? AvatarUrl { get; set; }
}