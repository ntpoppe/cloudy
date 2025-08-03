using Cloudy.Domain.Entities;
using Cloudy.Application.DTOs;

namespace Cloudy.Application.Mappers;

public static class FileMapper
{
    public static FileDto MapDto(Domain.Entities.File file)
        => new FileDto() 
        {
            Id = file.Id,
            Name = file.Name,
            Size = file.Size,
            ContentType = file.Metadata.ContentType,
            UploadedAt = file.Metadata.UploadedAt
        };
}