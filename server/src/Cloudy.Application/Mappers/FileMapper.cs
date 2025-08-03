using Cloudy.Domain.Entities;
using Cloudy.Application.DTOs;

namespace Cloudy.Application.Mappers;

public static class FileMapper
{
    public static FileDto MapDto(Domain.Entities.File file)
        => new FileDto(
            file.Id,
            file.Name,
            file.Size,
            file.Metadata.ContentType,
            file.Metadata.UploadedAt
        );
}