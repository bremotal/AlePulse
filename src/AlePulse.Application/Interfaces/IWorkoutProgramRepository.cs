using AlePulse.Domain.Entities;

namespace AlePulse.Application.Interfaces;

public interface IWorkoutProgramRepository
{
    Task<IEnumerable<WorkoutProgram>> GetAllByUserIdAsync(Guid userId);
    Task<WorkoutProgram?> GetByIdAsync(Guid id);
    Task AddAsync(WorkoutProgram program);
    Task AddWorkoutAsync(Workout workout);
    Task SaveChangesAsync();
    Task DeleteAsync(WorkoutProgram program);
    Task UpdateAsync(WorkoutProgram program);
    Task<bool> WorkoutNameExistsForUserAsync(Guid userId, string name);
}