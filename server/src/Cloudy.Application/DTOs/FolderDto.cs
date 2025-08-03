namespace Cloudy.Application.DTOs;

public class FolderDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int? ParentFolderId { get; set; }
    public DateTime CreatedAt { get; set; }
}