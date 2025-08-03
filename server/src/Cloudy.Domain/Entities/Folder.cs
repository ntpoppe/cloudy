using Cloudy.Domain.Exceptions;

namespace Cloudy.Domain.Entities;

public class Folder : TrashableEntity
{
    public string Name { get; private set; }
    public int? ParentFolderId { get; private set; }

    // For EF Core, those properties shouldn't be null
    private Folder()
    {
        Name = null!;
    }

    public Folder(string name, int? parentFolderId = null)
    {
        Name = name;
        ParentFolderId = parentFolderId;
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new DomainException("Name cannot be empty.");
        Name = newName;
    }
}