namespace Cloudy.Domain.Entities;

public interface IBlobStorable
{
    public string Bucket { get; }
    public string ObjectKey { get; }
}