namespace AlePulse.Application.DTOs;

public class UpdateProfileDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal? Weight { get; set; }
    public decimal? Height { get; set; }
    public string? TrainingGoal { get; set; }
}