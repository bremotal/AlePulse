using System;
using System.Collections.Generic;
using System.Text;
using AlePulse.Domain.Entities;

namespace AlePulse.Application.Interfaces;

public interface IExerciseRepository
{
    Task<Exercise?> GetByIdAsync(Guid id);
    Task<IEnumerable<Exercise>> GetAllAsync();
    Task AddAsync(Exercise exercise);
    Task SaveChangesAsync();
}