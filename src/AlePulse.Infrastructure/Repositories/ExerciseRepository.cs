using AlePulse.Application.Interfaces;
using AlePulse.Domain.Entities;
using AlePulse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlePulse.Infrastructure.Repositories;

public class ExerciseRepository : IExerciseRepository
{
    private readonly AlePulseDbContext _context;

    public ExerciseRepository(AlePulseDbContext context)
    {
        _context = context;
    }

    public async Task<Exercise?> GetByIdAsync(Guid id)
    {
        return await _context.Exercises
            .Include(e => e.Medias) // Traz as mídias (GIFs/Vídeos) junto
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Exercise>> GetAllAsync()
    {
        return await _context.Exercises
            .Include(e => e.Medias)
            .Where(e => e.IsActive)
            .ToListAsync();
    }

    public async Task AddAsync(Exercise exercise)
    {
        await _context.Exercises.AddAsync(exercise);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}