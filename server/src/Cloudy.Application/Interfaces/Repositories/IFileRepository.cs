namespace Cloudy.Application.Interfaces.Repositories;

public interface IFileRepository
{
    Task<Domain.Entities.File?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Domain.Entities.File>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<Domain.Entities.File>> GetByUserIdAsync(int userId, CancellationToken ct = default);
    Task AddAsync(Domain.Entities.File file, CancellationToken ct = default);
    void Update(Domain.Entities.File file);
    Task<long> GetTotalStorageUsedByUserAsync(int userId, CancellationToken ct = default);

}
