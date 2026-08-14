using AlePulse.Application.DTOs;
using AlePulse.Application.Interfaces;
using AlePulse.Domain.Entities;
using AlePulse.Domain.Enums;
using AlePulse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlePulse.Infrastructure.Repositories;

public class WorkoutSessionRepository : IWorkoutSessionRepository
{
    private readonly AlePulseDbContext _context;

    public WorkoutSessionRepository(AlePulseDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ExerciseSet>> GetHistoryByExerciseAsync(Guid userId, Guid exerciseId)
    {
        return await _context.ExerciseSets
            .Where(es => es.ExerciseId == exerciseId && es.WorkoutSession.UserId == userId && es.IsCompleted)
            .OrderByDescending(es => es.CompletedAt)
            .ToListAsync();
    }

    public async Task LogSetAsync(Guid userId, Guid workoutId, Guid exerciseId, LogSetDto dto)
    {
        var session = await _context.WorkoutSessions
            .FirstOrDefaultAsync(s => s.WorkoutId == workoutId && s.UserId == userId && s.StartedAt.Date == DateTime.Now);

        if (session == null)
        {
            session = new WorkoutSession
            {
                UserId = userId,
                WorkoutId = workoutId,
                StartedAt = DateTime.Now.Date,
                Status = SessionStatus.InProgress
            };
            await _context.WorkoutSessions.AddAsync(session);
            await _context.SaveChangesAsync();
        }

        var set = new ExerciseSet
        {
            WorkoutSessionId = session.Id,
            ExerciseId = exerciseId,
            SetNumber = dto.SetNumber,
            Weight = dto.Weight,
            Repetitions = dto.Repetitions,
            IsCompleted = true,
            CompletedAt = DateTime.Now.Date
        };

        await _context.ExerciseSets.AddAsync(set);
        await _context.SaveChangesAsync();
    }

    public async Task<ExerciseSet?> GetSetByIdAsync(Guid setId)
    {
        return await _context.ExerciseSets
            .FirstOrDefaultAsync(es => es.Id == setId);
    }

    public async Task UpdateSetAsync(Guid setId, int setNumber, decimal weight, int repetitions)
    {
        var set = await _context.ExerciseSets.FindAsync(setId);
        if (set != null)
        {
            set.SetNumber = setNumber;
            set.Weight = weight;
            set.Repetitions = repetitions;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteSetAsync(Guid setId)
    {
        var set = await _context.ExerciseSets.FindAsync(setId);
        if (set != null)
        {
            _context.ExerciseSets.Remove(set);
            await _context.SaveChangesAsync();
        }
    }
}