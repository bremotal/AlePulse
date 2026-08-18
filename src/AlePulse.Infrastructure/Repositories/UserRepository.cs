using AlePulse.Application.Interfaces;
using AlePulse.Domain.Entities;
using AlePulse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlePulse.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AlePulseDbContext _context;

    public UserRepository(AlePulseDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
}