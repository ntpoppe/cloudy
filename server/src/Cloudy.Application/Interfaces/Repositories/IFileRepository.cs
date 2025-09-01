using Cloudy.Domain.Entities;

namespace Cloudy.Application.Interfaces;

public interface IFileRepository
{
    Task<Domain.Entities.File?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Domain.Entities.File>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<Domain.Entities.File>> GetByUserIdAsync(int userId, CancellationToken ct = default);
    Task AddAsync(Domain.Entities.File file, CancellationToken ct = default);
    void Update(Domain.Entities.File file);
}