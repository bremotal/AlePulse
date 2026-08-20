namespace AlePulse.Mobile.Models;

public class ExerciseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<ExerciseMediaDto>? Medias { get; set; }
}