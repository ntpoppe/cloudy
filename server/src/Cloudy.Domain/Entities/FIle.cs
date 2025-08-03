using Cloudy.Domain.Exceptions;
using Cloudy.Domain.ValueObjects;

namespace Cloudy.Domain.Entities;

public class File : TrashableEntity
{
    public string Name { get; private set; }
    public long Size { get; private set; }
    public FileMetadata Metadata { get; private set; }

    // For EF Core, those properties shouldn't be null
    private File() 
    { 
        Name = null!;
        Metadata = null!;
    }

    public File(string name, long size, FileMetadata metadata)
    {
        Name = name;
        Size = size;
        Metadata = metadata;
    }

    public void Rename(string? newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new DomainException("Name cannot be empty.");
        Name = newName;
    }
}
