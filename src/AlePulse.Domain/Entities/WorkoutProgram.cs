namespace AlePulse.Domain.Entities;

public class WorkoutProgram : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Name { get; set; } = string.Empty; // Ex: "Hipertrofia ABC"
    public string? Description { get; set; }

    // Um Programa tem vários Treinos (A, B, C)
    public ICollection<Workout> Workouts { get; set; } = new List<Workout>();
}