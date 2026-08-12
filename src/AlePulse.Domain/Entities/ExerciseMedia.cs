using System;
using System.Collections.Generic;
using System.Text;
using AlePulse.Domain.Enums;

namespace AlePulse.Domain.Entities;

public class ExerciseMedia : BaseEntity
{
    public Guid ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;

    public MediaType MediaType { get; set; }

    // Preferência de demonstração (Masculina, Feminina, Neutra)
    public ExerciseRepresentation Representation { get; set; } = ExerciseRepresentation.Neutral;

    public string Url { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public int? Duration { get; set; } // Para vídeos, em segundos
    public int SortOrder { get; set; } // Para ordenar as mídias na tela
}