namespace Cloudy.Domain.ValueObjects;

public sealed class FileMetadata : IEquatable<FileMetadata>
{
    public string ContentType { get; }
    public DateTime UploadedAt  { get; }

    public FileMetadata(string contentType, DateTime uploadedAt)
    {
        ContentType = contentType;
        UploadedAt  = uploadedAt;
    }

    // override equality (value‐object semantics)
    public bool Equals(FileMetadata? other)
        => other is not null
           && other.ContentType == ContentType
           && other.UploadedAt == UploadedAt;

    public override bool Equals(object? obj) => Equals(obj as FileMetadata);

    public override int GetHashCode() => HashCode.Combine(ContentType, UploadedAt);

    public static bool operator ==(FileMetadata? left, FileMetadata? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(FileMetadata? left, FileMetadata? right) =>
        !(left == right);
}