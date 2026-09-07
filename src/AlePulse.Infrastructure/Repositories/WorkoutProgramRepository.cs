using AlePulse.Application.Interfaces;
using AlePulse.Domain.Entities;
using AlePulse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlePulse.Infrastructure.Repositories;

public class WorkoutProgramRepository : IWorkoutProgramRepository
{
    private readonly AlePulseDbContext _context;

    public WorkoutProgramRepository(AlePulseDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkoutProgram>> GetAllByUserIdAsync(Guid userId)
    {
        return await _context.WorkoutPrograms
            .Where(p => p.UserId == userId && p.IsActive)
            .OrderByDescending(p => p.CreatedAt) // Ordena do mais novo para o mais velho
            .ToListAsync();
    }

    public async Task<WorkoutProgram?> GetByIdAsync(Guid id)
    {
        return await _context.WorkoutPrograms
            .Include(p => p.Workouts)
                .ThenInclude(w => w.Exercises)
                    .ThenInclude(we => we.Exercise)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddAsync(WorkoutProgram program)
    {
        await _context.WorkoutPrograms.AddAsync(program);
    }

    public async Task AddWorkoutAsync(Workout workout)
    {
        await _context.Workouts.AddAsync(workout);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
    public async Task DeleteAsync(WorkoutProgram program)
    {
        program.IsActive = false;
        _context.WorkoutPrograms.Update(program);
    }

    public async Task UpdateAsync(WorkoutProgram program)
    {
        var existing = await _context.WorkoutPrograms.FindAsync(program.Id);
        if (existing != null)
        {
            existing.Name = program.Name;
            existing.Description = program.Description;
            await _context.SaveChangesAsync();
        }
    }
    public async Task<bool> WorkoutNameExistsForUserAsync(Guid userId, string name)
    {
        // Verifica se já existe um treino com esse nome (ignorando maiúsculas/minúsculas) para o usuário
        return await _context.Workouts
            .AnyAsync(w => w.UserId == userId && w.Name.ToLower() == name.ToLower());
    }
}