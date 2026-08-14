using AlePulse.Application.DTOs;
using AlePulse.Domain.Entities;

namespace AlePulse.Application.Interfaces;

public interface IWorkoutSessionRepository
{
    Task<IEnumerable<ExerciseSet>> GetHistoryByExerciseAsync(Guid userId, Guid exerciseId);
    Task LogSetAsync(Guid userId, Guid workoutId, Guid exerciseId, LogSetDto dto);
    Task<ExerciseSet?> GetSetByIdAsync(Guid setId);

    // Assinaturas atualizadas para bater com a implementação:
    Task UpdateSetAsync(Guid setId, int setNumber, decimal weight, int repetitions);
    Task DeleteSetAsync(Guid setId);
}