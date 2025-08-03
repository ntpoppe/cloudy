namespace Cloudy.Application.Interfaces;

public interface IFileRepository
{
    Task<Domain.Entities.File?> GetByIdAsync(int id);
    Task AddAsync(Domain.Entities.File file);
    void Update(Domain.Entities.File file);
}