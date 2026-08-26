namespace AlePulse.Mobile.Models;

public class WorkoutProgramDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<WorkoutDto>? Workouts { get; set; } // Adicione esta linha
}