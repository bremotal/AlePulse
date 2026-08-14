using AlePulse.Application.Interfaces;
using AlePulse.Domain.Entities;
using AlePulse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlePulse.Infrastructure.Repositories;

public class WorkoutRepository : IWorkoutRepository
{
    private readonly AlePulseDbContext _context;

    public WorkoutRepository(AlePulseDbContext context)
    {
        _context = context;
    }

    public async Task<Workout?> GetByIdAsync(Guid id)
    {
        return await _context.Workouts
            .Include(w => w.Exercises) // Traz os exercícios do treino
                .ThenInclude(we => we.Exercise) // Traz os detalhes do exercício
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<IEnumerable<Workout>> GetAllByUserIdAsync(Guid userId)
    {
        return await _context.Workouts
            .Where(w => w.UserId == userId && w.IsActive)
            .ToListAsync();
    }

    public async Task AddAsync(Workout workout)
    {
        await _context.Workouts.AddAsync(workout);
    }

    public async Task AddExerciseAsync(WorkoutExercise exercise)
    {
        await _context.WorkoutExercises.AddAsync(exercise);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
    public async Task DeleteAsync(Workout workout)
    {
        // Exclusão lógica (Soft Delete) - some da tela mas mantém o histórico no banco
        workout.IsActive = false;
        _context.Workouts.Update(workout);
    }
    public async Task DeleteExerciseFromWorkoutAsync(WorkoutExercise exercise)
    {
        _context.WorkoutExercises.Remove(exercise);
    }
}