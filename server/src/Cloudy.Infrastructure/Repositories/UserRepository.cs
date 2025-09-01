using Cloudy.Application.Interfaces;
using Cloudy.Domain.Entities;
using Cloudy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cloudy.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly CloudyDbContext _context;
    public UserRepository(CloudyDbContext context) => _context = context;

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) 
        => await _context.Users.SingleOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _context.Users.SingleOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        => await _context.Users.SingleOrDefaultAsync(u => u.Username == username, cancellationToken);

    public async Task<User?> GetByUsernameOrEmailAsync(string input, CancellationToken cancellationToken = default)
        => await GetByEmailAsync(input, cancellationToken) ?? await GetByUsernameAsync(input, cancellationToken);
}