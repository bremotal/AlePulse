namespace AlePulse.Mobile.Models;

public class ExerciseSetDto
{
    public Guid Id { get; set; }
    public int SetNumber { get; set; }
    public decimal Weight { get; set; }
    public int Repetitions { get; set; }
    public DateTime CompletedAt { get; set; }
}