namespace Cloudy.Application.DTOs;

public class FileDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public long Size { get; set; }
    public string ContentType { get; set; } = null!;
    public DateTime UploadedAt{ get; set; }
}