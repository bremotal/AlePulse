namespace AlePulse.Mobile.Models;

public class WorkoutDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<WorkoutExerciseDto>? Exercises { get; set; }
}

public class WorkoutExerciseDto
{
    public Guid Id { get; set; }
    public ExerciseDto? Exercise { get; set; }
    public int Sets { get; set; }
    public int Repetitions { get; set; }
    public decimal Weight { get; set; }
    public int RestSeconds { get; set; }
}