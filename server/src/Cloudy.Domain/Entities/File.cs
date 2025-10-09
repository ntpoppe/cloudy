using Cloudy.Domain.Exceptions;
using Cloudy.Domain.ValueObjects;
using Cloudy.Domain.Entities.Bases;

namespace Cloudy.Domain.Entities;

public class File : TrashableEntity, IBlobStorable
{
    public string Name { get; private set; }
    public long Size { get; private set; }
    public FileMetadata Metadata { get; private set; }
    public string Bucket { get; private set; }
    public string ObjectKey { get; private set; }
    public User User { get; private set; } = null!;

    // For EF Core, those properties shouldn't be null
    private File() 
    { 
        Name = null!;
        Metadata = null!;
        Bucket = null!;
        ObjectKey = null!;
    }
    
    public File(string name, long size, FileMetadata metadata, int userId)
    {
        Name = name;
        Size = size;
        Metadata = metadata;
        CreatedBy = userId;
        UpdatedBy = userId;
        Bucket = "cloudy";
        ObjectKey = Common.GenerateStorageKey(name);
    }

    public void Rename(string? newName, int userId)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new DomainException("Name cannot be empty.");
        Name = newName;
        Touch(userId);
    }

    public void SetStorage(string bucket, string objectKey)
    {
        Bucket = bucket;
        ObjectKey = objectKey;
    }
}
