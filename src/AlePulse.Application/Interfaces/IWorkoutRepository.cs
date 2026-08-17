using AlePulse.Domain.Entities;

namespace AlePulse.Application.Interfaces;

public interface IWorkoutRepository
{
    Task<Workout?> GetByIdAsync(Guid id);
    Task<IEnumerable<Workout>> GetAllByUserIdAsync(Guid userId);
    Task AddAsync(Workout workout);
    Task AddExerciseAsync(WorkoutExercise exercise);
    Task SaveChangesAsync();
    Task DeleteAsync(Workout workout);
    Task DeleteExerciseFromWorkoutAsync(WorkoutExercise exercise);
    Task UpdateExerciseAsync(WorkoutExercise exercise);
}