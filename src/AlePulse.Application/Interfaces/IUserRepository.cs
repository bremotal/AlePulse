using System;
using System.Collections.Generic;
using System.Text;
using AlePulse.Domain.Entities;

namespace AlePulse.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
    Task SaveChangesAsync();
    Task UpdateUserAsync(User user);
}