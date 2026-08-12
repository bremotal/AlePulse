using System;
using System.Collections.Generic;
using System.Text;

namespace AlePulse.Application.DTOs;

public class CreateExerciseDto
{
    public string Name { get; set; } = string.Empty;
    public string PrimaryMuscleGroup { get; set; } = string.Empty;
    public string SecondaryMuscleGroup { get; set; } = string.Empty;
    public string Equipment { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
}