namespace AlePulse.Application.DTOs;

public class AddWorkoutExerciseDto
{
    public Guid ExerciseId { get; set; }
    public int Sets { get; set; }
    public int Repetitions { get; set; }
    public decimal Weight { get; set; }
    public int RestSeconds { get; set; }
}