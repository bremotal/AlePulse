using System;
using System.Collections.Generic;
using System.Text;

namespace AlePulse.Domain.Entities;

public class Exercise : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string PrimaryMuscleGroup { get; set; } = string.Empty;
    public string SecondaryMuscleGroup { get; set; } = string.Empty;
    public string Equipment { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty; // Iniciante, Intermediário, Avançado

    public string Instructions { get; set; } = string.Empty;
    public string Breathing { get; set; } = string.Empty;
    public string CommonMistakes { get; set; } = string.Empty;
    public string Tips { get; set; } = string.Empty;

    // Define se é um exercício oficial da biblioteca AlePulse ou criado pelo usuário
    public bool IsOfficial { get; set; } = false;

    // Relacionamento 1 para N
    public ICollection<ExerciseMedia> Medias { get; set; } = new List<ExerciseMedia>();
}